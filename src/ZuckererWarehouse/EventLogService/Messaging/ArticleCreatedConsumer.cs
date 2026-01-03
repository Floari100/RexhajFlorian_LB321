using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;


namespace EventLogService.Messaging;

public class ArticleCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<ArticleCreatedConsumer> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    public ArticleCreatedConsumer(IConfiguration config, ILogger<ArticleCreatedConsumer> logger)
    {
        _config = config;
        _logger = logger;
    }

    

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RABBITMQ_HOST"] ?? "MessageQueue",
            Port = int.TryParse(_config["RABBITMQ_PORT"], out var p) ? p : 5672,
            UserName = _config["RABBITMQ_USER"] ?? "guest",
            Password = _config["RABBITMQ_PASS"] ?? "guest",
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Topic Exchange (Pub/Sub)
        _channel.ExchangeDeclare("zuckerer.events", ExchangeType.Topic, durable: true);

        // Queue für EventLogService
        _channel.QueueDeclare(queue: "eventlog.q", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: "eventlog.q", exchange: "zuckerer.events", routingKey: "article.*");

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null) return Task.CompletedTask;

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (_, ea) =>
        {
            var msg = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Received event {RoutingKey}: {Message}", ea.RoutingKey, msg);
            try
            {
                // msg verarbeiten
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message rk={RoutingKey}", ea.RoutingKey);
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: "eventlog.q", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventLogService.Messaging;

public class RabbitMqSubscriber : BackgroundService
{
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqSubscriber(IConfiguration config)
    {
        _config = config;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RABBITMQ_HOST"] ?? "MessageQueue",
            Port = int.TryParse(_config["RABBITMQ_PORT"], out var p) ? p : 5672,
            UserName = _config["RABBITMQ_USER"] ?? "guest",
            Password = _config["RABBITMQ_PASS"] ?? "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Exchange (Topic)
        _channel.ExchangeDeclare("zuckerer.events", ExchangeType.Topic, durable: true);

        // Queue + Binding
        _channel.QueueDeclare(queue: "eventlog.q", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: "eventlog.q", exchange: "zuckerer.events", routingKey: "article.*");

        // Prefetch (sauber für Consumer)
        _channel.BasicQos(0, 10, false);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null) return Task.CompletedTask;

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);

                Console.WriteLine($"[EventLog] routingKey={ea.RoutingKey} msg={msg}");

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch
            {
                // Requeue true, wenn du später Retry willst – für LB reicht meist false + Log
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }

            await Task.CompletedTask;
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

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ArticleService.Messaging;

public class RabbitMqPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqPublisher(IConfiguration config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RABBITMQ_HOST"] ?? "MessageQueue",
            Port = int.TryParse(config["RABBITMQ_PORT"], out var p) ? p : 5672,
            UserName = config["RABBITMQ_USER"] ?? "guest",
            Password = config["RABBITMQ_PASS"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare("zuckerer.events", ExchangeType.Topic, durable: true);
    }

    public void Publish<T>(string routingKey, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;

        _channel.BasicPublish("zuckerer.events", routingKey, props, body);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}

{{if .IncludeMessageQueue}}
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message);
    Task PublishAsync(string queueName, string message);
}

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(IConnection connection, ILogger<RabbitMQPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        var json = JsonSerializer.Serialize(message);
        await PublishAsync(queueName, json);
    }

    public async Task PublishAsync(string queueName, string message)
    {
        try
        {
            using var channel = await _connection.CreateChannelAsync();
            
            // Declare queue (create if doesn't exist)
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Published message to queue {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            throw;
        }
    }
}
{{end}}
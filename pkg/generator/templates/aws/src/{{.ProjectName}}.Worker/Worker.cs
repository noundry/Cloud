{{if .IncludeWorker}}
using Microsoft.EntityFrameworkCore;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing {WorkerName}", nameof(Worker));
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        {{if ne .Database ""}}
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Example: Clean up old records
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var oldUsers = await dbContext.Users
            .Where(u => u.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken);
            
        if (oldUsers.Any())
        {
            _logger.LogInformation("Found {Count} old users to clean up", oldUsers.Count);
            // Uncomment to actually delete
            // dbContext.Users.RemoveRange(oldUsers);
            // await dbContext.SaveChangesAsync(cancellationToken);
        }
        {{end}}

        {{if .IncludeCache}}
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var database = redis.GetDatabase();
        
        // Example: Update cache with system statistics
        var stats = new
        {
            Timestamp = DateTime.UtcNow,
            WorkerStatus = "Running",
            {{if ne .Database ""}}
            UserCount = await dbContext.Users.CountAsync(cancellationToken)
            {{else}}
            UserCount = 0
            {{end}}
        };
        
        await database.StringSetAsync("system:stats", System.Text.Json.JsonSerializer.Serialize(stats), TimeSpan.FromHours(1));
        {{end}}

        _logger.LogInformation("Worker ran at: {time}", DateTimeOffset.Now);
    }
}

{{if .IncludeMessageQueue}}
public class MessageConsumerService : BackgroundService
{
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;

    public MessageConsumerService(ILogger<MessageConsumerService> logger, IServiceProvider serviceProvider, IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = await _connection.CreateChannelAsync();
        
        var queueName = "worker-queue";
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                
                _logger.LogInformation("Received message: {Message}", message);
                
                // Process message here
                await ProcessMessageAsync(message);
                
                // Acknowledge message
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                // Reject message and requeue
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        
        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);
        
        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
    
    private async Task ProcessMessageAsync(string message)
    {
        using var scope = _serviceProvider.CreateScope();
        
        // Example message processing
        await Task.Delay(100); // Simulate work
        
        _logger.LogInformation("Processed message: {Message}", message);
    }
}

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public class RabbitMQConsumer : IMessageConsumer
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(IConnection connection, ILogger<RabbitMQConsumer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message consumer started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message consumer stopped");
        return Task.CompletedTask;
    }
}
{{end}}

{{if ne .Database ""}}
// Shared database context (should be in a shared project in real apps)
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
{{end}}
{{end}}
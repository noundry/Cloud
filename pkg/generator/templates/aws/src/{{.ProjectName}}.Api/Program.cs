using Microsoft.EntityFrameworkCore;
{{if .IncludeJobs}}using Hangfire;{{end}}

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

{{if ne .Database ""}}
// Add Entity Framework
{{if eq .Database "PostgreSQL"}}
builder.AddNpgsqlDbContext<ApplicationDbContext>("database");
{{else if eq .Database "MySQL"}}
builder.AddMySqlDbContext<ApplicationDbContext>("database");
{{else if eq .Database "SqlServer"}}
builder.AddSqlServerDbContext<ApplicationDbContext>("database");
{{end}}
{{end}}

{{if .IncludeCache}}
// Add Redis
builder.AddRedis("redis");
{{end}}

{{if .IncludeStorage}}
// Add S3 Storage
builder.Services.Configure<S3Options>(builder.Configuration.GetSection("S3"));
builder.Services.AddSingleton<IStorageService, S3StorageService>();
{{end}}

{{if .IncludeMail}}
// Add Email
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailService, EmailService>();
{{end}}

{{if .IncludeMessageQueue}}
// Add RabbitMQ
builder.AddRabbitMQ("rabbitmq");
builder.Services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
{{end}}

{{if .IncludeJobs}}
// Add Hangfire
{{if eq .Database "PostgreSQL"}}
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("database")));
{{else if eq .Database "MySQL"}}
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(builder.Configuration.GetConnectionString("database"))));
{{else if eq .Database "SqlServer"}}
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("database")));
{{end}}
builder.Services.AddHangfireServer();
{{end}}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

{{if .IncludeJobs}}
app.UseHangfireDashboard();
{{end}}

app.UseHttpsRedirection();

// API Endpoints
app.MapGet("/", () => TypedResults.Ok(new { 
    Message = "Hello from {{.ProjectName}}!",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
}))
.WithName("Root")
.WithOpenApi();

{{if ne .Database ""}}
// Database endpoints
app.MapGet("/users", async (ApplicationDbContext db) => 
    await db.Users.ToListAsync())
.WithName("GetUsers")
.WithOpenApi();

app.MapPost("/users", async (CreateUserRequest request, ApplicationDbContext db) => 
{
    var user = new User { Name = request.Name, Email = request.Email };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser")
.WithOpenApi();
{{end}}

{{if .IncludeCache}}
// Cache endpoints
app.MapGet("/cache/{key}", async (string key, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var value = await db.StringGetAsync(key);
    return value.HasValue ? TypedResults.Ok(value.ToString()) : TypedResults.NotFound();
})
.WithName("GetFromCache")
.WithOpenApi();

app.MapPost("/cache", async (CacheRequest request, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    await db.StringSetAsync(request.Key, request.Value, TimeSpan.FromMinutes(5));
    return TypedResults.Ok();
})
.WithName("SetCache")
.WithOpenApi();
{{end}}

{{if .IncludeStorage}}
// Storage endpoints
app.MapGet("/files/{fileName}", async (string fileName, IStorageService storage) =>
{
    var fileBytes = await storage.GetFileAsync(fileName);
    return fileBytes != null ? TypedResults.File(fileBytes, "application/octet-stream", fileName) : TypedResults.NotFound();
})
.WithName("GetFile")
.WithOpenApi();

app.MapPost("/files", async (IFormFile file, IStorageService storage) =>
{
    if (file.Length > 0)
    {
        using var stream = file.OpenReadStream();
        var fileName = await storage.SaveFileAsync(file.FileName, stream);
        return TypedResults.Ok(new { FileName = fileName });
    }
    return TypedResults.BadRequest();
})
.WithName("UploadFile")
.WithOpenApi();
{{end}}

{{if .IncludeMail}}
// Email endpoints
app.MapPost("/email/send", async (SendEmailRequest request, IEmailService email) =>
{
    await email.SendEmailAsync(request.To, request.Subject, request.Body);
    return TypedResults.Ok();
})
.WithName("SendEmail")
.WithOpenApi();
{{end}}

{{if .IncludeMessageQueue}}
// Message queue endpoints
app.MapPost("/messages/publish", async (PublishMessageRequest request, IMessagePublisher publisher) =>
{
    await publisher.PublishAsync(request.Queue, request.Message);
    return TypedResults.Ok();
})
.WithName("PublishMessage")
.WithOpenApi();
{{end}}

{{if .IncludeJobs}}
// Job endpoints
app.MapPost("/jobs/enqueue", (EnqueueJobRequest request) =>
{
    var jobId = BackgroundJob.Enqueue(() => ProcessJob(request.Data));
    return TypedResults.Ok(new { JobId = jobId });
})
.WithName("EnqueueJob")
.WithOpenApi();

app.MapPost("/jobs/schedule", (ScheduleJobRequest request) =>
{
    var jobId = BackgroundJob.Schedule(() => ProcessJob(request.Data), TimeSpan.FromMinutes(request.DelayMinutes));
    return TypedResults.Ok(new { JobId = jobId });
})
.WithName("ScheduleJob")
.WithOpenApi();

// Sample job method
static void ProcessJob(string data)
{
    // Simulate job processing
    Thread.Sleep(1000);
    Console.WriteLine($"Processed job with data: {data}");
}
{{end}}

{{if ne .Database ""}}
// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}
{{end}}

app.Run();

{{if ne .Database ""}}
// Database models
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

public record CreateUserRequest(string Name, string Email);
{{end}}

{{if .IncludeCache}}
public record CacheRequest(string Key, string Value);
{{end}}

{{if .IncludeMail}}
public record SendEmailRequest(string To, string Subject, string Body);
{{end}}

{{if .IncludeMessageQueue}}
public record PublishMessageRequest(string Queue, string Message);
{{end}}

{{if .IncludeJobs}}
public record EnqueueJobRequest(string Data);
public record ScheduleJobRequest(string Data, int DelayMinutes);
{{end}}
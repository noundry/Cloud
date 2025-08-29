using Microsoft.EntityFrameworkCore;
using Company.WebApplication1.Api.Services;
using Company.WebApplication1.Api.Data;
#if (IncludeJobs)
using Hangfire;
#endif

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Add configured services based on appsettings
builder.Services.AddConfiguredServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

#if (IncludeJobs)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = app.Environment.IsDevelopment() ? 
        Array.Empty<IDashboardAuthorizationFilter>() : 
        new[] { new HangfireAuthorizationFilter() }
});
#endif

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready"); // Readiness probe

// API Endpoints
app.MapGet("/", () => Results.Ok(new { 
    Message = "Hello from Company.WebApplication1!",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0"
}))
.WithName("Root")
.WithOpenApi();

#if (HasDatabase)
// Database endpoints
app.MapGet("/users", async (AppDbContext db) => 
    await db.Users.ToListAsync())
.WithName("GetUsers")
.WithOpenApi();

app.MapPost("/users", async (CreateUserRequest request, AppDbContext db) => 
{
    var user = new User { Name = request.Name, Email = request.Email };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser")
.WithOpenApi();

app.MapGet("/users/{id:int}", async (int id, AppDbContext db) =>
    await db.Users.FindAsync(id) is User user ? Results.Ok(user) : Results.NotFound())
.WithName("GetUser")
.WithOpenApi();
#endif

#if (IncludeCache)
// Cache endpoints
app.MapGet("/cache/{key}", async (string key, ICacheService cache) =>
{
    var value = await cache.GetAsync(key);
    return value != null ? Results.Ok(value) : Results.NotFound();
})
.WithName("GetFromCache")
.WithOpenApi();

app.MapPost("/cache", async (SetCacheRequest request, ICacheService cache) =>
{
    await cache.SetAsync(request.Key, request.Value, TimeSpan.FromMinutes(request.ExpirationMinutes ?? 60));
    return Results.Ok();
})
.WithName("SetCache")
.WithOpenApi();
#endif

#if (IncludeStorage)
// File storage endpoints
app.MapGet("/files/{fileName}", async (string fileName, IStorageService storage) =>
{
    var fileStream = await storage.GetFileAsync(fileName);
    return fileStream != null ? 
        Results.Stream(fileStream, "application/octet-stream", fileName) : 
        Results.NotFound();
})
.WithName("GetFile")
.WithOpenApi();

app.MapPost("/files", async (IFormFile file, IStorageService storage) =>
{
    if (file.Length > 0)
    {
        using var stream = file.OpenReadStream();
        var fileName = await storage.SaveFileAsync(file.FileName, stream);
        return Results.Ok(new { FileName = fileName, Size = file.Length });
    }
    return Results.BadRequest("No file uploaded");
})
.WithName("UploadFile")
.WithOpenApi()
.DisableAntiforgery();
#endif

#if (IncludeMail)
// Email endpoints
app.MapPost("/email/send", async (SendEmailRequest request, IEmailService email) =>
{
    await email.SendEmailAsync(request.To, request.Subject, request.Body, request.IsHtml);
    return Results.Ok(new { Message = "Email sent successfully" });
})
.WithName("SendEmail")
.WithOpenApi();
#endif

#if (IncludeQueue)
// Message queue endpoints
app.MapPost("/messages/publish", async (PublishMessageRequest request, IMessageService messageService) =>
{
    await messageService.PublishAsync(request.Queue, request.Message);
    return Results.Ok(new { Message = "Message published successfully" });
})
.WithName("PublishMessage")
.WithOpenApi();
#endif

#if (IncludeJobs)
// Background job endpoints
app.MapPost("/jobs/enqueue", (EnqueueJobRequest request) =>
{
    var jobId = BackgroundJob.Enqueue(() => ProcessJob(request.Data));
    return Results.Ok(new { JobId = jobId, Status = "Enqueued" });
})
.WithName("EnqueueJob")
.WithOpenApi();

app.MapPost("/jobs/schedule", (ScheduleJobRequest request) =>
{
    var jobId = BackgroundJob.Schedule(() => ProcessJob(request.Data), TimeSpan.FromMinutes(request.DelayMinutes));
    return Results.Ok(new { JobId = jobId, Status = "Scheduled" });
})
.WithName("ScheduleJob")
.WithOpenApi();

// Sample job processing method
static void ProcessJob(string data)
{
    // Simulate processing
    Thread.Sleep(TimeSpan.FromSeconds(2));
    Console.WriteLine($"Processed job with data: {data}");
}
#endif

#if (HasDatabase)
// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment())
    {
        await context.Database.EnsureCreatedAsync();
    }
    else
    {
        await context.Database.MigrateAsync();
    }
}
#endif

app.Run();

#if (HasDatabase)
// Database models and DTOs
public record CreateUserRequest(string Name, string Email);
#endif

#if (IncludeCache)
public record SetCacheRequest(string Key, string Value, int? ExpirationMinutes);
#endif

#if (IncludeMail)
public record SendEmailRequest(string To, string Subject, string Body, bool IsHtml = false);
#endif

#if (IncludeQueue)
public record PublishMessageRequest(string Queue, string Message);
#endif

#if (IncludeJobs)
public record EnqueueJobRequest(string Data);
public record ScheduleJobRequest(string Data, int DelayMinutes);

#if (IncludeJobs)
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, implement proper authorization
        return true; // TODO: Implement proper authorization
    }
}
#endif
#endif
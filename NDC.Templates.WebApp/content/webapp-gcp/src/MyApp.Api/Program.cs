using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add services
builder.Services.AddOpenApi();

// Database
builder.AddNpgsqlDbContext<AppDbContext>("database");

// Cache  
builder.AddRedis("redis");

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// API Endpoints
app.MapGet("/", () => Results.Ok(new { 
    Message = "Hello from MyApp!",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow 
}))
.WithName("Root")
.WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy",
    Timestamp = DateTime.UtcNow 
}))
.WithName("Health")
.WithOpenApi();

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

// Cache endpoints
app.MapGet("/cache/{key}", async (string key, IConnectionMultiplexer redis) =>
{
    var database = redis.GetDatabase();
    var value = await database.StringGetAsync(key);
    return value.HasValue ? Results.Ok(new { Key = key, Value = value.ToString() }) : Results.NotFound();
})
.WithName("GetCache")
.WithOpenApi();

app.MapPost("/cache", async (SetCacheRequest request, IConnectionMultiplexer redis) =>
{
    var database = redis.GetDatabase();
    await database.StringSetAsync(request.Key, request.Value, TimeSpan.FromMinutes(5));
    return Results.Ok(new { Message = "Cached successfully" });
})
.WithName("SetCache")
.WithOpenApi();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();

// Database context
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// DTOs
public record CreateUserRequest(string Name, string Email);
public record SetCacheRequest(string Key, string Value);
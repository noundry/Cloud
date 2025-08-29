var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return TypedResults.Ok("Hello from {{.ProjectName}}!");
})
.WithName("{{.ProjectName}}");

app.MapGet("/health", () => Results.Ok(new { 
    status = "ok", 
    service = "{{.ProjectName}}",
    timestamp = DateTime.UtcNow 
}));

app.Run();
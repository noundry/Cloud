{{if .IncludeWorker}}
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

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

{{if .IncludeMessageQueue}}
// Add RabbitMQ
builder.AddRabbitMQ("rabbitmq");
builder.Services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();
{{end}}

// Add background services
builder.Services.AddHostedService<Worker>();
{{if .IncludeMessageQueue}}
builder.Services.AddHostedService<MessageConsumerService>();
{{end}}

var host = builder.Build();
host.Run();
{{end}}
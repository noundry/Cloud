var builder = DistributedApplication.CreateBuilder(args);

// Database
{{if eq .Database "PostgreSQL"}}
var postgres = builder.AddPostgreSQL("postgres")
    .WithEnvironment("POSTGRES_DB", "{{.ServiceName}}")
    .WithLifetime(ContainerLifetime.Persistent);
var database = postgres.AddDatabase("database");
{{else if eq .Database "MySQL"}}
var mysql = builder.AddMySQL("mysql")
    .WithEnvironment("MYSQL_DATABASE", "{{.ServiceName}}")
    .WithLifetime(ContainerLifetime.Persistent);
var database = mysql.AddDatabase("database");
{{else if eq .Database "SqlServer"}}
var sqlserver = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);
var database = sqlserver.AddDatabase("database");
{{end}}

{{if .IncludeCache}}
// Redis Cache
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);
{{end}}

{{if .IncludeStorage}}
// MinIO (S3-compatible storage)
var minio = builder.AddContainer("minio", "minio/minio")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithBindMount("./data/minio", "/data")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithHttpEndpoint(targetPort: 9000, name: "api")
    .WithHttpEndpoint(targetPort: 9001, name: "console")
    .WithLifetime(ContainerLifetime.Persistent);
{{end}}

{{if .IncludeMail}}
// MailHog (Local SMTP server)
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(targetPort: 8025, name: "web")
    .WithEndpoint(targetPort: 1025, name: "smtp")
    .WithLifetime(ContainerLifetime.Persistent);
{{end}}

{{if .IncludeMessageQueue}}
// RabbitMQ
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);
{{end}}

// API Application
var apiService = builder.AddProject<Projects.{{.ProjectName}}_Api>("api")
    .WithReference(database)
    {{if .IncludeCache}}.WithReference(redis){{end}}
    {{if .IncludeStorage}}.WithReference(minio){{end}}
    {{if .IncludeMail}}.WithReference(mailhog){{end}}
    {{if .IncludeMessageQueue}}.WithReference(rabbitmq){{end}}
    .WithExternalHttpEndpoints();

{{if .IncludeWorker}}
// Worker Service
builder.AddProject<Projects.{{.ProjectName}}_Worker>("worker")
    .WithReference(database)
    {{if .IncludeCache}}.WithReference(redis){{end}}
    {{if .IncludeStorage}}.WithReference(minio){{end}}
    {{if .IncludeMail}}.WithReference(mailhog){{end}}
    {{if .IncludeMessageQueue}}.WithReference(rabbitmq){{end}};
{{end}}

builder.Build().Run();
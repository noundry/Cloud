var builder = DistributedApplication.CreateBuilder(args);

// Database
var postgres = builder.AddPostgreSQL("postgres")
    .WithEnvironment("POSTGRES_DB", "myapp")
    .WithLifetime(ContainerLifetime.Persistent);
var database = postgres.AddDatabase("database");

// Cache
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Storage (MinIO - S3 compatible)
var minio = builder.AddContainer("minio", "minio/minio")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithBindMount("./data/minio", "/data")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithHttpEndpoint(targetPort: 9000, name: "api")
    .WithHttpEndpoint(targetPort: 9001, name: "console")
    .WithLifetime(ContainerLifetime.Persistent);

// Email (MailHog for local development)
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(targetPort: 8025, name: "web")
    .WithEndpoint(targetPort: 1025, name: "smtp")
    .WithLifetime(ContainerLifetime.Persistent);

// API Application
var apiService = builder.AddProject<Projects.MyApp_Api>("api")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(minio)
    .WithReference(mailhog)
    .WithExternalHttpEndpoints();

builder.Build().Run();
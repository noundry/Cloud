var builder = DistributedApplication.CreateBuilder(args);

#if (UsePostgreSQL)
var postgres = builder.AddPostgreSQL("postgres")
    .WithEnvironment("POSTGRES_DB", "Company.WebApplication1".ToLowerInvariant())
    .WithLifetime(ContainerLifetime.Persistent);
var database = postgres.AddDatabase("database");
#endif
#if (UseMySQL)
var mysql = builder.AddMySQL("mysql")
    .WithEnvironment("MYSQL_DATABASE", "Company.WebApplication1".ToLowerInvariant())
    .WithLifetime(ContainerLifetime.Persistent);
var database = mysql.AddDatabase("database");
#endif
#if (UseSqlServer)
var sqlserver = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);
var database = sqlserver.AddDatabase("database");
#endif

#if (IncludeCache)
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);
#endif

#if (IncludeStorage)
var minio = builder.AddContainer("minio", "minio/minio")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithBindMount("./data/minio", "/data")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithHttpEndpoint(targetPort: 9000, name: "api")
    .WithHttpEndpoint(targetPort: 9001, name: "console")
    .WithLifetime(ContainerLifetime.Persistent);
#endif

#if (IncludeMail)
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(targetPort: 8025, name: "web")
    .WithEndpoint(targetPort: 1025, name: "smtp")
    .WithLifetime(ContainerLifetime.Persistent);
#endif

#if (IncludeQueue)
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);
#endif

// API Application
var apiService = builder.AddProject<Projects.Company_WebApplication1_Api>("api")
#if (HasDatabase)
    .WithReference(database)
#endif
#if (IncludeCache)
    .WithReference(redis)
#endif
#if (IncludeStorage)
    .WithReference(minio)
#endif
#if (IncludeMail)
    .WithReference(mailhog)
#endif
#if (IncludeQueue)
    .WithReference(rabbitmq)
#endif
    .WithExternalHttpEndpoints();

#if (IncludeWorker)
// Worker Service
builder.AddProject<Projects.Company_WebApplication1_Worker>("worker")
#if (HasDatabase)
    .WithReference(database)
#endif
#if (IncludeCache)
    .WithReference(redis)
#endif
#if (IncludeStorage)
    .WithReference(minio)
#endif
#if (IncludeMail)
    .WithReference(mailhog)
#endif
#if (IncludeQueue)
    .WithReference(rabbitmq);
#else
    ;
#endif
#endif

builder.Build().Run();
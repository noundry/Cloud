using Microsoft.EntityFrameworkCore;
using Company.WebApplication1.Api.Data;
#if (IncludeCache)
using Company.WebApplication1.Api.Services;
#endif
#if (IncludeStorage)
using Amazon.S3;
#endif
#if (IncludeMail)
using Company.WebApplication1.Api.Services;
#endif
#if (IncludeQueue)
using Company.WebApplication1.Api.Services;
#endif
#if (IncludeJobs)
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.MySqlStorage;
using Hangfire.SqlServer;
#endif

namespace Company.WebApplication1.Api.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add health checks
        var healthChecks = services.AddHealthChecks();

#if (HasDatabase)
        // Database configuration
        AddDatabaseServices(services, configuration, healthChecks);
#endif

#if (IncludeCache)
        // Cache configuration
        AddCacheServices(services, configuration, healthChecks);
#endif

#if (IncludeStorage)
        // Storage configuration
        AddStorageServices(services, configuration, healthChecks);
#endif

#if (IncludeMail)
        // Email configuration
        AddEmailServices(services, configuration);
#endif

#if (IncludeQueue)
        // Message queue configuration
        AddMessageServices(services, configuration, healthChecks);
#endif

#if (IncludeJobs)
        // Background jobs configuration
        AddJobServices(services, configuration);
#endif

        return services;
    }

#if (HasDatabase)
    private static void AddDatabaseServices(IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecks)
    {
        var databaseType = configuration["Services:Database:Type"];
        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is required");
        }

        switch (databaseType?.ToUpperInvariant())
        {
#if (UsePostgreSQL)
            case "POSTGRESQL":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));
                healthChecks.AddNpgSql(connectionString, name: "database");
                break;
#endif
#if (UseMySQL)
            case "MYSQL":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                healthChecks.AddMySql(connectionString, name: "database");
                break;
#endif
#if (UseSqlServer)
            case "SQLSERVER":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
                healthChecks.AddSqlServer(connectionString, name: "database");
                break;
#endif
            default:
                throw new InvalidOperationException($"Unsupported database type: {databaseType}");
        }
    }
#endif

#if (IncludeCache)
    private static void AddCacheServices(IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecks)
    {
        var cacheEnabled = configuration.GetValue<bool>("Services:Cache:Enabled");
        if (!cacheEnabled) return;

        var connectionString = configuration.GetConnectionString("Redis");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback to in-memory cache
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
            healthChecks.AddRedis(connectionString, name: "redis");
        }
    }
#endif

#if (IncludeStorage)
    private static void AddStorageServices(IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecks)
    {
        var storageEnabled = configuration.GetValue<bool>("Services:Storage:Enabled");
        if (!storageEnabled) return;

        // Configure S3 client
        services.Configure<S3Options>(configuration.GetSection("S3"));
        
        var s3Config = configuration.GetSection("S3");
        var serviceUrl = s3Config["ServiceUrl"]; // For MinIO local development
        
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            // Local MinIO configuration
            services.AddSingleton<IAmazonS3>(provider =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = serviceUrl,
                    ForcePathStyle = true
                };
                return new AmazonS3Client(s3Config["AccessKey"], s3Config["SecretKey"], config);
            });
        }
        else
        {
            // AWS S3 configuration (uses default credentials)
            services.AddSingleton<IAmazonS3, AmazonS3Client>();
        }
        
        services.AddSingleton<IStorageService, S3StorageService>();
        healthChecks.AddCheck<S3HealthCheck>("s3");
    }
#endif

#if (IncludeMail)
    private static void AddEmailServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddSingleton<IEmailService, EmailService>();
    }
#endif

#if (IncludeQueue)
    private static void AddMessageServices(IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecks)
    {
        var queueType = configuration["Services:MessageQueue:Type"];
        
        switch (queueType?.ToUpperInvariant())
        {
            case "RABBITMQ":
                var rabbitConnectionString = configuration.GetConnectionString("RabbitMQ");
                services.Configure<RabbitMQOptions>(options =>
                {
                    options.ConnectionString = rabbitConnectionString ?? "amqp://guest:guest@localhost:5672/";
                });
                services.AddSingleton<IMessageService, RabbitMQService>();
                if (!string.IsNullOrEmpty(rabbitConnectionString))
                {
                    healthChecks.AddRabbitMQ(rabbitConnectionString, name: "rabbitmq");
                }
                break;
            case "MEMORY":
            default:
                services.AddSingleton<IMessageService, InMemoryMessageService>();
                break;
        }
    }
#endif

#if (IncludeJobs)
    private static void AddJobServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        var databaseType = configuration["Services:Database:Type"];

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            if (!string.IsNullOrEmpty(connectionString))
            {
                switch (databaseType?.ToUpperInvariant())
                {
#if (UsePostgreSQL)
                    case "POSTGRESQL":
                        config.UsePostgreSqlStorage(connectionString);
                        break;
#endif
#if (UseMySQL)
                    case "MYSQL":
                        config.UseStorage(new MySqlStorage(connectionString));
                        break;
#endif
#if (UseSqlServer)
                    case "SQLSERVER":
                        config.UseSqlServerStorage(connectionString);
                        break;
#endif
                }
            }
            else
            {
                config.UseMemoryStorage();
            }
        });

        services.AddHangfireServer();
    }
#endif
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Company.WebApplication1.Api.Data;
#if (IncludeJobs)
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Hangfire.Redis;
#endif

namespace Company.WebApplication1.Api.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add health checks
        var healthChecksBuilder = services.AddHealthChecks();

        // Configure database
        var databaseProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";
        ConfigureDatabase(services, configuration, healthChecksBuilder, databaseProvider);

        // Configure cache
        var cacheProvider = configuration["CacheProvider"] ?? "Redis";
        ConfigureCache(services, configuration, healthChecksBuilder, cacheProvider);

        // Configure storage
        var storageProvider = configuration["StorageProvider"] ?? "FileSystem";
        ConfigureStorage(services, configuration, storageProvider);

        // Configure email
        var mailProvider = configuration["MailProvider"] ?? "SMTP";
        ConfigureMail(services, configuration, mailProvider);

        // Configure message queue
        var queueProvider = configuration["QueueProvider"] ?? "InMemory";
        ConfigureQueue(services, configuration, queueProvider);

        // Configure background jobs
        ConfigureBackgroundJobs(services, configuration, databaseProvider, cacheProvider);

        return services;
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, 
        IHealthChecksBuilder healthChecksBuilder, string provider)
    {
#if (HasDatabase)
        var connectionString = configuration.GetConnectionString("Database");
        
        switch (provider.ToLowerInvariant())
        {
            case "postgresql":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));
                healthChecksBuilder.AddNpgSql(connectionString, name: "database");
                break;
                
            case "sqlserver":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
                healthChecksBuilder.AddSqlServer(connectionString, name: "database");
                break;
                
            case "mysql":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                healthChecksBuilder.AddMySql(connectionString, name: "database");
                break;
                
            case "sqlite":
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(connectionString));
                healthChecksBuilder.AddSqlite(connectionString, name: "database");
                break;
                
            default:
                throw new NotSupportedException($"Database provider '{provider}' is not supported");
        }
#endif
    }

    private static void ConfigureCache(IServiceCollection services, IConfiguration configuration, 
        IHealthChecksBuilder healthChecksBuilder, string provider)
    {
#if (IncludeCache)
        switch (provider.ToLowerInvariant())
        {
            case "redis":
                var redisConnectionString = configuration.GetConnectionString("Redis");
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
                services.AddScoped<ICacheService, RedisCacheService>();
                healthChecksBuilder.AddRedis(redisConnectionString, name: "redis");
                break;
                
            case "inmemory":
                services.AddMemoryCache();
                services.AddScoped<ICacheService, InMemoryCacheService>();
                break;
                
            default:
                services.AddMemoryCache();
                services.AddScoped<ICacheService, InMemoryCacheService>();
                break;
        }
#endif
    }

    private static void ConfigureStorage(IServiceCollection services, IConfiguration configuration, string provider)
    {
#if (IncludeStorage)
        switch (provider.ToLowerInvariant())
        {
            case "aws":
            case "awss3":
                services.Configure<AwsS3StorageOptions>(configuration.GetSection("AwsS3"));
                services.AddScoped<IStorageService, AwsS3StorageService>();
                break;
                
            case "azure":
            case "azureblobstorage":
                services.Configure<AzureBlobStorageOptions>(configuration.GetSection("AzureBlobStorage"));
                services.AddScoped<IStorageService, AzureBlobStorageService>();
                break;
                
            case "gcp":
            case "googlecloudstorage":
                services.Configure<GoogleCloudStorageOptions>(configuration.GetSection("GoogleCloudStorage"));
                services.AddScoped<IStorageService, GoogleCloudStorageService>();
                break;
                
            case "filesystem":
            default:
                services.Configure<FileSystemStorageOptions>(configuration.GetSection("FileSystemStorage"));
                services.AddScoped<IStorageService, FileSystemStorageService>();
                break;
        }
#endif
    }

    private static void ConfigureMail(IServiceCollection services, IConfiguration configuration, string provider)
    {
#if (IncludeMail)
        switch (provider.ToLowerInvariant())
        {
            case "aws":
            case "awsses":
                services.Configure<AwsSesOptions>(configuration.GetSection("AwsSes"));
                services.AddScoped<IEmailService, AwsSesEmailService>();
                break;
                
            case "sendgrid":
                services.Configure<SendGridOptions>(configuration.GetSection("SendGrid"));
                services.AddScoped<IEmailService, SendGridEmailService>();
                break;
                
            case "smtp":
            default:
                services.Configure<SmtpOptions>(configuration.GetSection("SmtpSettings"));
                services.AddScoped<IEmailService, SmtpEmailService>();
                break;
        }
#endif
    }

    private static void ConfigureQueue(IServiceCollection services, IConfiguration configuration, string provider)
    {
#if (IncludeQueue)
        switch (provider.ToLowerInvariant())
        {
            case "aws":
            case "awssqs":
                services.Configure<AwsSqsOptions>(configuration.GetSection("AwsSqs"));
                services.AddScoped<IMessageService, AwsSqsMessageService>();
                break;
                
            case "azure":
            case "azureservicebus":
                services.Configure<AzureServiceBusOptions>(configuration.GetSection("AzureServiceBus"));
                services.AddScoped<IMessageService, AzureServiceBusMessageService>();
                break;
                
            case "gcp":
            case "googlecloudpubsub":
                services.Configure<GoogleCloudPubSubOptions>(configuration.GetSection("GoogleCloudPubSub"));
                services.AddScoped<IMessageService, GoogleCloudPubSubMessageService>();
                break;
                
            case "rabbitmq":
                services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
                services.AddScoped<IMessageService, RabbitMqMessageService>();
                break;
                
            case "inmemory":
            default:
                services.AddScoped<IMessageService, InMemoryMessageService>();
                break;
        }
#endif
    }

    private static void ConfigureBackgroundJobs(IServiceCollection services, IConfiguration configuration, 
        string databaseProvider, string cacheProvider)
    {
#if (IncludeJobs)
        // Configure Hangfire based on available infrastructure
        if (databaseProvider.ToLowerInvariant() == "postgresql")
        {
            var connectionString = configuration.GetConnectionString("Database");
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(connectionString));
        }
        else if (databaseProvider.ToLowerInvariant() == "sqlserver")
        {
            var connectionString = configuration.GetConnectionString("Database");
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString));
        }
        else if (cacheProvider.ToLowerInvariant() == "redis")
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseRedisStorage(redisConnectionString));
        }
        else
        {
            // Fallback to in-memory storage
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage());
        }

        services.AddHangfireServer();
#endif
    }
}
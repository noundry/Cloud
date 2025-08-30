using System.Text.Json;
using Microsoft.Extensions.Logging;
using NDC.Cli.Models;

namespace NDC.Cli.Services;

public class AspireService : IAspireService
{
    private readonly ILogger<AspireService> _logger;

    public AspireService(ILogger<AspireService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<CloudServiceMapping>> GetServiceMappingsAsync(string cloudProvider, ServiceConfiguration services)
    {
        var mappings = new List<CloudServiceMapping>();

        if (services.IncludeCache)
        {
            mappings.Add(new CloudServiceMapping
            {
                LocalService = "redis",
                CloudService = GetCloudCacheService(cloudProvider),
                Configuration = GetCacheConfiguration(cloudProvider)
            });
        }

        if (services.IncludeStorage)
        {
            mappings.Add(new CloudServiceMapping
            {
                LocalService = "minio",
                CloudService = GetCloudStorageService(cloudProvider),
                Configuration = GetStorageConfiguration(cloudProvider)
            });
        }

        if (services.IncludeMessageQueue)
        {
            mappings.Add(new CloudServiceMapping
            {
                LocalService = "rabbitmq",
                CloudService = GetCloudQueueService(cloudProvider),
                Configuration = GetQueueConfiguration(cloudProvider)
            });
        }

        if (services.IncludeMail)
        {
            mappings.Add(new CloudServiceMapping
            {
                LocalService = "mailhog",
                CloudService = GetCloudMailService(cloudProvider),
                Configuration = GetMailConfiguration(cloudProvider)
            });
        }

        return mappings;
    }

    public async Task GenerateAspireManifestAsync(string projectPath, ServiceConfiguration services)
    {
        try
        {
            var manifest = new
            {
                resources = GenerateManifestResources(services)
            };

            var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var manifestPath = Path.Combine(projectPath, "aspire-manifest.json");
            await File.WriteAllTextAsync(manifestPath, manifestJson);

            _logger.LogInformation("Generated Aspire manifest at {ManifestPath}", manifestPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Aspire manifest");
        }
    }

    public async Task ConfigureServiceDiscoveryAsync(string projectPath, ServiceConfiguration services)
    {
        try
        {
            // Generate service discovery configuration
            var serviceDiscoveryConfig = GenerateServiceDiscoveryConfig(services);
            
            var configPath = Path.Combine(projectPath, "src", "appsettings.ServiceDiscovery.json");
            
            // Ensure directory exists
            var configDir = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            var configJson = JsonSerializer.Serialize(serviceDiscoveryConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null // Use default (PascalCase for C# properties)
            });

            await File.WriteAllTextAsync(configPath, configJson);

            _logger.LogInformation("Generated service discovery configuration at {ConfigPath}", configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring service discovery");
        }
    }

    private Dictionary<string, object> GenerateManifestResources(ServiceConfiguration services)
    {
        var resources = new Dictionary<string, object>();

        if (services.IncludeCache)
        {
            resources["redis"] = new
            {
                type = "container.v0",
                connectionString = "{redis.bindings.tcp.host}:{redis.bindings.tcp.port}",
                image = "redis:latest",
                bindings = new
                {
                    tcp = new
                    {
                        scheme = "tcp",
                        protocol = "tcp",
                        transport = "tcp",
                        targetPort = 6379
                    }
                }
            };
        }

        if (services.IncludeStorage)
        {
            resources["minio"] = new
            {
                type = "container.v0",
                image = "minio/minio:latest",
                args = new[] { "server", "/data", "--console-address", ":9001" },
                env = new
                {
                    MINIO_ROOT_USER = "minioadmin",
                    MINIO_ROOT_PASSWORD = "minioadmin"
                },
                bindings = new
                {
                    api = new
                    {
                        scheme = "http",
                        protocol = "tcp",
                        transport = "tcp",
                        targetPort = 9000
                    },
                    console = new
                    {
                        scheme = "http",
                        protocol = "tcp",
                        transport = "tcp",
                        targetPort = 9001
                    }
                }
            };
        }

        if (services.IncludeMessageQueue)
        {
            resources["rabbitmq"] = new
            {
                type = "container.v0",
                connectionString = "amqp://guest:guest@{rabbitmq.bindings.tcp.host}:{rabbitmq.bindings.tcp.port}",
                image = "rabbitmq:3-management",
                bindings = new
                {
                    tcp = new
                    {
                        scheme = "amqp",
                        protocol = "tcp",
                        transport = "tcp",
                        targetPort = 5672
                    },
                    management = new
                    {
                        scheme = "http",
                        protocol = "tcp",
                        transport = "tcp",
                        targetPort = 15672
                    }
                }
            };
        }

        if (services.IncludeMail)
        {
            resources["mailhog"] = new
            {
                type = "container.v0",
                image = "mailhog/mailhog:latest",
                bindings = new
                {
                    smtp = new
                    {
                        scheme = "smtp",
                        protocol = "tcp",
                        transport = "tcp",
                        targetPort = 1025
                    },
                    web = new
                    {
                        scheme = "http",
                        protocol = "tcp",
                        transport = "tcp",
                        targetPort = 8025
                    }
                }
            };
        }

        return resources;
    }

    private object GenerateServiceDiscoveryConfig(ServiceConfiguration services)
    {
        var config = new Dictionary<string, object>
        {
            ["Services"] = new Dictionary<string, object>()
        };

        var servicesConfig = (Dictionary<string, object>)config["Services"];

        if (services.IncludeCache)
        {
            servicesConfig["redis"] = new
            {
                https = new[] { "https://redis" },
                http = new[] { "http://redis" }
            };
        }

        if (services.IncludeStorage)
        {
            servicesConfig["minio"] = new
            {
                https = new[] { "https://minio:9000" },
                http = new[] { "http://minio:9000" }
            };
        }

        if (services.IncludeMessageQueue)
        {
            servicesConfig["rabbitmq"] = new
            {
                amqp = new[] { "amqp://rabbitmq:5672" }
            };
        }

        if (services.IncludeMail)
        {
            servicesConfig["mailhog"] = new
            {
                smtp = new[] { "smtp://mailhog:1025" },
                http = new[] { "http://mailhog:8025" }
            };
        }

        return config;
    }

    private string GetCloudCacheService(string cloudProvider) => cloudProvider.ToLowerInvariant() switch
    {
        "aws" => "ElastiCache for Redis",
        "gcp" => "Memorystore for Redis",
        "azure" => "Azure Cache for Redis",
        _ => "Redis"
    };

    private string GetCloudStorageService(string cloudProvider) => cloudProvider.ToLowerInvariant() switch
    {
        "aws" => "Amazon S3",
        "gcp" => "Google Cloud Storage",
        "azure" => "Azure Blob Storage",
        _ => "S3-Compatible Storage"
    };

    private string GetCloudQueueService(string cloudProvider) => cloudProvider.ToLowerInvariant() switch
    {
        "aws" => "Amazon SQS + EventBridge",
        "gcp" => "Google Pub/Sub",
        "azure" => "Azure Service Bus",
        _ => "Message Queue"
    };

    private string GetCloudMailService(string cloudProvider) => cloudProvider.ToLowerInvariant() switch
    {
        "aws" => "Amazon SES",
        "gcp" => "SendGrid",
        "azure" => "Azure Communication Services",
        _ => "SMTP Service"
    };

    private Dictionary<string, object> GetCacheConfiguration(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["engine"] = "redis",
                ["node_type"] = "cache.t3.micro",
                ["num_cache_nodes"] = 1
            },
            "gcp" => new Dictionary<string, object>
            {
                ["tier"] = "BASIC",
                ["memory_size_gb"] = 1,
                ["redis_version"] = "REDIS_7_0"
            },
            "azure" => new Dictionary<string, object>
            {
                ["sku_name"] = "Basic_C0",
                ["family"] = "C",
                ["capacity"] = 0
            },
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> GetStorageConfiguration(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["versioning"] = true,
                ["public_read"] = false,
                ["encryption"] = "AES256"
            },
            "gcp" => new Dictionary<string, object>
            {
                ["storage_class"] = "STANDARD",
                ["location"] = "US",
                ["uniform_bucket_level_access"] = true
            },
            "azure" => new Dictionary<string, object>
            {
                ["account_tier"] = "Standard",
                ["account_replication_type"] = "LRS",
                ["access_tier"] = "Hot"
            },
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> GetQueueConfiguration(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["visibility_timeout_seconds"] = 300,
                ["message_retention_seconds"] = 1209600,
                ["max_message_size"] = 262144
            },
            "gcp" => new Dictionary<string, object>
            {
                ["message_retention_duration"] = "604800s",
                ["message_storage_policy"] = new Dictionary<string, object>
                {
                    ["allowed_persistence_regions"] = new[] { "us-central1" }
                }
            },
            "azure" => new Dictionary<string, object>
            {
                ["sku"] = "Standard",
                ["default_message_ttl"] = "P14D",
                ["max_size_in_megabytes"] = 1024
            },
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> GetMailConfiguration(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["delivery_options"] = new Dictionary<string, object>
                {
                    ["tls_policy"] = "Require"
                }
            },
            "gcp" => new Dictionary<string, object>
            {
                ["api_key_required"] = true
            },
            "azure" => new Dictionary<string, object>
            {
                ["data_location"] = "United States"
            },
            _ => new Dictionary<string, object>()
        };
    }
}
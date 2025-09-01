using Microsoft.Extensions.Logging;
using NDC.Cli.Models;

namespace NDC.Cli.Services;

public class CloudService : ICloudService
{
    private readonly ILogger<CloudService> _logger;

    public CloudService(ILogger<CloudService> logger)
    {
        _logger = logger;
    }

    public string GetDefaultDatabase(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => "PostgreSQL",
            "gcp" => "PostgreSQL", 
            "azure" => "SqlServer",
            _ => "PostgreSQL"
        };
    }

    public string GetDefaultRegion(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => "us-east-1",
            "gcp" => "us-central1",
            "azure" => "eastus",
            _ => "us-east-1"
        };
    }

    public async Task<Dictionary<string, object>> GetCloudDefaultsAsync(string cloudProvider, string serviceType)
    {
        return serviceType.ToLowerInvariant() switch
        {
            "compute" => GetComputeDefaults(cloudProvider),
            "database" => GetDatabaseDefaults(cloudProvider),
            "cache" => GetCacheDefaults(cloudProvider),
            "storage" => GetStorageDefaults(cloudProvider),
            "queue" => GetQueueDefaults(cloudProvider),
            "mail" => GetMailDefaults(cloudProvider),
            _ => new Dictionary<string, object>()
        };
    }

    public async Task GenerateCloudInfrastructureAsync(string projectPath, string cloudProvider, ServiceConfiguration services)
    {
        try
        {
            var terraformPath = Path.Combine(projectPath, "terraform");
            if (!Directory.Exists(terraformPath))
            {
                Directory.CreateDirectory(terraformPath);
            }

            await GenerateProviderConfig(terraformPath, cloudProvider);
            await GenerateMainTerraform(terraformPath, cloudProvider, services);
            await GenerateVariables(terraformPath, cloudProvider, services);
            await GenerateOutputs(terraformPath, cloudProvider, services);

            _logger.LogInformation("Generated Terraform infrastructure for {CloudProvider}", cloudProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cloud infrastructure");
        }
    }

    private Dictionary<string, object> GetComputeDefaults(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["cpu"] = "1024", // 1 vCPU
                ["memory"] = "2048", // 2GB
                ["platform"] = "LINUX"
            },
            "gcp" => new Dictionary<string, object>
            {
                ["cpu"] = "1000m", // 1 CPU
                ["memory"] = "2Gi", // 2GB
                ["platform"] = "managed"
            },
            "azure" => new Dictionary<string, object>
            {
                ["cpu"] = "1.0", // 1 CPU
                ["memory"] = "2.0Gi", // 2GB
                ["platform"] = "linux"
            },
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> GetDatabaseDefaults(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["engine"] = "aurora-postgresql",
                ["instance_class"] = "db.t3.medium",
                ["allocated_storage"] = 20
            },
            "gcp" => new Dictionary<string, object>
            {
                ["database_version"] = "POSTGRES_15",
                ["tier"] = "db-f1-micro",
                ["disk_size"] = 20
            },
            "azure" => new Dictionary<string, object>
            {
                ["sku_name"] = "B_Gen5_1",
                ["storage_mb"] = 20480,
                ["version"] = "15"
            },
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> GetCacheDefaults(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["node_type"] = "cache.t3.micro",
                ["num_cache_nodes"] = 1,
                ["engine_version"] = "7.0"
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

    private Dictionary<string, object> GetStorageDefaults(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["versioning"] = true,
                ["encryption"] = "AES256"
            },
            "gcp" => new Dictionary<string, object>
            {
                ["storage_class"] = "STANDARD",
                ["location"] = "US"
            },
            "azure" => new Dictionary<string, object>
            {
                ["account_tier"] = "Standard",
                ["account_replication_type"] = "LRS"
            },
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> GetQueueDefaults(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["visibility_timeout_seconds"] = 300
            },
            "gcp" => new Dictionary<string, object>
            {
                ["message_retention_duration"] = "604800s"
            },
            "azure" => new Dictionary<string, object>
            {
                ["max_size_in_megabytes"] = 1024
            },
            _ => new Dictionary<string, object>()
        };
    }

    private Dictionary<string, object> GetMailDefaults(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => new Dictionary<string, object>
            {
                ["configuration_set"] = "default"
            },
            "gcp" => new Dictionary<string, object>
            {
                ["provider"] = "sendgrid"
            },
            "azure" => new Dictionary<string, object>
            {
                ["data_location"] = "United States"
            },
            _ => new Dictionary<string, object>()
        };
    }

    private async Task GenerateProviderConfig(string terraformPath, string cloudProvider)
    {
        var providerConfig = cloudProvider.ToLowerInvariant() switch
        {
            "aws" => GenerateAwsProvider(),
            "gcp" => GenerateGcpProvider(),
            "azure" => GenerateAzureProvider(),
            _ => ""
        };

        await File.WriteAllTextAsync(Path.Combine(terraformPath, "provider.tf"), providerConfig);
    }

    private async Task GenerateMainTerraform(string terraformPath, string cloudProvider, ServiceConfiguration services)
    {
        var mainConfig = cloudProvider.ToLowerInvariant() switch
        {
            "aws" => GenerateAwsMain(services),
            "gcp" => GenerateGcpMain(services),
            "azure" => GenerateAzureMain(services),
            _ => ""
        };

        await File.WriteAllTextAsync(Path.Combine(terraformPath, "main.tf"), mainConfig);
    }

    private async Task GenerateVariables(string terraformPath, string cloudProvider, ServiceConfiguration services)
    {
        var variablesConfig = GenerateVariablesConfig(cloudProvider, services);
        await File.WriteAllTextAsync(Path.Combine(terraformPath, "variables.tf"), variablesConfig);
    }

    private async Task GenerateOutputs(string terraformPath, string cloudProvider, ServiceConfiguration services)
    {
        var outputsConfig = cloudProvider.ToLowerInvariant() switch
        {
            "aws" => GenerateAwsOutputs(services),
            "gcp" => GenerateGcpOutputs(services),
            "azure" => GenerateAzureOutputs(services),
            _ => ""
        };

        await File.WriteAllTextAsync(Path.Combine(terraformPath, "outputs.tf"), outputsConfig);
    }

    private string GenerateAwsProvider()
    {
        return @"terraform {
  required_version = "">= 1.0""
  required_providers {
    aws = {
      source  = ""hashicorp/aws""
      version = ""~> 5.0""
    }
  }
}

provider ""aws"" {
  region = var.region
  
  default_tags {
    tags = {
      Project   = var.project_name
      ManagedBy = ""ndc""
      Framework = var.framework
    }
  }
}";
    }

    private string GenerateGcpProvider()
    {
        return @"terraform {
  required_version = "">= 1.0""
  required_providers {
    google = {
      source  = ""hashicorp/google""
      version = ""~> 5.0""
    }
  }
}

provider ""google"" {
  project = var.project_id
  region  = var.region
}";
    }

    private string GenerateAzureProvider()
    {
        return @"terraform {
  required_version = "">= 1.0""
  required_providers {
    azurerm = {
      source  = ""hashicorp/azurerm""
      version = ""~> 3.0""
    }
  }
}

provider ""azurerm"" {
  features {}
}";
    }

    private string GenerateAwsMain(ServiceConfiguration services)
    {
        return @"# AWS App Runner Service and supporting infrastructure
# This configuration is generated by NDC CLI

resource ""aws_apprunner_service"" ""main"" {
  service_name = var.service_name
  
  source_configuration {
    image_repository {
      image_identifier      = ""${data.aws_caller_identity.current.account_id}.dkr.ecr.${var.region}.amazonaws.com/${var.ecr_repo_name}:latest""
      image_repository_type = ""ECR""
      
      image_configuration {
        port = var.port
      }
    }
    
    auto_deployments_enabled = true
  }
  
  instance_configuration {
    cpu    = var.cpu
    memory = var.memory
  }
}

data ""aws_caller_identity"" ""current"" {}";
    }

    private string GenerateGcpMain(ServiceConfiguration services)
    {
        return @"# Google Cloud Run Service and supporting infrastructure
# This configuration is generated by NDC CLI

resource ""google_cloud_run_v2_service"" ""main"" {
  name     = var.service_name
  location = var.region
  ingress  = ""INGRESS_TRAFFIC_ALL""

  template {
    containers {
      image = ""${var.region}-docker.pkg.dev/${var.project_id}/${var.artifact_registry_repo}/${var.service_name}:latest""
      
      resources {
        limits = {
          cpu    = var.cpu
          memory = var.memory
        }
      }
      
      ports {
        container_port = var.port
      }
    }
  }
}";
    }

    private string GenerateAzureMain(ServiceConfiguration services)
    {
        return @"# Azure Container Apps and supporting infrastructure
# This configuration is generated by NDC CLI

resource ""azurerm_resource_group"" ""main"" {
  name     = ""rg-${var.service_name}""
  location = var.location
}

resource ""azurerm_container_app"" ""main"" {
  name                         = ""ca-${var.service_name}""
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = ""Single""

  template {
    container {
      name   = var.service_name
      image  = ""${azurerm_container_registry.main.login_server}/${var.service_name}:latest""
      cpu    = var.cpu
      memory = var.memory
    }
  }
}

resource ""azurerm_container_app_environment"" ""main"" {
  name                = ""cae-${var.service_name}""
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}";
    }

    private string GenerateVariablesConfig(string cloudProvider, ServiceConfiguration services)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => GenerateAwsVariables(),
            "gcp" => GenerateGcpVariables(),
            "azure" => GenerateAzureVariables(),
            _ => ""
        };
    }

    private string GenerateAwsVariables()
    {
        return @"variable ""project_name"" {
  description = ""Project name""
  type        = string
}

variable ""service_name"" {
  description = ""Service name""
  type        = string
}

variable ""region"" {
  description = ""AWS region""
  type        = string
  default     = ""us-east-1""
}

variable ""framework"" {
  description = "".NET framework version""
  type        = string
  default     = ""net9.0""
}

variable ""port"" {
  description = ""Application port""
  type        = number
  default     = 8080
}

variable ""cpu"" {
  description = ""CPU allocation""
  type        = string
  default     = ""1024""
}

variable ""memory"" {
  description = ""Memory allocation""
  type        = string
  default     = ""2048""
}

variable ""ecr_repo_name"" {
  description = ""ECR repository name""
  type        = string
}";
    }

    private string GenerateGcpVariables()
    {
        return @"variable ""project_id"" {
  description = ""Google Cloud project ID""
  type        = string
}

variable ""service_name"" {
  description = ""Service name""
  type        = string
}

variable ""region"" {
  description = ""Google Cloud region""
  type        = string
  default     = ""us-central1""
}

variable ""port"" {
  description = ""Application port""
  type        = number
  default     = 8080
}

variable ""cpu"" {
  description = ""CPU allocation""
  type        = string
  default     = ""1000m""
}

variable ""memory"" {
  description = ""Memory allocation""
  type        = string
  default     = ""2Gi""
}

variable ""artifact_registry_repo"" {
  description = ""Artifact Registry repository name""
  type        = string
}";
    }

    private string GenerateAzureVariables()
    {
        return @"variable ""service_name"" {
  description = ""Service name""
  type        = string
}

variable ""location"" {
  description = ""Azure location""
  type        = string
  default     = ""East US""
}

variable ""port"" {
  description = ""Application port""
  type        = number
  default     = 8080
}

variable ""cpu"" {
  description = ""CPU allocation""
  type        = number
  default     = 1.0
}

variable ""memory"" {
  description = ""Memory allocation""
  type        = string
  default     = ""2.0Gi""
}";
    }

    private string GenerateAwsOutputs(ServiceConfiguration services)
    {
        return @"output ""service_url"" {
  description = ""App Runner service URL""
  value       = aws_apprunner_service.main.service_url
}

output ""service_arn"" {
  description = ""App Runner service ARN""
  value       = aws_apprunner_service.main.arn
}";
    }

    private string GenerateGcpOutputs(ServiceConfiguration services)
    {
        return @"output ""service_url"" {
  description = ""Cloud Run service URL""
  value       = google_cloud_run_v2_service.main.uri
}

output ""service_name"" {
  description = ""Cloud Run service name""
  value       = google_cloud_run_v2_service.main.name
}";
    }

    private string GenerateAzureOutputs(ServiceConfiguration services)
    {
        return @"output ""service_fqdn"" {
  description = ""Container App FQDN""
  value       = azurerm_container_app.main.latest_revision_fqdn
}

output ""service_url"" {
  description = ""Container App URL""
  value       = ""https://${azurerm_container_app.main.latest_revision_fqdn}""
}";
    }
}
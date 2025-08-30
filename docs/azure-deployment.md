# Azure Deployment Guide

Deploy .NET applications to Azure using Container Apps and Container Registry.

## Prerequisites

- Azure CLI (`az`) configured
- Docker installed and running  
- Terraform >= 1.0
- Azure subscription with appropriate permissions

## Required Azure Providers

The following resource providers must be registered:
```bash
az provider register --namespace Microsoft.ContainerRegistry
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights
az provider register --namespace Microsoft.ManagedIdentity
```

## IAM Permissions Required

Your user needs these roles:
- `Contributor` (or more specific roles below)
- `Container Registry Contributor`
- `Container Apps Contributor`
- `Log Analytics Contributor`

## Step-by-Step Deployment

### 1. Create Project
```bash
cp -r examples/working-azure-template my-api
cd my-api
```

### 2. Login to Azure
```bash
az login
az account set --subscription "your-subscription-id"
```

### 3. Deploy Infrastructure First
```bash
cd terraform
terraform init
terraform plan
terraform apply
```

This creates:
- Resource Group
- Container Registry (ACR)
- Log Analytics Workspace
- Container Apps Environment
- User-Assigned Managed Identity
- Container App (initially failing - no image)

### 4. Get Registry Credentials
```bash
# Get registry URL
ACR_URL=$(terraform output -raw container_registry_url)

# Login to ACR
az acr login --name $(echo $ACR_URL | cut -d'.' -f1)
```

### 5. Build and Push Container
```bash
# Build the container
docker build -t my-api .

# Tag for ACR
docker tag my-api $ACR_URL/my-api:latest

# Push to ACR
docker push $ACR_URL/my-api:latest
```

### 6. Verify Deployment
```bash
# Get application URL
terraform output container_app_url

# Test the application
curl $(terraform output -raw container_app_url)
curl $(terraform output -raw container_app_url)/health
```

## Configuration Options

### Scaling Configuration
Edit `terraform/variables.tf`:
```hcl
variable "min_instances" {
  default = 0   # Scale to zero for cost savings
}

variable "max_instances" {
  default = 10  # Maximum instances
}
```

### Resource Allocation
```hcl
variable "cpu" {
  default = 1.0    # CPU cores (0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0)
}

variable "memory" {
  default = "2.0Gi"  # Memory (0.5Gi to 4Gi)
}
```

### Environment Variables
Add to `main.tf`:
```hcl
resource "azurerm_container_app" "app" {
  # ... existing configuration

  template {
    container {
      # ... existing configuration
      
      env {
        name  = "CUSTOM_VAR"
        value = "production"
      }
      
      # Secret from Key Vault
      env {
        name        = "DB_PASSWORD"
        secret_name = "db-password"
      }
    }
  }

  # Define secrets
  secret {
    name  = "db-password"
    value = "your-secret-value"
  }
}
```

### Custom Domains
```hcl
# Add custom domain
resource "azurerm_container_app_custom_domain" "app" {
  name             = "api.yourdomain.com"
  container_app_id = azurerm_container_app.app.id
  
  certificate_binding_type = "SniEnabled"
  certificate_id          = azurerm_container_app_managed_certificate.app.id
}

resource "azurerm_container_app_managed_certificate" "app" {
  name               = "api-cert"
  container_app_environment_id = azurerm_container_app_environment.app.id
  subject_name       = "api.yourdomain.com"
  domain_control_validation_type = "CNAME"
}
```

## Monitoring and Logs

### Log Analytics
```bash
# View logs
az monitor log-analytics query \
  --workspace $(terraform output -raw log_analytics_workspace_id) \
  --analytics-query "ContainerAppConsoleLogs_CL | where ContainerName_s == 'my-api' | limit 100"
```

### Azure Monitor
Add Application Insights:
```hcl
resource "azurerm_application_insights" "app" {
  name                = "my-api-insights"
  location            = azurerm_resource_group.app.location
  resource_group_name = azurerm_resource_group.app.name
  application_type    = "web"
}

# Add to container environment
env {
  name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
  value = azurerm_application_insights.app.connection_string
}
```

### Structured Logging
```csharp
// Program.cs
using Microsoft.ApplicationInsights.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();

app.MapGet("/", (ILogger<Program> logger) => {
    logger.LogInformation("Request received at {Timestamp}", DateTime.UtcNow);
    return Results.Ok("Hello World");
});
```

## Security

### Private Container Apps
```hcl
resource "azurerm_container_app" "app" {
  # ... existing configuration

  ingress {
    external_enabled = false  # Internal only
    target_port      = var.port
  }
}
```

### Network Isolation
```hcl
# Create VNet integration
resource "azurerm_virtual_network" "app" {
  name                = "my-api-vnet"
  location            = azurerm_resource_group.app.location
  resource_group_name = azurerm_resource_group.app.name
  address_space       = ["10.0.0.0/16"]
}

resource "azurerm_subnet" "app" {
  name                 = "container-apps"
  resource_group_name  = azurerm_resource_group.app.name
  virtual_network_name = azurerm_virtual_network.app.name
  address_prefixes     = ["10.0.1.0/24"]

  delegation {
    name = "Microsoft.App.environments"
    service_delegation {
      name = "Microsoft.App/environments"
    }
  }
}

resource "azurerm_container_app_environment" "app" {
  # ... existing configuration
  
  infrastructure_subnet_id = azurerm_subnet.app.id
}
```

### Key Vault Integration
```hcl
resource "azurerm_key_vault" "app" {
  name                = "my-api-kv-${random_string.suffix.result}"
  location            = azurerm_resource_group.app.location
  resource_group_name = azurerm_resource_group.app.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
}

# Grant managed identity access to Key Vault
resource "azurerm_key_vault_access_policy" "app" {
  key_vault_id = azurerm_key_vault.app.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.app.principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}
```

## Troubleshooting

### Container App Not Starting
```bash
# Check container app status
az containerapp show --name my-api --resource-group rg-my-api

# Check logs
az containerapp logs show --name my-api --resource-group rg-my-api --follow
```

### Container Build Issues
```bash
# Build and test locally
docker build -t my-api .
docker run -p 8080:8080 my-api

# Check ACR build
az acr build --registry $(echo $ACR_URL | cut -d'.' -f1) --image my-api:latest .
```

### Registry Access Issues
```bash
# Check managed identity role assignments
az role assignment list --assignee $(terraform output -raw managed_identity_principal_id)

# Test registry access
az acr login --name $(echo $ACR_URL | cut -d'.' -f1)
```

## Cost Optimization

### Scale-to-Zero
```hcl
variable "min_instances" {
  default = 0  # Scale to zero when no traffic
}
```

### Resource Sizing
```hcl
# Start with minimal resources
variable "cpu" {
  default = 0.25
}

variable "memory" {
  default = "0.5Gi"
}
```

### Consumption Plan
Container Apps uses consumption-based pricing:
- Pay only for resources used
- Free allocation included (2M requests/month)

## Cleanup

```bash
# Destroy all resources
cd terraform
terraform destroy
```

This removes the entire resource group and all contained resources.

**Note**: Deleted container registries can be recovered for 7 days, but this incurs storage costs.
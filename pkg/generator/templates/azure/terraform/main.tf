# ---------------------------
# Resource Group
# ---------------------------
resource "azurerm_resource_group" "app" {
  name     = "rg-{{.ServiceName}}"
  location = var.location

  tags = {
    project   = "{{.Name}}"
    managedBy = "noundry"
    framework = "{{.Framework}}"
  }
}

# ---------------------------
# Container Registry
# ---------------------------
resource "azurerm_container_registry" "app" {
  name                = "acr{{.ServiceName}}${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.app.name
  location            = azurerm_resource_group.app.location
  sku                 = "Basic"
  admin_enabled       = true

  tags = {
    project   = "{{.Name}}"
    managedBy = "noundry"
  }
}

resource "random_string" "suffix" {
  length  = 8
  special = false
  upper   = false
}

# ---------------------------
# Log Analytics Workspace
# ---------------------------
resource "azurerm_log_analytics_workspace" "app" {
  name                = "law-{{.ServiceName}}"
  location            = azurerm_resource_group.app.location
  resource_group_name = azurerm_resource_group.app.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = {
    project   = "{{.Name}}"
    managedBy = "noundry"
  }
}

# ---------------------------
# Container Apps Environment
# ---------------------------
resource "azurerm_container_app_environment" "app" {
  name                       = "cae-{{.ServiceName}}"
  location                   = azurerm_resource_group.app.location
  resource_group_name        = azurerm_resource_group.app.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.app.id

  tags = {
    project   = "{{.Name}}"
    managedBy = "noundry"
  }
}

# ---------------------------
# User Assigned Identity
# ---------------------------
resource "azurerm_user_assigned_identity" "app" {
  name                = "id-{{.ServiceName}}"
  location            = azurerm_resource_group.app.location
  resource_group_name = azurerm_resource_group.app.name

  tags = {
    project   = "{{.Name}}"
    managedBy = "noundry"
  }
}

# ---------------------------
# Role Assignment for Container Registry
# ---------------------------
resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.app.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.app.principal_id
}

# ---------------------------
# Container App
# ---------------------------
resource "azurerm_container_app" "app" {
  name                         = "ca-{{.ServiceName}}"
  container_app_environment_id = azurerm_container_app_environment.app.id
  resource_group_name          = azurerm_resource_group.app.name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.app.id]
  }

  registry {
    server   = azurerm_container_registry.app.login_server
    identity = azurerm_user_assigned_identity.app.id
  }

  template {
    min_replicas = {{.MinInstances}}
    max_replicas = {{.MaxInstances}}

    container {
      name   = "{{.ServiceName}}"
      image  = "${azurerm_container_registry.app.login_server}/{{.ServiceName}}:latest"
      cpu    = {{.CPU}}
      memory = "{{.Memory}}"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "ASPNETCORE_URLS"
        value = "http://0.0.0.0:{{.Port}}"
      }

      liveness_probe {
        transport = "HTTP"
        port      = {{.Port}}
        path      = "/health"

        initial_delay           = 30
        interval_seconds        = 10
        timeout                 = 5
        failure_count_threshold = 3
      }

      readiness_probe {
        transport = "HTTP"
        port      = {{.Port}}
        path      = "/health"

        interval_seconds        = 5
        timeout                 = 5
        failure_count_threshold = 3
        success_count_threshold = 1
      }

      startup_probe {
        transport = "HTTP"
        port      = {{.Port}}
        path      = "/health"

        interval_seconds        = 5
        timeout                 = 5
        failure_count_threshold = 10
      }
    }

    http_scale_rule {
      name                = "http-scale"
      concurrent_requests = 100
    }
  }

  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = {{.Port}}
    transport                  = "http"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  tags = {
    project   = "{{.Name}}"
    managedBy = "noundry"
  }

  depends_on = [azurerm_role_assignment.acr_pull]
}

# ---------------------------
# Outputs
# ---------------------------
output "container_app_fqdn" {
  value       = azurerm_container_app.app.latest_revision_fqdn
  description = "The FQDN of the Container App"
}

output "container_app_url" {
  value       = "https://${azurerm_container_app.app.latest_revision_fqdn}"
  description = "The URL of the Container App"
}

output "container_registry_url" {
  value       = azurerm_container_registry.app.login_server
  description = "The URL of the Container Registry"
}

output "container_registry_username" {
  value       = azurerm_container_registry.app.admin_username
  description = "The admin username for the Container Registry"
  sensitive   = true
}

output "container_registry_password" {
  value       = azurerm_container_registry.app.admin_password
  description = "The admin password for the Container Registry"
  sensitive   = true
}
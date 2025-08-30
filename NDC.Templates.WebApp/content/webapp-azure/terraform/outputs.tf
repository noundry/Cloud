output "service_url" {
  description = "Container App URL"
  value       = "https://${azurerm_container_app.main.latest_revision_fqdn}"
}

output "service_fqdn" {
  description = "Container App FQDN"
  value       = azurerm_container_app.main.latest_revision_fqdn
}

output "container_registry_login_server" {
  description = "Container Registry login server"
  value       = azurerm_container_registry.main.login_server
}

output "container_registry_admin_username" {
  description = "Container Registry admin username"
  value       = azurerm_container_registry.main.admin_username
  sensitive   = true
}

output "container_registry_admin_password" {
  description = "Container Registry admin password"
  value       = azurerm_container_registry.main.admin_password
  sensitive   = true
}

output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_resource_group.main.name
}

output "location" {
  description = "Azure region"
  value       = azurerm_resource_group.main.location
}

# Database outputs
output "database_server_name" {
  description = "SQL Server name"
  value       = var.enable_database ? azurerm_mssql_server.main[0].name : null
}

output "database_server_fqdn" {
  description = "SQL Server fully qualified domain name"
  value       = var.enable_database ? azurerm_mssql_server.main[0].fully_qualified_domain_name : null
}

output "database_name" {
  description = "SQL Database name"
  value       = var.enable_database ? azurerm_mssql_database.main[0].name : null
}

# Cache outputs
output "redis_hostname" {
  description = "Redis cache hostname"
  value       = var.enable_cache ? azurerm_redis_cache.main[0].hostname : null
}

output "redis_ssl_port" {
  description = "Redis cache SSL port"
  value       = var.enable_cache ? azurerm_redis_cache.main[0].ssl_port : null
}

output "redis_primary_access_key" {
  description = "Redis cache primary access key"
  value       = var.enable_cache ? azurerm_redis_cache.main[0].primary_access_key : null
  sensitive   = true
}

# Storage outputs
output "storage_account_name" {
  description = "Storage account name"
  value       = var.enable_storage ? azurerm_storage_account.main[0].name : null
}

output "storage_primary_connection_string" {
  description = "Storage account primary connection string"
  value       = var.enable_storage ? azurerm_storage_account.main[0].primary_connection_string : null
  sensitive   = true
}

output "storage_container_name" {
  description = "Storage container name"
  value       = var.enable_storage ? azurerm_storage_container.main[0].name : null
}

# Service Bus outputs
output "servicebus_namespace_name" {
  description = "Service Bus namespace name"
  value       = var.enable_queue ? azurerm_servicebus_namespace.main[0].name : null
}

output "servicebus_primary_connection_string" {
  description = "Service Bus primary connection string"
  value       = var.enable_queue ? azurerm_servicebus_namespace.main[0].default_primary_connection_string : null
  sensitive   = true
}

output "servicebus_queue_name" {
  description = "Service Bus queue name"
  value       = var.enable_queue ? azurerm_servicebus_queue.main[0].name : null
}
variable "service_name" {
  description = "Name of the service"
  type        = string
  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.service_name)) && length(var.service_name) <= 32
    error_message = "Service name must contain only lowercase letters, numbers, and hyphens, and be 32 characters or less."
  }
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "port" {
  description = "Application port"
  type        = number
  default     = 8080
}

variable "cpu" {
  description = "CPU allocation for Container Apps"
  type        = number
  default     = 1.0
}

variable "memory" {
  description = "Memory allocation for Container Apps"
  type        = string
  default     = "2.0Gi"
}

variable "min_replicas" {
  description = "Minimum number of replicas"
  type        = number
  default     = 1
}

variable "max_replicas" {
  description = "Maximum number of replicas"
  type        = number
  default     = 5
}

# Container Registry
variable "acr_sku" {
  description = "Container Registry SKU"
  type        = string
  default     = "Basic"
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.acr_sku)
    error_message = "ACR SKU must be Basic, Standard, or Premium."
  }
}

# Database configuration
variable "enable_database" {
  description = "Enable Azure SQL Database"
  type        = bool
  default     = true
}

variable "database_sku" {
  description = "Database SKU"
  type        = string
  default     = "Basic"
}

variable "database_admin_username" {
  description = "Database administrator username"
  type        = string
  default     = "sqladmin"
}

variable "database_admin_password" {
  description = "Database administrator password"
  type        = string
  sensitive   = true
  default     = "P@ssw0rd123!"
  validation {
    condition     = length(var.database_admin_password) >= 8
    error_message = "Database password must be at least 8 characters long."
  }
}

# Cache configuration
variable "enable_cache" {
  description = "Enable Azure Redis Cache"
  type        = bool
  default     = true
}

variable "redis_sku" {
  description = "Redis cache SKU"
  type        = string
  default     = "Basic"
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.redis_sku)
    error_message = "Redis SKU must be Basic, Standard, or Premium."
  }
}

variable "redis_family" {
  description = "Redis cache family"
  type        = string
  default     = "C"
}

variable "redis_capacity" {
  description = "Redis cache capacity"
  type        = number
  default     = 0
}

# Storage configuration
variable "enable_storage" {
  description = "Enable Azure Storage Account"
  type        = bool
  default     = true
}

variable "storage_account_tier" {
  description = "Storage account tier"
  type        = string
  default     = "Standard"
  validation {
    condition     = contains(["Standard", "Premium"], var.storage_account_tier)
    error_message = "Storage account tier must be Standard or Premium."
  }
}

variable "storage_account_replication_type" {
  description = "Storage account replication type"
  type        = string
  default     = "LRS"
  validation {
    condition     = contains(["LRS", "GRS", "RAGRS", "ZRS", "GZRS", "RAGZRS"], var.storage_account_replication_type)
    error_message = "Storage account replication type must be one of: LRS, GRS, RAGRS, ZRS, GZRS, RAGZRS."
  }
}

# Queue configuration
variable "enable_queue" {
  description = "Enable Azure Service Bus"
  type        = bool
  default     = false
}

variable "servicebus_sku" {
  description = "Service Bus SKU"
  type        = string
  default     = "Basic"
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.servicebus_sku)
    error_message = "Service Bus SKU must be Basic, Standard, or Premium."
  }
}

# Tags
variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    "managed-by" = "ndc-cli"
    "framework"  = "dotnet"
  }
}
variable "project_id" {
  description = "Google Cloud project ID"
  type        = string
}

variable "service_name" {
  description = "Name of the service"
  type        = string
  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.service_name))
    error_message = "Service name must contain only lowercase letters, numbers, and hyphens."
  }
}

variable "region" {
  description = "Google Cloud region"
  type        = string
  default     = "us-central1"
}

variable "port" {
  description = "Application port"
  type        = number
  default     = 8080
}

variable "cpu" {
  description = "CPU allocation for Cloud Run"
  type        = string
  default     = "1000m"
}

variable "memory" {
  description = "Memory allocation for Cloud Run"
  type        = string
  default     = "2Gi"
}

# Database configuration
variable "enable_database" {
  description = "Enable Cloud SQL PostgreSQL instance"
  type        = bool
  default     = true
}

variable "database_tier" {
  description = "Cloud SQL instance tier"
  type        = string
  default     = "db-f1-micro"
}

variable "database_user" {
  description = "Database user name"
  type        = string
  default     = "appuser"
}

variable "database_password" {
  description = "Database password"
  type        = string
  sensitive   = true
  default     = "changeme123!"
}

# Cache configuration
variable "enable_cache" {
  description = "Enable Redis Memorystore instance"
  type        = bool
  default     = true
}

variable "redis_tier" {
  description = "Redis tier (BASIC or STANDARD_HA)"
  type        = string
  default     = "BASIC"
}

variable "redis_memory_size_gb" {
  description = "Redis memory size in GB"
  type        = number
  default     = 1
}

# Storage configuration
variable "enable_storage" {
  description = "Enable Cloud Storage bucket"
  type        = bool
  default     = true
}

# Queue configuration
variable "enable_queue" {
  description = "Enable Pub/Sub topic and subscription"
  type        = bool
  default     = false
}

# Networking
variable "enable_private_networking" {
  description = "Enable private networking for services"
  type        = bool
  default     = true
}

# Tags and labels
variable "labels" {
  description = "Labels to apply to resources"
  type        = map(string)
  default = {
    "managed-by" = "ndc-cli"
    "framework"  = "dotnet"
  }
}
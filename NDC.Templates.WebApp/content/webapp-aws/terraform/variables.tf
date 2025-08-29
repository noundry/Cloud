# Core Configuration
variable "environment" {
  description = "Environment name (development, staging, production)"
  type        = string
  default     = "development"
}

variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

# Application Configuration
variable "port" {
  description = "Application port"
  type        = number
  default     = Port
}

variable "cpu" {
  description = "CPU allocation (256, 512, 1024, 2048, 4096)"
  type        = string
  default     = "1024"
}

variable "memory" {
  description = "Memory allocation (512, 1024, 2048, 3072, 4096, 8192, 12288)"
  type        = string
  default     = "2048"
}

# Auto Scaling Configuration
variable "min_instances" {
  description = "Minimum number of instances"
  type        = number
  default     = 1
}

variable "max_instances" {
  description = "Maximum number of instances"
  type        = number
  default     = 10
}

variable "max_concurrency" {
  description = "Maximum concurrent requests per instance"
  type        = number
  default     = 100
}

variable "auto_deployments_enabled" {
  description = "Enable automatic deployments when new images are pushed"
  type        = bool
  default     = true
}

# Network Configuration
variable "vpc_id" {
  description = "VPC ID for database and cache"
  type        = string
  default     = "" # Will use default VPC if not provided
}

variable "vpc_cidr" {
  description = "VPC CIDR block"
  type        = string
  default     = "10.0.0.0/16"
}

variable "database_subnet_ids" {
  description = "Subnet IDs for database"
  type        = list(string)
  default     = [] # Will use default subnets if not provided
}

variable "cache_subnet_ids" {
  description = "Subnet IDs for cache"
  type        = list(string)
  default     = [] # Will use default subnets if not provided
}

# Service Feature Flags (based on template configuration)
variable "include_database" {
  description = "Include database service"
  type        = bool
  default     = HasDatabase
}

variable "include_cache" {
  description = "Include cache service"
  type        = bool
  default     = IncludeCache
}

variable "include_storage" {
  description = "Include storage service"
  type        = bool
  default     = IncludeStorage
}

variable "include_queue" {
  description = "Include message queue service"
  type        = bool
  default     = IncludeQueue
}

#if (HasDatabase)
# Database Configuration
variable "database_type" {
  description = "Database type"
  type        = string
#if (UsePostgreSQL)
  default     = "PostgreSQL"
#endif
#if (UseMySQL)
  default     = "MySQL"
#endif
#if (UseSqlServer)
  default     = "SqlServer"
#endif
}

variable "database_instance_class" {
  description = "Database instance class"
  type        = string
  default     = "db.t3.micro"
}

variable "database_allocated_storage" {
  description = "Database allocated storage in GB"
  type        = number
  default     = 20
}

variable "database_username" {
  description = "Database master username"
  type        = string
  default     = "admin"
}

variable "database_password" {
  description = "Database master password"
  type        = string
  sensitive   = true
  
  validation {
    condition     = length(var.database_password) >= 8
    error_message = "Database password must be at least 8 characters long."
  }
}

variable "database_port" {
  description = "Database port"
  type        = number
#if (UsePostgreSQL)
  default     = 5432
#endif
#if (UseMySQL)
  default     = 3306
#endif
#if (UseSqlServer)
  default     = 1433
#endif
}

#if (UsePostgreSQL)
variable "postgres_version" {
  description = "PostgreSQL version"
  type        = string
  default     = "15.4"
}
#endif

#if (UseMySQL)
variable "mysql_version" {
  description = "MySQL version"
  type        = string
  default     = "8.0"
}
#endif
#endif

#if (IncludeCache)
# Cache Configuration
variable "cache_node_type" {
  description = "ElastiCache node type"
  type        = string
  default     = "cache.t3.micro"
}
#endif

# Additional Environment Variables
variable "additional_environment_variables" {
  description = "Additional environment variables for the application"
  type        = map(string)
  default     = {}
}

# Tags
variable "tags" {
  description = "Additional tags to apply to resources"
  type        = map(string)
  default     = {}
}
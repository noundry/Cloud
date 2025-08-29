variable "project_id" {
  description = "Google Cloud Project ID"
  type        = string
}

variable "region" {
  description = "Google Cloud region"
  type        = string
  default     = "us-central1"
}

variable "service_name" {
  description = "Cloud Run service name"
  type        = string
  default     = "{{.ServiceName}}"
}

variable "min_instances" {
  description = "Minimum number of instances"
  type        = number
  default     = {{.MinInstances}}
}

variable "max_instances" {
  description = "Maximum number of instances"
  type        = number
  default     = {{.MaxInstances}}
}

variable "cpu" {
  description = "CPU allocation for the service"
  type        = string
  default     = "{{.CPU}}"
}

variable "memory" {
  description = "Memory allocation for the service"
  type        = string
  default     = "{{.Memory}}"
}

variable "port" {
  description = "Port on which the application runs"
  type        = number
  default     = {{.Port}}
}
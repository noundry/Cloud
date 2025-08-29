variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "service_name" {
  description = "Container App service name"
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
  type        = number
  default     = {{.CPU}}
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
# Application Outputs
output "app_runner_service_url" {
  description = "The URL of the App Runner service"
  value       = aws_apprunner_service.app.service_url
}

output "app_runner_service_arn" {
  description = "The ARN of the App Runner service"
  value       = aws_apprunner_service.app.arn
}

output "ecr_repository_url" {
  description = "The URL of the ECR repository"
  value       = aws_ecr_repository.app.repository_url
}

# Database Outputs
#if (HasDatabase)
#if (UsePostgreSQL)
output "database_endpoint" {
  description = "Database endpoint"
  value       = local.has_database ? aws_db_instance.postgres[0].endpoint : null
  sensitive   = true
}

output "database_port" {
  description = "Database port"
  value       = local.has_database ? aws_db_instance.postgres[0].port : null
}

output "database_name" {
  description = "Database name"
  value       = local.has_database ? aws_db_instance.postgres[0].db_name : null
}

output "database_connection_string" {
  description = "Full database connection string for application configuration"
  value       = local.has_database ? "Host=${aws_db_instance.postgres[0].endpoint};Port=${aws_db_instance.postgres[0].port};Database=${aws_db_instance.postgres[0].db_name};Username=${var.database_username};Password=${var.database_password}" : null
  sensitive   = true
}
#endif

#if (UseMySQL)
output "database_endpoint" {
  description = "Database endpoint"
  value       = local.has_database ? aws_db_instance.mysql[0].endpoint : null
  sensitive   = true
}

output "database_port" {
  description = "Database port" 
  value       = local.has_database ? aws_db_instance.mysql[0].port : null
}

output "database_name" {
  description = "Database name"
  value       = local.has_database ? aws_db_instance.mysql[0].db_name : null
}

output "database_connection_string" {
  description = "Full database connection string for application configuration"
  value       = local.has_database ? "Server=${aws_db_instance.mysql[0].endpoint};Port=${aws_db_instance.mysql[0].port};Database=${aws_db_instance.mysql[0].db_name};Uid=${var.database_username};Pwd=${var.database_password};" : null
  sensitive   = true
}
#endif
#endif

# Cache Outputs  
#if (IncludeCache)
output "redis_endpoint" {
  description = "Redis cache endpoint"
  value       = local.has_cache ? aws_elasticache_cluster.redis[0].cache_nodes[0].address : null
}

output "redis_port" {
  description = "Redis cache port"
  value       = local.has_cache ? aws_elasticache_cluster.redis[0].cache_nodes[0].port : null
}

output "redis_connection_string" {
  description = "Redis connection string for application configuration"
  value       = local.has_cache ? "${aws_elasticache_cluster.redis[0].cache_nodes[0].address}:${aws_elasticache_cluster.redis[0].cache_nodes[0].port}" : null
}
#endif

# Storage Outputs
#if (IncludeStorage)
output "s3_bucket_name" {
  description = "S3 bucket name"
  value       = local.has_storage ? aws_s3_bucket.app[0].bucket : null
}

output "s3_bucket_arn" {
  description = "S3 bucket ARN"
  value       = local.has_storage ? aws_s3_bucket.app[0].arn : null
}

output "s3_bucket_region" {
  description = "S3 bucket region"
  value       = local.has_storage ? aws_s3_bucket.app[0].region : null
}
#endif

# Message Queue Outputs
#if (IncludeQueue)
output "sqs_queue_url" {
  description = "SQS queue URL"
  value       = local.has_queue ? aws_sqs_queue.app[0].url : null
}

output "sqs_queue_arn" {
  description = "SQS queue ARN"
  value       = local.has_queue ? aws_sqs_queue.app[0].arn : null
}

output "sqs_dlq_url" {
  description = "SQS dead letter queue URL"
  value       = local.has_queue ? aws_sqs_queue.app_dlq[0].url : null
}
#endif

# Deployment Information
output "deployment_instructions" {
  description = "Instructions for deploying the application"
  value = <<-EOT
    
    Deployment completed! Follow these steps to deploy your application:
    
    1. Build and tag your container:
       docker build -t ${local.service_name} .
       docker tag ${local.service_name} ${aws_ecr_repository.app.repository_url}:latest
    
    2. Login to ECR:
       aws ecr get-login-password --region ${data.aws_region.current.name} | docker login --username AWS --password-stdin ${aws_ecr_repository.app.repository_url}
    
    3. Push your container:
       docker push ${aws_ecr_repository.app.repository_url}:latest
    
    4. Your application will be available at:
       ${aws_apprunner_service.app.service_url}
    
    Environment variables are automatically configured for cloud services.
    
  EOT
}
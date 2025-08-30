output "service_url" {
  description = "Cloud Run service URL"
  value       = google_cloud_run_v2_service.main.uri
}

output "service_name" {
  description = "Cloud Run service name"
  value       = google_cloud_run_v2_service.main.name
}

output "artifact_registry_repository" {
  description = "Artifact Registry repository name"
  value       = google_artifact_registry_repository.main.repository_id
}

output "artifact_registry_url" {
  description = "Artifact Registry repository URL"
  value       = "${var.region}-docker.pkg.dev/${var.project_id}/${google_artifact_registry_repository.main.repository_id}"
}

output "database_connection_name" {
  description = "Cloud SQL instance connection name"
  value       = var.enable_database ? google_sql_database_instance.main[0].connection_name : null
}

output "database_private_ip" {
  description = "Cloud SQL private IP address"
  value       = var.enable_database ? google_sql_database_instance.main[0].private_ip_address : null
}

output "redis_host" {
  description = "Redis instance host"
  value       = var.enable_cache ? google_redis_instance.main[0].host : null
}

output "redis_port" {
  description = "Redis instance port"
  value       = var.enable_cache ? google_redis_instance.main[0].port : null
}

output "storage_bucket_name" {
  description = "Cloud Storage bucket name"
  value       = var.enable_storage ? google_storage_bucket.main[0].name : null
}

output "storage_bucket_url" {
  description = "Cloud Storage bucket URL"
  value       = var.enable_storage ? google_storage_bucket.main[0].url : null
}

output "pubsub_topic_name" {
  description = "Pub/Sub topic name"
  value       = var.enable_queue ? google_pubsub_topic.main[0].name : null
}

output "pubsub_subscription_name" {
  description = "Pub/Sub subscription name"
  value       = var.enable_queue ? google_pubsub_subscription.main[0].name : null
}

output "project_id" {
  description = "Google Cloud project ID"
  value       = var.project_id
}

output "region" {
  description = "Google Cloud region"
  value       = var.region
}
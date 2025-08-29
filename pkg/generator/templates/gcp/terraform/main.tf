# ---------------------------
# Artifact Registry Repository
# ---------------------------
resource "google_artifact_registry_repository" "app" {
  location      = var.region
  repository_id = "{{.ServiceName}}"
  description   = "Docker repository for {{.ProjectName}}"
  format        = "DOCKER"

  cleanup_policies {
    id     = "keep-recent-images"
    action = "DELETE"
    condition {
      tag_state  = "ANY"
      older_than = "2592000s" # 30 days
    }
  }
}

# ---------------------------
# Service Account for Cloud Run
# ---------------------------
resource "google_service_account" "cloudrun_service" {
  account_id   = "{{.ServiceName}}-sa"
  display_name = "{{.ProjectName}} Cloud Run Service Account"
  description  = "Service account for {{.ProjectName}} Cloud Run service"
}

# Grant necessary permissions
resource "google_project_iam_member" "cloudrun_service_artifact_registry" {
  project = var.project_id
  role    = "roles/artifactregistry.reader"
  member  = "serviceAccount:${google_service_account.cloudrun_service.email}"
}

# ---------------------------
# Cloud Run Service
# ---------------------------
resource "google_cloud_run_v2_service" "app" {
  name     = "{{.ServiceName}}"
  location = var.region
  ingress  = "INGRESS_TRAFFIC_ALL"

  template {
    service_account = google_service_account.cloudrun_service.email
    
    scaling {
      min_instance_count = {{.MinInstances}}
      max_instance_count = {{.MaxInstances}}
    }

    containers {
      image = "${var.region}-docker.pkg.dev/${var.project_id}/{{.ServiceName}}/{{.ServiceName}}:latest"
      
      resources {
        limits = {
          cpu    = "{{.CPU}}"
          memory = "{{.Memory}}"
        }
      }
      
      ports {
        container_port = {{.Port}}
      }
      
      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      
      env {
        name  = "ASPNETCORE_URLS"
        value = "http://0.0.0.0:{{.Port}}"
      }

      liveness_probe {
        http_get {
          path = "/health"
          port = {{.Port}}
        }
        initial_delay_seconds = 30
        period_seconds        = 10
        timeout_seconds       = 5
        failure_threshold     = 3
      }

      startup_probe {
        http_get {
          path = "/health"
          port = {{.Port}}
        }
        initial_delay_seconds = 10
        period_seconds        = 5
        timeout_seconds       = 5
        failure_threshold     = 10
      }
    }

    timeout = "300s"
  }

  depends_on = [google_artifact_registry_repository.app]
}

# ---------------------------
# IAM for Public Access
# ---------------------------
resource "google_cloud_run_service_iam_binding" "public" {
  location = google_cloud_run_v2_service.app.location
  service  = google_cloud_run_v2_service.app.name
  role     = "roles/run.invoker"
  members = [
    "allUsers"
  ]
}

# ---------------------------
# Outputs
# ---------------------------
output "service_url" {
  value       = google_cloud_run_v2_service.app.uri
  description = "The URL of the Cloud Run service"
}

output "artifact_registry_url" {
  value       = "${var.region}-docker.pkg.dev/${var.project_id}/{{.ServiceName}}"
  description = "The URL of the Artifact Registry repository"
}

output "service_name" {
  value       = google_cloud_run_v2_service.app.name
  description = "The name of the Cloud Run service"
}
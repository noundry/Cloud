# Google Cloud Deployment Guide

Deploy .NET applications to Google Cloud using Cloud Run and Artifact Registry.

## Prerequisites

- Google Cloud CLI (`gcloud`) configured
- Docker installed and running
- Terraform >= 1.0
- Google Cloud project with billing enabled

## Required APIs

Enable these APIs in your project:
```bash
gcloud services enable \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com \
  iam.googleapis.com
```

## IAM Permissions Required

Your user needs these roles:
- `Cloud Run Admin`
- `Artifact Registry Admin` 
- `Service Account Admin`
- `Project IAM Admin`

## Step-by-Step Deployment

### 1. Create Project
```bash
ndc create dotnet-webapp-gcp --name my-api
cd my-api
```

### 2. Set Project Configuration
```bash
export PROJECT_ID="your-gcp-project-id"
export REGION="us-central1"  # or your preferred region
```

### 3. Deploy Infrastructure
```bash
cd terraform
terraform init
terraform plan -var="project_id=$PROJECT_ID"
terraform apply -var="project_id=$PROJECT_ID"
```

This creates:
- Artifact Registry repository
- Cloud Run service (initially failing - no image)
- Service account and IAM bindings

### 4. Build and Push Container
```bash
# Build the container
docker build -t my-api .

# Tag for Artifact Registry
docker tag my-api $REGION-docker.pkg.dev/$PROJECT_ID/my-api/my-api:latest

# Configure Docker for Artifact Registry
gcloud auth configure-docker $REGION-docker.pkg.dev

# Push to Artifact Registry
docker push $REGION-docker.pkg.dev/$PROJECT_ID/my-api/my-api:latest
```

### 5. Verify Deployment
```bash
# Get service URL
terraform output service_url

# Test the application
curl $(terraform output -raw service_url)
curl $(terraform output -raw service_url)/health
```

## Configuration Options

### Scaling Configuration
Edit `terraform/variables.tf`:
```hcl
variable "min_instances" {
  default = 0   # Scale to zero for cost savings
}

variable "max_instances" {
  default = 100  # Maximum instances
}
```

### Resource Allocation
```hcl
variable "cpu" {
  default = "1000m"  # 1 CPU (1000m = 1 CPU)
}

variable "memory" {
  default = "2Gi"    # 2GB memory
```

### Concurrency Settings
Add to `main.tf`:
```hcl
resource "google_cloud_run_v2_service" "app" {
  # ... existing configuration

  template {
    # ... existing configuration
    
    max_instance_request_concurrency = 80  # Requests per instance
    
    containers {
      # ... existing configuration
    }
  }
}
```

### Environment Variables
```hcl
# In main.tf containers block
env {
  name  = "CUSTOM_VAR"
  value = "production"
}

# Or from Secret Manager
env {
  name = "SECRET_VAR"
  value_source {
    secret_key_ref {
      secret  = google_secret_manager_secret.db_password.secret_id
      version = "latest"
    }
  }
}
```

## Monitoring and Logs

### Cloud Logging
```bash
# View logs
gcloud logs read "resource.type=cloud_run_revision AND resource.labels.service_name=my-api" \
  --limit 50 --format="table(timestamp,textPayload)"

# Follow logs
gcloud logs tail "resource.type=cloud_run_revision AND resource.labels.service_name=my-api"
```

### Cloud Monitoring
Add custom metrics:
```csharp
// Program.cs - Add OpenTelemetry for Google Cloud
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddGoogleCloudExporter());
```

### Structured Logging
```csharp
// Program.cs
using Google.Cloud.Logging.V2;

builder.Logging.AddGoogleCloud();

app.MapGet("/", (ILogger<Program> logger) => {
    logger.LogInformation("Request received");
    return Results.Ok("Hello World");
});
```

## Security

### Private Services
For internal-only services:
```hcl
resource "google_cloud_run_v2_service" "app" {
  # ... existing configuration
  
  ingress = "INGRESS_TRAFFIC_INTERNAL_ONLY"
}

# Remove public access
# resource "google_cloud_run_service_iam_binding" "public" {
#   # Comment out or remove this resource
# }
```

### VPC Connector
For accessing private resources:
```hcl
resource "google_vpc_access_connector" "connector" {
  name          = "my-api-connector"
  ip_cidr_range = "10.8.0.0/28"
  network       = "default"
  region        = var.region
}

resource "google_cloud_run_v2_service" "app" {
  # ... existing configuration
  
  template {
    vpc_access {
      connector = google_vpc_access_connector.connector.id
      egress    = "ALL_TRAFFIC"
    }
  }
}
```

## Troubleshooting

### Service Not Starting
```bash
# Check service status
gcloud run services describe my-api --region=$REGION

# Check revision logs
gcloud run revisions list --service=my-api --region=$REGION
gcloud logs read "resource.labels.revision_name=my-api-00001" --limit=50
```

### Container Build Issues
```bash
# Test locally
docker build -t my-api .
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://0.0.0.0:8080 my-api

# Build in Cloud Build
gcloud builds submit --tag $REGION-docker.pkg.dev/$PROJECT_ID/my-api/my-api:latest
```

### Permission Issues
```bash
# Check service account permissions
gcloud projects get-iam-policy $PROJECT_ID \
  --flatten="bindings[].members" \
  --filter="bindings.members:my-api-sa@*"
```

## Cost Optimization

### Scale-to-Zero
```hcl
variable "min_instances" {
  default = 0  # Scale to zero when no traffic
}
```

### CPU Allocation
```hcl
# Only allocate CPU during requests (default)
template {
  containers {
    resources {
      cpu_idle = false  # Don't allocate CPU when idle
    }
  }
}
```

### Request Timeout
```hcl
template {
  timeout = "60s"  # Shorter timeout for faster scale-down
}
```

## Cleanup

```bash
# Destroy all resources
cd terraform
terraform destroy -var="project_id=$PROJECT_ID"
```

This removes the Cloud Run service, Artifact Registry, and service accounts.
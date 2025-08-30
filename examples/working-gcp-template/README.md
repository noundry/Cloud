# NDC Google Cloud Platform Example

This example demonstrates deploying a .NET 9 API to Google Cloud Run with supporting infrastructure.

## Architecture

- **Local Development**: Aspire orchestrates PostgreSQL, Redis, and other services in containers
- **Production**: API container deploys to Google Cloud Run with managed cloud services

## What's Included

- **API Application**: .NET 9 minimal API with health checks, OpenAPI
- **Database**: Cloud SQL PostgreSQL instance
- **Cache**: Cloud Memorystore Redis
- **Storage**: Cloud Storage bucket
- **Infrastructure**: Complete Terraform configuration
- **Container Registry**: Artifact Registry for Docker images

## Prerequisites

1. **Google Cloud CLI**: Install and authenticate
   ```bash
   # Install gcloud CLI
   curl https://sdk.cloud.google.com | bash
   
   # Authenticate
   gcloud auth login
   gcloud auth application-default login
   
   # Set project
   gcloud config set project YOUR_PROJECT_ID
   ```

2. **Enable APIs**:
   ```bash
   gcloud services enable run.googleapis.com
   gcloud services enable artifactregistry.googleapis.com
   gcloud services enable sqladmin.googleapis.com
   gcloud services enable redis.googleapis.com
   ```

3. **Required Tools**:
   - .NET 9.0 SDK
   - Docker Desktop
   - Terraform >= 1.0

## Local Development

1. **Start Aspire orchestration**:
   ```bash
   dotnet run --project src/MyApp.AppHost
   ```

2. **Access services**:
   - API: http://localhost:8080
   - Aspire Dashboard: https://localhost:17001

3. **Test the API**:
   ```bash
   # Health check
   curl http://localhost:8080/health
   
   # Create user
   curl -X POST http://localhost:8080/users \
     -H "Content-Type: application/json" \
     -d '{"name":"John Doe","email":"john@example.com"}'
   
   # Get users
   curl http://localhost:8080/users
   ```

## Google Cloud Deployment

### 1. Configure Infrastructure

1. **Set variables**:
   ```bash
   cd terraform
   
   # Copy and edit terraform.tfvars
   cp terraform.tfvars.example terraform.tfvars
   ```

2. **Edit terraform.tfvars**:
   ```hcl
   project_id   = "your-gcp-project-id"
   service_name = "myapp-api"
   region       = "us-central1"
   
   # Database configuration
   enable_database = true
   database_tier   = "db-f1-micro"
   
   # Cache configuration
   enable_cache         = true
   redis_memory_size_gb = 1
   
   # Storage
   enable_storage = true
   ```

3. **Deploy infrastructure**:
   ```bash
   terraform init
   terraform plan
   terraform apply
   ```

### 2. Build and Deploy Container

1. **Configure Docker for Artifact Registry**:
   ```bash
   # Get repository URL from terraform output
   REPO_URL=$(terraform output -raw artifact_registry_url)
   
   # Configure Docker authentication
   gcloud auth configure-docker us-central1-docker.pkg.dev
   ```

2. **Build and push container**:
   ```bash
   cd ..
   
   # Build the API container (note: builds only API, not AppHost)
   docker build -t $REPO_URL/myapp-api:latest .
   
   # Push to Artifact Registry
   docker push $REPO_URL/myapp-api:latest
   ```

3. **Deploy to Cloud Run**:
   ```bash
   # Cloud Run automatically deploys from Artifact Registry
   # Check deployment status
   cd terraform && terraform refresh
   
   # Get service URL
   echo "Service URL: $(terraform output -raw service_url)"
   ```

### 3. Test Production Deployment

```bash
# Get the service URL
SERVICE_URL=$(cd terraform && terraform output -raw service_url)

# Test health endpoint
curl $SERVICE_URL/health

# Test API endpoints
curl -X POST $SERVICE_URL/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Production User","email":"prod@example.com"}'

curl $SERVICE_URL/users
```

## Infrastructure Components

### Cloud Run Service
- **CPU**: 1 vCPU (1000m)
- **Memory**: 2 GiB
- **Auto-scaling**: 1-5 instances
- **Health checks**: `/health` endpoint
- **VPC**: Private networking to database and cache

### Cloud SQL PostgreSQL
- **Version**: PostgreSQL 15
- **Instance**: db-f1-micro (development) / db-n1-standard-1 (production)
- **Storage**: 20 GB SSD
- **Backup**: Automated daily backups
- **Security**: Private IP, no public access

### Cloud Memorystore Redis
- **Version**: Redis 7.0
- **Tier**: Basic (development) / Standard (production with HA)
- **Memory**: 1 GB
- **Security**: Private VPC access only

### Cloud Storage
- **Location**: us-central1 (or your preferred region)
- **Storage Class**: Standard
- **Access**: IAM-controlled, service account access

## Environment Variables

The application automatically receives these environment variables in production:

```bash
# Database
DATABASE_HOST=<cloud-sql-private-ip>
DATABASE_NAME=myapp_db
DATABASE_USER=appuser
DATABASE_PASSWORD=<secure-password>

# Cache
REDIS_CONNECTION_STRING=<redis-host>:6379

# Storage
GCS_BUCKET_NAME=<bucket-name>
GOOGLE_PROJECT_ID=<project-id>

# Application
ASPNETCORE_ENVIRONMENT=Production
```

## Monitoring and Logging

1. **Cloud Run Logs**:
   ```bash
   gcloud logs tail --service myapp-api --region us-central1
   ```

2. **Application Metrics**: Available in Google Cloud Console under Cloud Run metrics

3. **Health Checks**: Built-in health monitoring via Cloud Run

## Scaling Configuration

The deployment includes auto-scaling configuration:

- **Minimum instances**: 1
- **Maximum instances**: 5
- **CPU utilization target**: 80%
- **Concurrency**: 80 requests per instance

Modify in `terraform/variables.tf` as needed.

## Security Features

- **Service Account**: Dedicated service account with minimal required permissions
- **Private Networking**: Database and cache accessible only via private VPC
- **HTTPS**: Cloud Run provides HTTPS termination
- **IAM**: Resource-level access control
- **Container Security**: Non-root user, minimal attack surface

## Cost Optimization

This configuration is optimized for development and small production workloads:

- **Cloud Run**: Pay per request, scales to zero
- **Cloud SQL**: db-f1-micro is eligible for always-free tier
- **Redis**: Basic tier, 1GB memory
- **Storage**: Standard storage with lifecycle rules

## Cleanup

To avoid ongoing charges:

```bash
cd terraform
terraform destroy
```

This will remove all created resources.

## Troubleshooting

### Common Issues

1. **API Permission Errors**: Ensure required APIs are enabled
2. **Database Connection Issues**: Check VPC connectivity
3. **Container Build Failures**: Verify Dockerfile and dependencies

### Debugging

1. **View Cloud Run logs**:
   ```bash
   gcloud logs tail --service myapp-api --region us-central1
   ```

2. **Check service health**:
   ```bash
   gcloud run services describe myapp-api --region us-central1
   ```

3. **Test database connectivity**:
   ```bash
   gcloud sql connect <instance-name> --user=appuser
   ```

## Next Steps

- Add CI/CD pipeline for automated deployments
- Configure custom domain and SSL certificate
- Add monitoring and alerting
- Implement proper backup and disaster recovery
- Add staging environment
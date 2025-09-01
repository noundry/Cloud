# MyApp - AWS Deployment Example

This is a **working example** of the NDC AWS template with Aspire integration.

## 🏗️ **Architecture**

- **AppHost**: Aspire orchestration for local development (NOT deployed)
- **API**: Web API application (ONLY this gets deployed to AWS)
- **ServiceDefaults**: Shared configuration and telemetry
- **Terraform**: AWS infrastructure (RDS, ElastiCache, S3, App Runner)

## 🚀 **Quick Start**

### 1. Local Development
```bash
# Start Aspire orchestration (automatically starts PostgreSQL, Redis, MinIO)
dotnet run --project src/MyApp.AppHost

# ✅ API: http://localhost:8080
# ✅ Aspire dashboard: https://localhost:17001
# ✅ Services automatically configured and running
```

### 2. Test Locally
```bash
# Health check
curl http://localhost:8080/health

# Database endpoints
curl http://localhost:8080/users
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}'

# Cache endpoints  
curl -X POST http://localhost:8080/cache \
  -H "Content-Type: application/json" \
  -d '{"key":"test","value":"hello world"}'
curl http://localhost:8080/cache/test
```

### 3. Deploy to AWS

#### Prerequisites
```bash
# Install required tools
aws --version      # AWS CLI
terraform --version  # Terraform

# Configure AWS credentials
aws configure
```

#### Deploy Infrastructure
```bash
cd terraform

# Initialize and apply
terraform init
terraform apply

# This creates:
# ✅ ECR repository for container images
# ✅ RDS PostgreSQL instance
# ✅ ElastiCache Redis cluster
# ✅ S3 bucket for file storage
# ✅ App Runner service with auto-scaling
# ✅ IAM roles with least-privilege access
```

#### Deploy Application
```bash
# Build API container (Dockerfile builds API project only)
cd ..
docker build -t myapp .

# Get ECR repository URL
ECR_URL=$(cd terraform && terraform output -raw ecr_repository_url)

# Login to ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin $ECR_URL

# Tag and push
docker tag myapp $ECR_URL:latest
docker push $ECR_URL:latest

# App Runner automatically deploys the new image
```

#### Access Production API
```bash
# Get service URL
SERVICE_URL=$(cd terraform && terraform output -raw app_runner_service_url)
echo "API URL: $SERVICE_URL"

# Test production API
curl $SERVICE_URL/health
curl $SERVICE_URL/users
```

## ⚙️ **Configuration**

### Local Development (appsettings.json)
The API automatically connects to Aspire-orchestrated services:
- PostgreSQL container
- Redis container  
- MinIO container (S3-compatible)
- MailHog SMTP server

### Production (Environment Variables)
Terraform automatically configures these environment variables in App Runner:
- `ConnectionStrings__Database`: RDS PostgreSQL connection
- `ConnectionStrings__Redis`: ElastiCache endpoint
- `S3__BucketName`: S3 bucket name
- `S3__Region`: AWS region

## 🎯 **Key Benefits**

### ✅ **Amazing Local Development**
- One command starts everything: `dotnet run --project src/MyApp.AppHost`
- All services automatically configured and connected
- Aspire dashboard for monitoring and debugging
- Hot reload and full debugging support

### ✅ **Clean Production Deployment**
- Only API container gets deployed (lightweight)
- Connects to managed AWS services (RDS, ElastiCache, S3)
- No Aspire dependencies in production
- Auto-scaling and high availability built-in

### ✅ **Configuration-Driven**
- Same API code works locally and in production
- Services discovered via configuration
- Easy to switch between local and cloud services
- Environment-specific settings

## 🔧 **Customization**

### Add More Services
Edit `terraform/variables.tf` to enable additional services:
```hcl
variable "include_storage" {
  default = true    # Enable S3 bucket
}

variable "include_cache" {
  default = true    # Enable ElastiCache
}
```

### Modify Resources
```hcl
variable "database_instance_class" {
  default = "db.t3.small"    # Larger database instance
}

variable "cache_node_type" {
  default = "cache.t3.small"  # Larger cache instance
}
```

## 🧹 **Cleanup**
```bash
# Destroy all AWS resources
cd terraform
terraform destroy
```

## 📋 **What This Example Demonstrates**

1. **Aspire Local Orchestration**: Rich development environment with zero configuration
2. **API-Only Deployment**: Lightweight production container deployment
3. **Service Discovery**: Configuration-driven connection to local/cloud services
4. **Infrastructure as Code**: Complete AWS infrastructure with Terraform
5. **Production-Ready**: Security, auto-scaling, and monitoring built-in

This example shows the complete NDC vision in action with a working AWS deployment!
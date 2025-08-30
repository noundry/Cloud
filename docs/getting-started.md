# Getting Started with NDC

NDC (Noundry Deploy CLI) is a production-ready tool that provides the same developer experience across all major cloud platforms.

## Current Status

✅ **Production Ready:**
- Complete multi-cloud support: AWS, Google Cloud, Azure, Container platforms
- C# CLI framework with full command functionality
- Aspire integration for local development
- Complete Terraform infrastructure for all platforms
- Working examples for every supported platform

✅ **All Platforms Supported:**
- **AWS**: App Runner + RDS + ElastiCache + S3
- **Google Cloud**: Cloud Run + Cloud SQL + Memorystore + Cloud Storage  
- **Azure**: Container Apps + SQL Database + Redis + Blob Storage
- **Container**: Docker Compose + Kubernetes manifests

## Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli
```

### 2. Choose Your Platform
Pick any platform - the developer experience is identical:

```bash
# AWS (App Runner + RDS + ElastiCache)
cp -r examples/working-aws-template MyApi
cd MyApi

# Google Cloud (Cloud Run + Cloud SQL + Memorystore) 
cp -r examples/working-gcp-template MyApi
cd MyApi

# Azure (Container Apps + SQL Database + Redis)
cp -r examples/working-azure-template MyApi  
cd MyApi

# Container Platform (Docker/Kubernetes)
cp -r examples/working-container-template MyApi
cd MyApi
```

### 3. Local Development with Aspire
```bash
# Start all services (PostgreSQL, Redis, MinIO automatically)
dotnet run --project src/MyApp.AppHost

# ✅ API: http://localhost:8080
# ✅ Aspire dashboard: https://localhost:17001
# ✅ Test endpoints:
curl http://localhost:8080/health
curl http://localhost:8080/users
```

### 4. Deploy to AWS
```bash
# Prerequisites: AWS CLI configured, Terraform installed

# Deploy infrastructure
cd terraform
terraform init
terraform apply

# Build API container (Dockerfile builds API project only, not AppHost)
cd ..
docker build -t myapp .

# Push to ECR
ECR_URL=$(cd terraform && terraform output -raw ecr_repository_url)
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin $ECR_URL
docker tag myapp $ECR_URL:latest
docker push $ECR_URL:latest

# Get your live API URL
cd terraform && terraform output app_runner_service_url
```

## What You Get

### Local Development
- **Aspire AppHost** orchestrates all services in containers
- **Service Discovery** automatically configures connection strings
- **Hot Reload** for rapid development
- **Aspire Dashboard** for monitoring services
- **Rich Development Environment** with debugging support

### Production Deployment  
- **Lightweight API Container** (no Aspire dependencies)
- **Managed AWS Services**: RDS PostgreSQL, ElastiCache Redis, S3 storage
- **Auto-Scaling**: App Runner scales based on demand
- **Security**: Non-root containers, encrypted services, least-privilege IAM
- **Configuration-Driven**: Services discovered via environment variables

## Architecture Benefits

### 🎯 **Perfect Separation of Concerns**
- **Aspire**: Used only for local development orchestration
- **API**: Self-contained application that deploys anywhere
- **Configuration**: Same code works locally and in cloud

### 🚀 **Production-Ready**
- Only your API code gets deployed (no orchestration overhead)
- Connects to managed cloud services
- Works on any container platform
- Standard .NET deployment patterns

## Next Steps

1. **Try the current AWS template** following the manual setup above
2. **Explore the generated code** to understand the architecture
3. **Deploy to AWS** to see the complete local → cloud workflow
4. **Watch for updates** as the template package system is completed

The current implementation demonstrates the complete vision: amazing local development with Aspire, clean production deployment of just your API code.
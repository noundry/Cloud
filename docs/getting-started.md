# Getting Started with NDC

A simple guide to using the current NDC implementation.

## Current Status

✅ **What's Working:**
- C# CLI framework (System.CommandLine + Spectre.Console)
- AWS template with full Aspire integration
- Complete Terraform for AWS App Runner + managed services
- Local development with PostgreSQL, Redis, MinIO orchestration

🚧 **In Development:**
- Template package publishing to NuGet
- Multi-cloud templates (GCP, Azure)
- Automated `ndc create` command

## Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli
```

### 2. Use the AWS Template (Manual Setup)
```bash
# Copy the working template
cp -r NDC.Templates.WebApp/content/webapp-aws MyApp
cd MyApp

# Update template placeholders (required)
# Replace "Company.WebApplication1" with "MyApp" in all files
find . -type f \( -name "*.cs" -o -name "*.json" -o -name "*.csproj" -o -name "*.sln" \) \
  -exec sed -i 's/Company\.WebApplication1/MyApp/g' {} \;

# Rename directories and files
find . -name "*Company.WebApplication1*" -type d | while read dir; do
  new_dir="${dir//Company.WebApplication1/MyApp}"
  mv "$dir" "$new_dir" 2>/dev/null || true
done

find . -name "*Company.WebApplication1*" -type f | while read file; do
  new_file="${file//Company.WebApplication1/MyApp}"
  mv "$file" "$new_file" 2>/dev/null || true
done
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
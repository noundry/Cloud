# NDC Quick Start Cheat Sheet

## 📦 Installation (One-Time Setup)

```bash
# Install CLI
dotnet tool install --global NDC.Cli

# Install template packages (choose what you need)
dotnet new install NDC.Templates.WebApp.Aws      # AWS templates
dotnet new install NDC.Templates.WebApp.Gcp      # Google Cloud templates  
dotnet new install NDC.Templates.WebApp.Azure    # Azure templates
dotnet new install NDC.Templates.WebApp.Docker   # Docker templates
dotnet new install NDC.Templates.WebApp.K8s      # Kubernetes templates
```

---

## 🚀 **30-Second Start**

```bash
# 1. Create project (replace 'aws' with gcp/azure/docker/k8s)
ndc create webapp-aws --name MyApp --services database,cache

# 2. Start local development  
cd MyApp && dotnet run --project src/MyApp.AppHost

# 3. Visit Aspire dashboard
open https://localhost:17001

# 4. Test your API
curl http://localhost:8080/health
```

---

## 📋 **Common Commands**

### List & Discovery
```bash
ndc list                           # Show all templates
ndc list --all                     # Include not-installed templates
```

### Project Creation
```bash
# Basic patterns
ndc create webapp-aws --name MyApp                    # Basic web app
ndc create webapp-aws --name MyApp --services all     # Full-featured app
ndc create webapp-aws --name MyApp --services database,cache  # Specific services

# With customization
ndc create webapp-gcp --name MyApp \
  --database MySQL \
  --framework net8.0 \
  --port 5000 \
  --services database,cache,storage
```

### Deployment
```bash
ndc deploy --platform aws                 # Deploy to configured platform
ndc deploy --platform aws --dry-run       # See what would be deployed
ndc deploy --platform k8s --environment production  # Deploy to specific environment
```

---

## 🎯 **By Use Case**

### Simple CRUD API
```bash
ndc create webapp-aws --name CrudApi --services database
```

### High-Performance API
```bash
ndc create webapp-gcp --name FastApi \
  --services database,cache \
  --cpu 2000m \
  --max-instances 50
```

### File Upload Service  
```bash
ndc create webapp-azure --name FileApi \
  --services database,storage,queue,jobs
```

### Notification System
```bash
ndc create webapp-aws --name NotifyApi \
  --services database,cache,mail,queue,worker \
  --include-worker true
```

### Microservice
```bash
ndc create webapp-k8s --name UserService \
  --services database,cache \
  --port 8080
```

---

## ☁️ **By Cloud Platform**

### AWS (App Runner + Managed Services)
```bash
# Simple
ndc create webapp-aws --name MyApp --services database

# Full-featured  
ndc create webapp-aws --name MyApp --services all

# Custom
ndc create webapp-aws --name MyApp \
  --database PostgreSQL \
  --services cache,storage,mail,queue \
  --min-instances 2
```

### Google Cloud (Cloud Run + Managed Services)
```bash
# Simple
ndc create webapp-gcp --name MyApp --services database

# Full-featured
ndc create webapp-gcp --name MyApp --services all

# Custom
ndc create webapp-gcp --name MyApp \
  --database MySQL \
  --services cache,storage,mail \
  --cpu 2000m \
  --memory 4Gi
```

### Azure (Container Apps + Managed Services)
```bash
# Simple
ndc create webapp-azure --name MyApp --services database

# Full-featured
ndc create webapp-azure --name MyApp --services all

# Custom
ndc create webapp-azure --name MyApp \
  --database SqlServer \
  --services cache,storage,mail,jobs \
  --cpu 2.0 \
  --memory 4.0Gi
```

---

## 🐳 **By Container Platform**

### Docker Compose
```bash
# Local + Production Docker deployment
ndc create webapp-docker --name MyApp --services database,cache,storage

cd MyApp
docker compose up                          # Local development
docker compose -f docker-compose.prod.yml up  # Production with external services
```

### Kubernetes
```bash
# Any Kubernetes cluster
ndc create webapp-k8s --name MyApp --services database,cache,storage

cd MyApp
kubectl apply -f k8s/                     # Deploy to current cluster context
```

---

## 🛠️ **Development Workflow**

### Day 1: Create and Run
```bash
ndc create webapp-aws --name MyApp --services database,cache
cd MyApp
dotnet run --project src/MyApp.AppHost
# ✅ Immediate full-stack development environment
```

### Day 2: Add Services
```bash
# Edit appsettings.json to enable more services
# Update terraform/variables.tf if deploying to cloud
terraform apply  # Add new cloud services
```

### Day 3: Deploy
```bash
cd terraform && terraform apply           # Deploy infrastructure
docker build -t myapp . && docker push   # Deploy application
```

---

## 📊 **Service Combinations Guide**

### Minimal (database only)
```bash
--services database
# Local: PostgreSQL container
# Cloud: RDS/Cloud SQL/Azure PostgreSQL
```

### Standard Web App
```bash
--services database,cache
# Local: PostgreSQL + Redis containers  
# Cloud: Managed database + managed cache
```

### Content Management
```bash
--services database,cache,storage
# Local: PostgreSQL + Redis + MinIO
# Cloud: Managed DB + cache + S3/GCS/Blob
```

### Full-Stack Application
```bash
--services database,cache,storage,mail,queue,jobs --include-worker true
# Local: All services in containers + worker project
# Cloud: All managed services + worker deployment
```

---

## ⚡ **Quick Troubleshooting**

### Local Development Issues
```bash
# Check Aspire dashboard
dotnet run --project src/MyApp.AppHost
# Visit: https://localhost:17001

# Check individual containers
docker ps                        # See running containers
docker logs <container-name>     # Check container logs
```

### Deployment Issues
```bash
# Validate project structure
ndc deploy --platform aws --dry-run

# Check cloud credentials
aws sts get-caller-identity      # AWS
gcloud auth list                 # Google Cloud
az account show                  # Azure

# Check Terraform state
cd terraform && terraform plan
```

### Common Fixes
```bash
# Reset local environment
docker compose down -v           # Clean containers and volumes
dotnet run --project src/MyApp.AppHost  # Restart Aspire

# Update templates
dotnet new uninstall NDC.Templates.WebApp.Aws
dotnet new install NDC.Templates.WebApp.Aws

# Update CLI
dotnet tool update --global NDC.Cli
```

---

## 🎨 **Customization Patterns**

### Override Defaults
```bash
# Custom framework and resources
ndc create webapp-aws --name MyApp \
  --framework net8.0 \
  --port 5000 \
  --cpu 2048 \
  --memory 4096 \
  --min-instances 3

# Custom database
ndc create webapp-azure --name MyApp \
  --database SqlServer \
  --services cache,storage
```

### Development vs Production
```bash
# Development (minimal resources)
ndc create webapp-aws --name DevApp \
  --services database,cache \
  --min-instances 1 \
  --max-instances 3

# Production (scaled resources)  
ndc create webapp-aws --name ProdApp \
  --services all \
  --min-instances 5 \
  --max-instances 100 \
  --cpu 4096 \
  --memory 8192
```

This cheat sheet covers 90% of common NDC usage patterns!
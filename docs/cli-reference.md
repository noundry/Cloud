# NDC CLI Command Reference

Complete reference for all NDC CLI commands, options, and parameters.

## 📋 **Core Commands**

### `ndc list`
List available templates and their installation status.

```bash
ndc list                    # Show installed templates
ndc list --all             # Show all templates (including not installed)
```

**Output:**
```
Available NDC Templates:

📦 webapp-aws
   Cloud: ☁️ AWS  
   Type: Aspire
   Description: Web application for AWS App Runner (Aspire local DX)
   Status: ✅ Installed

📦 webapp-gcp
   Cloud: 🌐 Google Cloud
   Type: Aspire  
   Description: Web application for Google Cloud Run (Aspire local DX)
   Status: 🟨 Available

Usage:
  ndc create <template> --name <project-name>

Example:
  ndc create webapp-aws --name my-api
```

---

### `ndc create`
Create a new project from a template.

```bash
ndc create <template> --name <project-name> [options]
```

#### **Required Parameters**
- `<template>`: Template name (see `ndc list`)
- `--name, -n`: Project name (required)

#### **Core Options**
```bash
--name MyApp                    # Project name (required)
--output ./projects            # Output directory (default: current directory)  
--framework net9.0             # .NET framework (net9.0, net8.0) - default: net9.0
--port 8080                    # Application port (default: 8080)
```

#### **Database Options**
```bash
--database PostgreSQL          # Database type: PostgreSQL, MySQL, SqlServer, None
                               # Default: PostgreSQL (AWS/GCP), SqlServer (Azure)
```

#### **Service Options**
```bash
# Individual service flags
--include-cache                # Include Redis cache service
--include-storage              # Include S3-compatible storage service  
--include-mail                 # Include email/SMTP service
--include-queue                # Include message queue service
--include-jobs                 # Include background jobs (Hangfire)
--include-worker               # Include worker service project

# Convenience flag  
--services <list>              # Comma-separated services
                               # Options: database,cache,storage,mail,queue,jobs,worker,all
```

#### **Cloud Configuration**
```bash
--cpu 1024                     # CPU allocation (cloud-specific format)
--memory 2048                  # Memory allocation (cloud-specific format)
--min-instances 1              # Minimum instances (default: 1)
--max-instances 10             # Maximum instances (default: 10) 
--region us-east-1             # Cloud region (uses cloud defaults)
```

#### **Examples**
```bash
# Basic web app
ndc create webapp-aws --name SimpleApi

# Database + cache
ndc create webapp-gcp --name FastApi --services database,cache

# Full-featured application
ndc create webapp-azure --name CompleteApp \
  --services all \
  --framework net8.0 \
  --min-instances 3

# Custom configuration
ndc create webapp-k8s --name CustomApi \
  --database MySQL \
  --include-cache true \
  --include-storage true \
  --port 5000
```

---

### `ndc install`
Install template packages from NuGet.

```bash
ndc install <package-name> [options]
```

#### **Parameters**
- `<package-name>`: NuGet package name

#### **Options**
```bash
--version 1.2.0               # Specific version to install
--prerelease                  # Include prerelease versions
```

#### **Available Packages**
```bash
# Install individual cloud packages
ndc install NDC.Templates.WebApp.Aws
ndc install NDC.Templates.WebApp.Gcp
ndc install NDC.Templates.WebApp.Azure

# Install container platform packages
ndc install NDC.Templates.WebApp.Docker
ndc install NDC.Templates.WebApp.K8s

# Install PaaS platform packages
ndc install NDC.Templates.WebApp.Railway
ndc install NDC.Templates.WebApp.Render
ndc install NDC.Templates.WebApp.Fly
```

#### **Examples**
```bash
# Install latest AWS templates
ndc install NDC.Templates.WebApp.Aws

# Install specific version
ndc install NDC.Templates.WebApp.Gcp --version 1.0.0

# Install prerelease
ndc install NDC.Templates.WebApp.Azure --prerelease
```

---

### `ndc uninstall`
Uninstall template packages.

```bash
ndc uninstall <package-name>
```

#### **Examples**
```bash
ndc uninstall NDC.Templates.WebApp.Aws
ndc uninstall NDC.Templates.WebApp.Docker
```

---

### `ndc deploy`
Deploy your application to the configured platform.

```bash
ndc deploy --platform <platform> [options]
```

#### **Parameters**
```bash
--platform <platform>         # Deployment platform (required)
                               # Options: aws, gcp, azure, docker, k8s
--project ./MyApp              # Project directory (default: current directory)
--environment production       # Environment name (default: development)
```

#### **Deployment Options**
```bash
--build                       # Build and push container image (default: true)
--infrastructure              # Deploy infrastructure with Terraform (default: true)  
--dry-run                     # Show deployment plan without executing
```

#### **Examples**
```bash
# Deploy to AWS
ndc deploy --platform aws

# Deploy to specific environment
ndc deploy --platform gcp --environment production

# Deploy without building (image already pushed)
ndc deploy --platform azure --build false

# See what would be deployed
ndc deploy --platform k8s --dry-run
```

---

## 🎯 **Service Configuration Reference**

### **Database Options**
```bash
--database PostgreSQL         # PostgreSQL (default for AWS/GCP)
--database MySQL              # MySQL/MariaDB
--database SqlServer          # SQL Server (default for Azure)
--database None               # No database
```

**Local:** Runs in Docker containers via Aspire
**Cloud:** Maps to managed database services (RDS, Cloud SQL, Azure Database)

### **Cache Service**
```bash
--include-cache               # Enables Redis caching
```

**Local:** Redis container via Aspire
**Cloud:** ElastiCache (AWS), Memorystore (GCP), Azure Cache for Redis

### **Storage Service**
```bash
--include-storage             # Enables S3-compatible storage
```

**Local:** MinIO container (S3-compatible) via Aspire  
**Cloud:** S3 (AWS), Cloud Storage (GCP), Blob Storage (Azure)

### **Email Service**
```bash
--include-mail                # Enables email/SMTP service
```

**Local:** MailHog SMTP server via Aspire
**Cloud:** SES (AWS), SendGrid (GCP), Communication Services (Azure)

### **Message Queue**
```bash
--include-queue               # Enables message queue service
```

**Local:** RabbitMQ container via Aspire
**Cloud:** SQS + EventBridge (AWS), Pub/Sub (GCP), Service Bus (Azure)

### **Background Jobs**
```bash
--include-jobs                # Enables Hangfire background jobs
```

**Local:** Hangfire with in-memory storage
**Cloud:** Hangfire with database storage (uses configured database)

### **Worker Service**
```bash
--include-worker              # Adds background worker service project
```

Adds a separate worker service project for background processing, message consumption, etc.

---

## 🚀 **Template Categories**

### **Cloud-Native Templates**
| Template | Platform | Services | Best For |
|----------|----------|----------|----------|
| `webapp-aws` | AWS App Runner | RDS, ElastiCache, S3, SES, SQS | Serverless APIs |
| `webapp-gcp` | Cloud Run | Cloud SQL, Memorystore, GCS, Pub/Sub | Global scale APIs |
| `webapp-azure` | Container Apps | PostgreSQL, Azure Cache, Blob, Service Bus | Enterprise APIs |

### **Container Platform Templates**  
| Template | Platform | Services | Best For |
|----------|----------|----------|----------|
| `webapp-docker` | Docker Compose | External services | Self-hosted deployment |
| `webapp-k8s` | Kubernetes | External services | Microservices architecture |

### **PaaS Platform Templates**
| Template | Platform | Services | Best For |
|----------|----------|----------|----------|
| `webapp-railway` | Railway | Railway add-ons | Rapid prototyping |
| `webapp-render` | Render | Render services | Full-stack apps |
| `webapp-fly` | Fly.io | Fly services | Global edge deployment |
| `webapp-heroku` | Heroku | Heroku add-ons | Traditional PaaS |

---

## ⚙️ **Advanced Configuration**

### **Resource Sizing by Cloud**

#### AWS
```bash
--cpu 256|512|1024|2048|4096    # vCPU (1024 = 1 vCPU)
--memory 512|1024|2048|3072|4096|8192|12288  # MB
```

#### Google Cloud  
```bash
--cpu 1000m|2000m|4000m         # Millicores (1000m = 1 CPU)
--memory 1Gi|2Gi|4Gi|8Gi       # Memory
```

#### Azure
```bash
--cpu 0.5|1.0|1.5|2.0|4.0       # CPU cores  
--memory 1.0Gi|2.0Gi|4.0Gi|8.0Gi  # Memory
```

### **Scaling Configuration**
```bash
--min-instances 0              # Scale-to-zero (GCP, Azure)
--min-instances 1              # Always-on (AWS, default)
--max-instances 10             # Maximum scale (default: 10)
--max-instances 1000           # High-scale applications
```

### **Framework Options**
```bash
--framework net9.0             # .NET 9 (default, recommended)
--framework net8.0             # .NET 8 (LTS support)
```

---

## 🔄 **Workflow Commands**

### **Complete Project Lifecycle**
```bash
# 1. Create
ndc create webapp-aws --name MyApi --services database,cache,storage

# 2. Develop locally
cd MyApi && dotnet run --project src/MyApi.AppHost

# 3. Deploy infrastructure  
cd terraform && terraform apply

# 4. Deploy application
cd .. && ndc deploy --platform aws

# 5. Scale as needed
ndc deploy --platform aws --min-instances 5 --max-instances 100
```

### **Development to Production**
```bash
# Development
ndc create webapp-docker --name DevApp --services database,cache
cd DevApp && dotnet run --project src/DevApp.AppHost

# Staging (cloud testing)
ndc create webapp-gcp --name DevApp-staging --services database,cache
ndc deploy --platform gcp --environment staging

# Production (full scale)  
ndc create webapp-aws --name DevApp-prod --services all
ndc deploy --platform aws --environment production
```

### **Multi-Platform Deployment**
```bash
# Create once
ndc create webapp-k8s --name FlexibleApi --services database,cache

# Deploy anywhere
ndc deploy --platform k8s --environment eks        # AWS EKS
ndc deploy --platform k8s --environment gke        # Google GKE  
ndc deploy --platform k8s --environment aks        # Azure AKS
ndc deploy --platform k8s --environment on-prem    # On-premises K8s
```

---

## 🆘 **Common Command Patterns**

### **Quick Prototyping**
```bash
ndc create webapp-docker --name Prototype --services database
cd Prototype && dotnet run --project src/Prototype.AppHost
```

### **Production-Ready**  
```bash
ndc create webapp-aws --name Production \
  --services all \
  --framework net8.0 \
  --min-instances 3 \
  --max-instances 100 \
  --cpu 2048 \
  --memory 4096
```

### **Microservice**
```bash
ndc create webapp-k8s --name UserService \
  --services database,cache \
  --port 8081
```

### **Background Processing**
```bash
ndc create webapp-gcp --name ProcessingService \
  --services database,storage,queue,jobs,worker \
  --include-worker true
```

### **API Gateway**
```bash
ndc create webapp-azure --name ApiGateway \
  --services cache \
  --min-instances 2 \
  --max-instances 50
```

This reference covers every NDC CLI command and configuration option!
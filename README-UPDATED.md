# NDC (Noundry Deploy CLI)

**Aspire for Local DX + Deploy API Only**

A powerful .NET CLI that uses **Aspire for amazing local development** and deploys **only your API** to any cloud or container platform.

[![Release](https://img.shields.io/github/v/release/plsft/noundry-cloud-cli)](https://github.com/plsft/noundry-cloud-cli/releases)
[![NuGet](https://img.shields.io/nuget/v/NDC.Cli)](https://www.nuget.org/packages/NDC.Cli/)
[![License](https://img.shields.io/github/license/plsft/noundry-cloud-cli)](LICENSE)

## 🎯 **Core Philosophy**

- **🏠 Local Development**: Aspire orchestrates all services (database, cache, storage, mail, queues)
- **☁️ Production Deployment**: Only your API container gets deployed
- **⚙️ Configuration-Driven**: Services discovered via appsettings.json
- **🌍 Platform Agnostic**: Deploy to AWS, GCP, Azure, Docker, Kubernetes, Railway, etc.

---

## ⚡ **30-Second Quick Start**

```bash
# Install
dotnet tool install --global NDC.Cli
dotnet new install NDC.Templates.WebApp.Aws

# Create & run (full-stack local development)
ndc create webapp-aws --name MyApp --services database,cache,storage
cd MyApp && dotnet run --project src/MyApp.AppHost

# ✅ API at http://localhost:8080
# ✅ Aspire dashboard at https://localhost:17001
# ✅ PostgreSQL + Redis + MinIO running automatically
```

---

## 🚀 **Platform Examples**

### ☁️ **Major Clouds**

#### AWS App Runner
```bash
ndc create webapp-aws --name PaymentApi --services database,cache,storage,mail,queue
# Local: Aspire orchestration  
# Deploy: API → App Runner, PostgreSQL → RDS, Redis → ElastiCache, Storage → S3
```

#### Google Cloud Run
```bash
ndc create webapp-gcp --name AnalyticsApi --services database,cache,storage --cpu 2000m --max-instances 100
# Local: Aspire orchestration
# Deploy: API → Cloud Run, PostgreSQL → Cloud SQL, Redis → Memorystore, Storage → Cloud Storage  
```

#### Azure Container Apps
```bash
ndc create webapp-azure --name EnterpriseApi --database SqlServer --services cache,storage,mail
# Local: Aspire orchestration
# Deploy: API → Container Apps, SQL Server → Azure Database, Redis → Azure Cache
```

### 🐳 **Container Platforms**

#### Docker Compose
```bash
ndc create webapp-docker --name SelfHosted --services database,cache
# Local: Aspire orchestration
# Deploy: API container + external database/cache connections
```

#### Kubernetes
```bash
ndc create webapp-k8s --name MicroserviceApi --services database,cache,queue
# Local: Aspire orchestration  
# Deploy: API pods + external cloud services (any K8s cluster)
```

### 🌟 **Modern PaaS**

#### Railway/Render/Fly.io
```bash
ndc create webapp-railway --name StartupApi --services database,cache
ndc create webapp-render --name FullStackApp --services database,storage
ndc create webapp-fly --name GlobalApi --services database,cache,storage
# Local: Aspire orchestration
# Deploy: API container + platform-specific add-ons
```

---

## 📋 **Available Templates & Platforms**

| Template | Platform | Local Services | Production Services | Best For |
|----------|----------|----------------|-------------------|----------|
| `webapp-aws` | AWS App Runner | Aspire containers | RDS, ElastiCache, S3, SES, SQS | Serverless APIs |
| `webapp-gcp` | Google Cloud Run | Aspire containers | Cloud SQL, Memorystore, GCS, Pub/Sub | Global scale |
| `webapp-azure` | Azure Container Apps | Aspire containers | PostgreSQL, Azure Cache, Blob, Service Bus | Enterprise |
| `webapp-docker` | Docker Compose | Aspire containers | External cloud services | Self-hosted |
| `webapp-k8s` | Kubernetes | Aspire containers | External cloud services | Microservices |
| `webapp-railway` | Railway | Aspire containers | Railway add-ons | Rapid prototyping |
| `webapp-render` | Render | Aspire containers | Render services | Full-stack apps |
| `webapp-fly` | Fly.io | Aspire containers | Fly services | Global edge |

---

## 🎛️ **CLI Usage**

### **Basic Commands**
```bash
ndc list                                    # Show available templates
ndc create webapp-aws --name MyApp         # Create basic project
ndc create webapp-aws --name MyApp --services all  # Create with all services
```

### **Service Selection**
```bash
# Individual services
ndc create webapp-gcp --name MyApi \
  --database PostgreSQL \
  --include-cache \
  --include-storage \
  --include-mail

# Convenience flag
ndc create webapp-azure --name MyApi --services database,cache,storage,mail,queue,jobs

# All services
ndc create webapp-k8s --name MyApi --services all --include-worker
```

### **Platform-Specific Options**
```bash
# AWS with custom resources
ndc create webapp-aws --name ScalableApi \
  --services database,cache \
  --cpu 2048 \
  --memory 4096 \
  --min-instances 3 \
  --max-instances 100

# Google Cloud with high performance
ndc create webapp-gcp --name FastApi \
  --database MySQL \
  --services cache,storage \
  --cpu 4000m \
  --memory 8Gi

# Azure with SQL Server
ndc create webapp-azure --name EnterpriseApi \
  --database SqlServer \
  --services cache,mail \
  --cpu 2.0 \
  --memory 4.0Gi
```

### **Development Workflow**  
```bash
# 1. Create project
ndc create webapp-aws --name BlogApi --services database,cache,storage

# 2. Start local development (Aspire orchestrates everything)
cd BlogApi && dotnet run --project src/BlogApi.AppHost

# 3. Deploy to cloud (API container only)
cd terraform && terraform apply
docker build -t blog-api . && docker push $ECR_URL:latest
```

---

## 🏗️ **Architecture Benefits**

### 🎯 **Aspire for Local DX Only**
- **Rich Local Development**: All services orchestrated automatically
- **Service Discovery**: Connection strings and endpoints auto-configured
- **Aspire Dashboard**: Visual monitoring of all local services
- **No Production Aspire**: Clean, lightweight production deployments

### 🚀 **API-Only Production**  
- **Lightweight Containers**: Only your application code gets deployed
- **Configuration-Driven**: Services discovered via environment variables
- **Cloud-Native**: Connects to managed cloud services (RDS, ElastiCache, S3, etc.)
- **Platform Agnostic**: Same API runs on AWS, GCP, Azure, K8s, Docker, etc.

### ⚙️ **Configuration Flexibility**
- **Easy Service Switching**: Change database/cache providers via config
- **Environment-Specific**: Different services per environment (dev/staging/prod)
- **External Services**: Connect to any existing database/cache/storage
- **Gradual Migration**: Start simple, add services as you grow

---

## 📊 **Service Mapping**

| Service | Local (Aspire) | AWS | Google Cloud | Azure | External |
|---------|----------------|-----|--------------|-------|----------|
| **Database** | PostgreSQL/MySQL/SQL Server containers | RDS Aurora | Cloud SQL | PostgreSQL Flexible | Any managed DB |
| **Cache** | Redis container | ElastiCache | Memorystore | Azure Cache for Redis | Any Redis instance |
| **Storage** | MinIO container | S3 | Cloud Storage | Blob Storage | Any S3-compatible |
| **Email** | MailHog SMTP | SES | SendGrid | Communication Services | Any SMTP server |
| **Queue** | RabbitMQ container | SQS + EventBridge | Pub/Sub | Service Bus | Any message queue |
| **Jobs** | Hangfire (in-memory) | Hangfire (RDS) | Hangfire (Cloud SQL) | Hangfire (Azure DB) | Any job system |

---

## 📖 **Complete Documentation**

### 🚀 **Quick Guides**
- ⚡ **[5-Minute Quick Start](docs/examples/5-minute-quickstart.md)** - Get running immediately
- 📋 **[Quick Start Cheat Sheet](docs/examples/quick-start-cheatsheet.md)** - Common commands
- 🌍 **[All Platforms Examples](docs/examples/all-platforms-examples.md)** - Every platform covered

### 🏗️ **Architecture Guides**  
- 🎯 **[Deployment Architecture](docs/deployment-focused-architecture.md)** - How it all works
- 🔧 **[Platform-Specific Guides](docs/examples/platform-specific-guides.md)** - Detailed workflows
- 📚 **[CLI Reference](docs/cli-reference.md)** - Complete command reference

### ☁️ **Cloud Platform Guides**
- ☁️ [AWS Deployment](docs/aws-deployment.md)
- 🌐 [Google Cloud Deployment](docs/gcp-deployment.md)  
- 🔷 [Azure Deployment](docs/azure-deployment.md)

---

## 🎨 **Real-World Examples**

### Blog/CMS API
```bash
ndc create webapp-aws --name BlogApi --services database,cache,storage
# ✅ Content in PostgreSQL, performance caching, media storage
```

### E-commerce Backend  
```bash
ndc create webapp-gcp --name ShopApi --services database,cache,storage,mail,queue,jobs --include-worker
# ✅ Full e-commerce stack with order processing and email notifications
```

### Microservices Architecture
```bash
# User service
ndc create webapp-k8s --name UserService --services database,cache --port 8081

# Product service  
ndc create webapp-k8s --name ProductService --services database,storage --port 8082

# Deploy all to same K8s cluster with service mesh
```

### Rapid Prototyping
```bash
ndc create webapp-railway --name QuickProto --services database
# ✅ Instantly deploy to Railway with PostgreSQL add-on
```

---

## 📦 **Installation**

### CLI Tool
```bash
dotnet tool install --global NDC.Cli
```

### Template Packages
```bash
# Major clouds
dotnet new install NDC.Templates.WebApp.Aws      # AWS App Runner
dotnet new install NDC.Templates.WebApp.Gcp      # Google Cloud Run
dotnet new install NDC.Templates.WebApp.Azure    # Azure Container Apps

# Container platforms
dotnet new install NDC.Templates.WebApp.Docker   # Docker Compose  
dotnet new install NDC.Templates.WebApp.K8s      # Kubernetes

# PaaS platforms
dotnet new install NDC.Templates.WebApp.Railway  # Railway
dotnet new install NDC.Templates.WebApp.Render   # Render
dotnet new install NDC.Templates.WebApp.Fly      # Fly.io
```

---

## 🔧 **Requirements**

- .NET 9.0 SDK (or .NET 8.0 LTS)
- Docker Desktop (for local development)
- Cloud CLI tools for deployment (AWS CLI, gcloud, Azure CLI)
- Terraform >= 1.0 (for infrastructure deployment)

---

## 🤝 **Contributing**

We welcome contributions! The C# implementation offers:

- **Standard .NET patterns** familiar to all C# developers
- **Rich template system** with complex conditional logic
- **Native toolchain integration** with Visual Studio and VS Code
- **NuGet package distribution** using standard .NET packaging

### Development Setup
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli
dotnet restore NDC.Cli/NDC.Cli.csproj
dotnet build NDC.Cli/NDC.Cli.csproj
```

---

## 📞 **Support**

- 🐛 [Report Issues](https://github.com/plsft/noundry-cloud-cli/issues)
- 💬 [Discussions](https://github.com/plsft/noundry-cloud-cli/discussions)
- 📧 [Email Support](mailto:support@noundry.com)

---

## 🎉 **What Makes NDC Special**

1. **🎯 Best Local DX**: Aspire gives you the best local development experience possible
2. **🚀 Lightweight Production**: Only your API code gets deployed (no orchestration overhead)  
3. **⚙️ Configuration Magic**: Same code, different services via configuration
4. **🌍 Deploy Anywhere**: AWS, GCP, Azure, Docker, K8s, Railway, Render, Fly.io, Heroku
5. **📈 Scales with You**: Start simple, add services as you grow
6. **🔧 .NET Native**: Built by .NET developers, for .NET developers

---

**NDC** - The easiest way to build and deploy .NET applications to any platform.

*Built with ❤️ for the .NET community*
# NDC (Noundry Deploy CLI)

**Aspire for Local Development + Deploy API to Any Cloud Platform**

A .NET CLI tool that uses **Aspire for local development** and deploys **only your API** to any cloud platform with the same developer experience.

[![Release](https://img.shields.io/github/v/release/Noundry/Cloud)](https://github.com/Noundry/Cloud/releases)
[![License](https://img.shields.io/github/license/Noundry/Cloud)](LICENSE)

**Status**: ✅ **Production Ready** - All major cloud platforms supported

---

## 🎯 **What NDC Does**

### 🏠 **Local Development (Aspire Orchestration)**
- Automatically runs PostgreSQL, Redis, MinIO, MailHog in containers
- Service discovery and connection strings auto-configured
- Rich Aspire dashboard for monitoring services
- Hot reload and debugging support
- **Same code works locally and in production**

### ☁️ **Production Deployment (API Container Only)**
- Builds and deploys only your API code (lightweight container)
- Connects to managed cloud services (databases, cache, storage, queues)
- **Identical developer experience across all cloud providers**
- Complete Infrastructure as Code (Terraform)
- Works on AWS, Google Cloud, Azure, or any container platform

---

## ⚡ **Quick Start - Choose Your Platform**

### 🔄 **All Platforms - Same Experience**

**Pick any platform, get the same developer experience:**

```bash
# Use working examples (current approach)
git clone https://github.com/Noundry/Cloud.git
cd noundry-cloud-cli

# Choose your platform:
cp -r examples/working-aws-template MyApi      # AWS
cp -r examples/working-gcp-template MyApi      # Google Cloud  
cp -r examples/working-azure-template MyApi    # Azure
cp -r examples/working-container-template MyApi # Container

# CLI commands (requires template installation):
ndc create aws --name MyApi
ndc create gcp --name MyApi  
ndc create azure --name MyApi
ndc create container --name MyApi
```

### 1. **Quick Start with CLI Commands**
```bash
git clone https://github.com/Noundry/Cloud.git
cd noundry-cloud-cli

# Install templates (one-time setup)
dotnet pack src/NDC.Templates.WebApp/NDC.Templates.WebApp.csproj -o packages/
dotnet new install packages/NDC.Templates.WebApp.1.0.0.nupkg

# Create projects with any platform
ndc create aws --name MyApi      # AWS
ndc create gcp --name MyApi      # Google Cloud
ndc create azure --name MyApi    # Azure
ndc create container --name MyApi # Container
```

### 2. **Try with Working Examples (No installation required)**
```bash
git clone https://github.com/Noundry/Cloud.git
cd noundry-cloud-cli

# Choose your platform:
cp -r examples/working-aws-template MyApi      # AWS
cp -r examples/working-gcp-template MyApi      # Google Cloud  
cp -r examples/working-azure-template MyApi    # Azure
cp -r examples/working-container-template MyApi # Container

cd MyApi
```

### 2. Start Local Development
```bash
# This works! Aspire orchestrates all services automatically
dotnet run --project src/MyApp.AppHost

# ✅ API available at: http://localhost:8080
# ✅ Aspire dashboard at: https://localhost:17001
# ✅ Services running: PostgreSQL, Redis, MinIO, MailHog
```

### 3. Test the API
```bash
# Health check
curl http://localhost:8080/health

# Database endpoints (PostgreSQL via Aspire)
curl http://localhost:8080/users
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{"name":"John","email":"john@example.com"}'

# Cache endpoints (Redis via Aspire)  
curl -X POST http://localhost:8080/cache \
  -H "Content-Type: application/json" \
  -d '{"key":"test","value":"hello world"}'
curl http://localhost:8080/cache/test
```

### 4. **Deploy to Any Cloud Platform**

**The deployment experience is identical across all platforms:**

#### 🚀 **AWS Deployment**
```bash
# Deploy infrastructure (App Runner + RDS + ElastiCache + S3)
cd terraform && terraform init && terraform apply

# Build and deploy API container
cd .. && docker build -t myapi .
ECR_URL=$(cd terraform && terraform output -raw ecr_repository_url)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ECR_URL
docker tag myapi $ECR_URL:latest && docker push $ECR_URL:latest

# ✅ App Runner auto-deploys with managed AWS services
```

#### 🌐 **Google Cloud Deployment**
```bash
# Deploy infrastructure (Cloud Run + Cloud SQL + Memorystore + Cloud Storage)
cd terraform && terraform init && terraform apply

# Build and deploy API container
cd .. && docker build -t myapi .
REPO_URL=$(cd terraform && terraform output -raw artifact_registry_url)
gcloud auth configure-docker us-central1-docker.pkg.dev
docker tag myapi $REPO_URL/myapi:latest && docker push $REPO_URL/myapi:latest

# ✅ Cloud Run auto-deploys with managed Google services
```

#### 🔵 **Azure Deployment**
```bash
# Deploy infrastructure (Container Apps + SQL Database + Redis + Blob Storage)
cd terraform && terraform init && terraform apply

# Build and deploy API container
cd .. && docker build -t myapi .
ACR_URL=$(cd terraform && terraform output -raw container_registry_login_server)
az acr login --name ${ACR_URL%.*}
docker tag myapi $ACR_URL/myapi:latest && docker push $ACR_URL/myapi:latest

# ✅ Container Apps auto-deploys with managed Azure services
```

#### 🐳 **Container Platform (Docker/Kubernetes)**
```bash
# Docker Compose
docker build -t myapi:latest .
docker-compose up -d

# Kubernetes
kubectl apply -f k8s/

# ✅ Deploy anywhere containers run
```

---

## 🏗️ **Complete Multi-Cloud Implementation**

### ✅ **Fully Supported Platforms**

| Platform | Service | Database | Cache | Storage | Queue | Status |
|----------|---------|----------|--------|---------|--------|--------|
| **AWS** | App Runner | RDS PostgreSQL | ElastiCache | S3 | SQS | ✅ Production |
| **Google Cloud** | Cloud Run | Cloud SQL | Memorystore | Cloud Storage | Pub/Sub | ✅ Production |
| **Azure** | Container Apps | SQL Database | Redis Cache | Blob Storage | Service Bus | ✅ Production |
| **Container** | Docker/K8s | PostgreSQL | Redis | File/S3 | RabbitMQ/SQS | ✅ Production |

### 🎯 **Key Features**
- **Identical Experience**: Same commands, same workflow across all platforms
- **Complete Templates**: Web API and Razor Web App templates for each platform
- **Infrastructure as Code**: Full Terraform configurations included
- **Production Ready**: Real-world examples with security best practices
- **Local Development**: Aspire orchestration works the same everywhere

### Universal Architecture

**Every platform follows the same structure:**

```
MyApi/ (works on any platform)
├── src/
│   ├── MyApi.AppHost/         # 🏠 LOCAL ONLY - Aspire orchestration
│   ├── MyApi.Api/             # ☁️ DEPLOYED - API application
│   └── MyApi.ServiceDefaults/ # 📚 SHARED - Configuration
├── terraform/                 # ☁️ Cloud infrastructure (AWS/GCP/Azure)
├── k8s/                       # 🐳 Kubernetes manifests (container)
├── docker-compose.yml         # 🐳 Docker Compose (container)
├── Dockerfile                 # 🐳 Builds ONLY API project
└── README.md                  # 📖 Platform-specific instructions
```

**The same codebase deploys everywhere with platform-specific infrastructure.**

---

## 📚 **Complete Documentation**

### 🎯 **Platform-Specific Guides**
- [AWS Deployment](examples/working-aws-template/README.md) - App Runner + RDS + ElastiCache
- [Google Cloud Deployment](examples/working-gcp-template/README.md) - Cloud Run + Cloud SQL + Memorystore 
- [Azure Deployment](examples/working-azure-template/README.md) - Container Apps + SQL Database + Redis
- [Container Platform](examples/working-container-template/README.md) - Docker Compose + Kubernetes

### 🏗️ **Architecture & Design**
- [Getting Started](docs/getting-started.md) - Quick start guide
- [Deployment Architecture](docs/deployment-focused-architecture.md) - How Aspire + cloud works
- [Multi-Cloud Design](docs/multi-cloud-architecture.md) - Platform abstraction approach
- [CLI Architecture](docs/csharp-cli-architecture.md) - Technical implementation details

---

## 🔧 **Requirements**

### **Universal Requirements**
- .NET 9.0 SDK
- Docker Desktop (for Aspire local development)
- Terraform >= 1.0

### **Platform-Specific Tools**

**AWS:**
- AWS CLI and credentials
- Configured AWS profile

**Google Cloud:**
- Google Cloud CLI (`gcloud`)
- Authenticated with `gcloud auth login`

**Azure:**
- Azure CLI (`az`)
- Authenticated with `az login`

**Container Platforms:**
- Docker and Docker Compose
- kubectl (for Kubernetes)
- Access to container registry

---

## 🤝 **Contributing**

NDC provides production-ready templates for all major platforms:

✅ **Complete Implementation**: All major cloud platforms supported  
✅ **Unified Experience**: Same developer workflow everywhere  
✅ **Production Ready**: Real-world examples with best practices  
✅ **Infrastructure as Code**: Complete Terraform configurations

### Development Setup
```bash
git clone https://github.com/Noundry/Cloud.git
cd noundry-cloud-cli
dotnet build src/NDC.Cli/NDC.Cli.csproj
```

### Test All Platforms
```bash
# Test local development (same for all platforms)
cp -r examples/working-aws-template TestApp
cd TestApp
dotnet run --project src/MyApp.AppHost

# Test different deployment targets
cp -r examples/working-gcp-template TestGcp
cp -r examples/working-azure-template TestAzure
cp -r examples/working-container-template TestContainer
```

---

## 📞 **Support**

- 🐛 [Report Issues](https://github.com/Noundry/Cloud/issues)
- 💬 [Discussions](https://github.com/Noundry/Cloud/discussions)

## 📄 **License**

MIT License - see [LICENSE](LICENSE) for details.

---

## 🎯 **Multi-Cloud Vision Realized**

NDC delivers on the promise of **write once, deploy anywhere**:

### 🏠 **Consistent Local Development**
```bash
# Same command works for ANY target platform
dotnet run --project src/MyApi.AppHost
```

### ☁️ **Unified Cloud Deployment**
```bash
# Same workflow, different platforms (using working examples)
cp -r examples/working-aws-template MyApi     # → AWS App Runner + RDS + ElastiCache
cp -r examples/working-gcp-template MyApi     # → Cloud Run + Cloud SQL + Memorystore
cp -r examples/working-azure-template MyApi   # → Container Apps + SQL DB + Redis
cp -r examples/working-container-template MyApi # → Docker/K8s + PostgreSQL + Redis

# CLI commands (now working):
ndc create webapp-aws --name MyApi     # → AWS App Runner + RDS + ElastiCache
ndc create webapp-gcp --name MyApi     # → Cloud Run + Cloud SQL + Memorystore
ndc create webapp-azure --name MyApi   # → Container Apps + SQL DB + Redis
ndc create webapp-container --name MyApi # → Docker/K8s + PostgreSQL + Redis
```

### ⚙️ **Smart Configuration**
- **Local**: Aspire orchestrates development services in containers
- **Cloud**: Same code connects to managed cloud services via environment variables
- **No code changes**: Configuration-driven service discovery

### 🚀 **Production Ready**
- Complete infrastructure automation
- Security best practices
- Auto-scaling and monitoring
- Cost-optimized configurations

**Choose your platform, keep your workflow!**

*Built with ❤️ for the .NET community*
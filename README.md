# NDC (Noundry Deploy CLI)

**Aspire for Local Development + Deploy API to Any Platform**

A .NET CLI tool that uses **Aspire for local development** and deploys **only your API** to cloud platforms.

[![Release](https://img.shields.io/github/v/release/plsft/noundry-cloud-cli)](https://github.com/plsft/noundry-cloud-cli/releases)
[![License](https://img.shields.io/github/license/plsft/noundry-cloud-cli)](LICENSE)

**Status**: 🚧 **Active Development** - AWS template working, CLI framework complete

---

## 🎯 **What NDC Does**

### 🏠 **Local Development (Aspire Orchestration)**
- Automatically runs PostgreSQL, Redis, MinIO, MailHog in containers
- Service discovery and connection strings auto-configured
- Rich Aspire dashboard for monitoring services
- Hot reload and debugging support

### ☁️ **Production Deployment (API Container Only)**
- Builds and deploys only your API code (lightweight)
- Connects to managed cloud services (RDS, ElastiCache, S3, etc.)
- Configuration-driven service discovery
- Works on any container platform

---

## ⚡ **Try It Now (What's Working)**

### 1. Clone and Use Working Example
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli

# Use the working AWS template example
cp -r examples/working-aws-template MyBlogApi
cd MyBlogApi
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

### 4. Deploy to AWS (Complete Working Flow)
```bash
# Deploy infrastructure (RDS, ElastiCache, S3, App Runner)
cd terraform
terraform init && terraform apply

# Build API container (Dockerfile builds API only, not AppHost)
cd .. && docker build -t myblogapi .

# Push to ECR and deploy
ECR_URL=$(cd terraform && terraform output -raw ecr_repository_url)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ECR_URL
docker tag myblogapi $ECR_URL:latest && docker push $ECR_URL:latest

# ✅ App Runner automatically deploys API container
# ✅ Connects to managed AWS services via environment variables
# Get live URL: terraform output app_runner_service_url
```

---

## 🏗️ **Current Implementation**

### ✅ **What's Working**
- **C# CLI Framework**: Complete System.CommandLine + Spectre.Console
- **AWS Template**: Full Aspire local development + AWS App Runner deployment
- **Service Integration**: PostgreSQL, Redis, S3, email configured
- **Terraform Infrastructure**: Complete AWS setup (RDS, ElastiCache, S3, App Runner)
- **Working Example**: `examples/working-aws-template/` - ready to use

### 🚧 **In Development**  
- **Template Package Publishing**: NuGet packages for `dotnet new install`
- **CLI Commands**: Automated `ndc create` command
- **Multi-Cloud Templates**: GCP and Azure versions
- **Container Platform Templates**: Docker, Kubernetes, PaaS platforms

### Architecture

```
MyApp/ (from working example)
├── src/
│   ├── MyApp.AppHost/         # 🏠 LOCAL ONLY - Aspire orchestration
│   ├── MyApp.Api/             # ☁️ DEPLOYED - API application
│   └── MyApp.ServiceDefaults/ # 📚 SHARED - Configuration
├── terraform/                 # ☁️ AWS infrastructure
├── Dockerfile                 # 🐳 Builds ONLY API project
└── README.md                  # 📖 Instructions
```

---

## 📚 **Documentation**

### 🎯 **Current Guides**
- [Getting Started](docs/getting-started.md) - How to use the current implementation
- [Deployment Architecture](docs/deployment-focused-architecture.md) - How Aspire + cloud works
- [Working Example](examples/working-aws-template/README.md) - Step-by-step AWS deployment

### 🏗️ **Reference Documentation**
- [AWS Deployment Guide](docs/aws-deployment.md) - Complete AWS workflow
- [Architecture Design](docs/csharp-cli-architecture.md) - Technical decisions

---

## 🔧 **Requirements**

- .NET 9.0 SDK
- Docker Desktop (for Aspire local development)
- AWS CLI and credentials (for AWS deployment)
- Terraform >= 1.0

---

## 🤝 **Contributing**

The project demonstrates the complete vision with working AWS implementation. Current development focus:

1. **Template Package System**: Publishing to NuGet.org for easy installation
2. **CLI Completion**: Automated project creation commands
3. **Multi-Cloud Support**: GCP and Azure template implementations
4. **Platform Expansion**: Docker, Kubernetes, and PaaS templates

### Development Setup
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli
dotnet build NDC.Cli/NDC.Cli.csproj
```

### Test Current Implementation
```bash
# Try the working example
cp -r examples/working-aws-template TestApp
cd TestApp
dotnet run --project src/MyApp.AppHost
```

---

## 📞 **Support**

- 🐛 [Report Issues](https://github.com/plsft/noundry-cloud-cli/issues)
- 💬 [Discussions](https://github.com/plsft/noundry-cloud-cli/discussions)

## 📄 **License**

MIT License - see [LICENSE](LICENSE) for details.

---

## 🎯 **Vision Demonstrated**

The working AWS example demonstrates the complete NDC vision:

1. **🏠 Amazing Local DX**: `dotnet run --project src/MyApp.AppHost` starts everything
2. **☁️ Clean Production**: `docker build` creates lightweight API container  
3. **⚙️ Configuration Magic**: Same code works locally (Aspire) and cloud (managed services)
4. **🚀 Production Ready**: Complete AWS infrastructure with auto-scaling and security

**Try the working example to see the full potential!**

*Built with ❤️ for the .NET community*
# NDC (Noundry Deploy CLI) - C# Implementation

A powerful .NET CLI tool that generates production-ready .NET Aspire applications for AWS, Google Cloud, and Azure using Terraform and native service orchestration.

[![Release](https://img.shields.io/github/v/release/plsft/noundry-cloud-cli)](https://github.com/plsft/noundry-cloud-cli/releases)
[![NuGet](https://img.shields.io/nuget/v/NDC.Cli)](https://www.nuget.org/packages/NDC.Cli/)
[![License](https://img.shields.io/github/license/plsft/noundry-cloud-cli)](LICENSE)

NDC leverages .NET's native template engine and Aspire's service orchestration to create cloud-native applications with seamless local development and cloud deployment.

## 🚀 Quick Start

### Installation (30 seconds)

```bash
# Install NDC CLI tool
dotnet tool install --global NDC.Cli

# Install template packages (choose what you need)
dotnet new install NDC.Templates.WebApp.Aws      # AWS templates
dotnet new install NDC.Templates.WebApp.Gcp      # Google Cloud templates  
dotnet new install NDC.Templates.WebApp.Azure    # Azure templates
dotnet new install NDC.Templates.WebApp.Docker   # Docker templates
dotnet new install NDC.Templates.WebApp.K8s      # Kubernetes templates
```

### Create Your First Project (30 seconds)

```bash
# List available templates
ndc list

# Create a web application (replace 'aws' with gcp/azure/docker/k8s)
ndc create webapp-aws --name MyApp --services database,cache

# Or create with all services
ndc create webapp-aws --name MyProject --services all
```

### Start Local Development (30 seconds)

```bash
cd MyApp

# Start with Aspire (recommended) - orchestrates all services automatically
dotnet run --project src/MyApp.AppHost

# ✅ Your API: http://localhost:8080
# ✅ Aspire dashboard: https://localhost:17001  
# ✅ All services running: PostgreSQL, Redis, MinIO, MailHog, etc.
```

### Deploy to Production (2-3 minutes)

```bash
# Deploy infrastructure (creates managed cloud services)
cd terraform && terraform init && terraform apply -auto-approve

# Build and deploy API container only (no Aspire in production)
cd .. && docker build -t myapp .
# Push to your cloud registry (AWS ECR, GCP Artifact Registry, etc.)
# App Runner/Cloud Run/Container Apps automatically deploys
```

**🎉 You now have a production API with managed cloud services!**

## 🏗️ Architecture

NDC creates a complete application structure with:

```
MyApp/
├── src/
│   ├── MyApp.AppHost/           # 🎯 Aspire orchestration
│   ├── MyApp.Api/               # 🌐 Web API application  
│   ├── MyApp.ServiceDefaults/   # ⚙️  Shared configuration
│   └── MyApp.Worker/            # 🔄 Background services (optional)
├── terraform/                   # ☁️  Cloud infrastructure
├── docker-compose.yml          # 🐳 Local development
├── README.md                   # 📖 Deployment guide
└── MyApp.sln                   # 🔧 Visual Studio solution
```

## 📦 Available Templates

| Template | Platform | Description | Local DX | Deploy Target |
|----------|----------|-------------|----------|---------------|
| `webapp-aws` | AWS App Runner | Web app with AWS managed services | Aspire | API Container Only |
| `webapp-gcp` | Google Cloud Run | Web app with GCP managed services | Aspire | API Container Only |
| `webapp-azure` | Azure Container Apps | Web app with Azure managed services | Aspire | API Container Only |
| `webapp-docker` | Docker Compose | Self-hosted with external services | Aspire | API Container Only |
| `webapp-k8s` | Kubernetes | K8s deployment with external services | Aspire | API Container Only |
| `webapp-railway` | Railway | Optimized for Railway deployment | Aspire | API Container Only |
| `webapp-render` | Render | Optimized for Render deployment | Aspire | API Container Only |
| `webapp-fly` | Fly.io | Optimized for Fly.io deployment | Aspire | API Container Only |

## 🛠️ Template Options

### Core Options
- `--name, -n`: Project name (required)
- `--framework, -f`: .NET framework (net9.0, net8.0) - default: net9.0  
- `--port, -p`: Application port - default: 8080

### Database Options
- `--database`: Database provider (PostgreSQL, MySQL, SqlServer, None) - default: PostgreSQL

### Service Options
- `--include-cache`: Redis cache - default: true
- `--include-storage`: S3-compatible storage - default: false
- `--include-mail`: Email service - default: false
- `--include-queue`: Message queue - default: false  
- `--include-jobs`: Background jobs - default: false
- `--include-worker`: Worker service project - default: false

### Convenience Options
- `--services`: Comma-separated services (cache,storage,mail,queue,jobs,worker,all)

## 🌥️ Service Mapping

NDC automatically maps services between local development and cloud deployment:

### Local Development (Aspire + Docker)
- **Database**: PostgreSQL/MySQL/SQL Server containers
- **Cache**: Redis container  
- **Storage**: MinIO (S3-compatible)
- **Mail**: MailHog SMTP server
- **Queue**: RabbitMQ container
- **Jobs**: Hangfire with in-memory storage

### AWS Cloud Deployment  
- **Database**: RDS Aurora PostgreSQL/MySQL or RDS SQL Server
- **Cache**: ElastiCache for Redis
- **Storage**: Amazon S3
- **Mail**: Amazon SES  
- **Queue**: Amazon SQS + EventBridge
- **Jobs**: Hangfire with database storage
- **Compute**: AWS App Runner

## ✨ Key Features

### 🎯 **Aspire-First Design**
- Native .NET Aspire integration
- Service discovery and orchestration
- Built-in observability and health checks
- Seamless local-to-cloud transitions

### 🏗️ **Production-Ready**
- Optimized multi-stage Dockerfiles
- Security best practices (non-root containers)
- Infrastructure as Code with Terraform
- Auto-scaling and load balancing

### 👨‍💻 **Developer Experience**
- Full local development environment
- Hot reload and debugging support
- Comprehensive documentation
- IDE integration with IntelliSense

### ☁️ **Multi-Cloud Support**
- AWS, Google Cloud, and Azure templates
- Cloud-specific optimizations
- Consistent developer experience across clouds

## 🚀 Example Usage

### ⚡ **Cloud Platforms**

#### AWS App Runner + Managed Services
```bash
# Simple API with database
ndc create webapp-aws --name BlogApi --services database

# Full e-commerce backend  
ndc create webapp-aws --name EcommerceApi \
  --database PostgreSQL \
  --services cache,storage,mail,queue,jobs,worker \
  --min-instances 2 \
  --max-instances 50

cd EcommerceApi && dotnet run --project src/EcommerceApi.AppHost
```

#### Google Cloud Run + Managed Services  
```bash
# High-performance API
ndc create webapp-gcp --name AnalyticsApi \
  --database MySQL \
  --services cache,storage \
  --cpu 2000m \
  --memory 4Gi \
  --max-instances 100

cd AnalyticsApi && dotnet run --project src/AnalyticsApi.AppHost
```

#### Azure Container Apps + Managed Services
```bash
# Enterprise application
ndc create webapp-azure --name EnterpriseApi \
  --database SqlServer \
  --services cache,storage,mail,queue \
  --framework net8.0 \
  --cpu 2.0 \
  --memory 4.0Gi

cd EnterpriseApi && dotnet run --project src/EnterpriseApi.AppHost
```

### 🐳 **Container Platforms**

#### Docker Compose (Self-Hosted)
```bash
# Perfect for VPS deployment
ndc create webapp-docker --name SelfHostedApi \
  --services database,cache,storage

cd SelfHostedApi
dotnet run --project src/SelfHostedApi.AppHost                    # Local with Aspire
docker compose -f docker-compose.prod.yml up                     # Production deployment
```

#### Kubernetes (Any Cluster)
```bash
# Deploy to any K8s cluster
ndc create webapp-k8s --name MicroserviceApi \
  --services database,cache,queue

cd MicroserviceApi
dotnet run --project src/MicroserviceApi.AppHost                  # Local development
kubectl apply -f k8s/                                            # Deploy to K8s
```

### 🌟 **PaaS Platforms**

#### Modern PaaS
```bash
# Railway
ndc create webapp-railway --name RailwayApi --services database,cache

# Render
ndc create webapp-render --name RenderApi --services database,storage  

# Fly.io
ndc create webapp-fly --name GlobalApi --services database,cache,storage
```

## 🚀 Deployment

Each generated project includes detailed deployment instructions:

1. **Configure cloud credentials** (AWS CLI, etc.)
2. **Deploy infrastructure:**
   ```bash
   cd terraform
   terraform init && terraform apply
   ```
3. **Build and deploy application:**
   ```bash
   # Follow project-specific README.md
   ```

## 🔧 Advanced Usage

### Using with Visual Studio
```bash
ndc create aspire-webapp-aws --name MyApp
cd MyApp
start MyApp.sln  # Opens in Visual Studio
```

### Using dotnet new directly
```bash
# After installing templates
dotnet new aspire-webapp-aws --name MyApp --database PostgreSQL --include-cache true
```

## 📋 Requirements

- .NET 9.0 SDK or later
- Docker Desktop
- Cloud CLI tools (AWS CLI, gcloud, Azure CLI)
- Terraform >= 1.0

## 📚 Documentation & Examples

### 📖 **Core Documentation**
- 🎯 [Deployment Architecture](docs/deployment-focused-architecture.md) - How Aspire + cloud deployment works
- 🏗️ [Original Aspire Integration](docs/aspire-architecture.md) - Technical deep dive
- ☁️ [AWS Deployment Guide](docs/aws-deployment.md)
- 🌐 [Google Cloud Guide](docs/gcp-deployment.md)  
- 🔷 [Azure Deployment Guide](docs/azure-deployment.md)

### 🚀 **Practical Examples**
- ⚡ [5-Minute Quick Start](docs/examples/5-minute-quickstart.md) - Get running immediately
- 📋 [Quick Start Cheat Sheet](docs/examples/quick-start-cheatsheet.md) - Common commands reference
- 🌍 [All Platforms Examples](docs/examples/all-platforms-examples.md) - Comprehensive platform coverage
- 🛠️ [Platform-Specific Guides](docs/examples/platform-specific-guides.md) - Detailed deployment workflows

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md).

### Development Setup
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli/ndc-csharp
dotnet restore
dotnet build
```

### Testing Templates Locally
```bash
# Install CLI locally
dotnet pack NDC.Cli/NDC.Cli.csproj -o packages
dotnet tool install --global --add-source packages NDC.Cli

# Install templates locally  
dotnet new install NDC.Templates.Aspire.Aws/

# Test template creation
ndc create aspire-webapp-aws --name TestApp
```

## 🆚 Why C# Implementation?

The C# implementation offers several advantages over the original Go version:

- **Native .NET Integration**: Uses standard `dotnet new` template system
- **Aspire-First**: Built specifically for .NET Aspire workflows  
- **Rich Templating**: Complex conditional logic and parameter validation
- **IDE Support**: Full IntelliSense and debugging capabilities
- **Community Familiar**: .NET developers know the patterns
- **Package Management**: Standard NuGet distribution

## 📞 Support

- 🐛 [Report Issues](https://github.com/plsft/noundry-cloud-cli/issues)
- 💬 [Discussions](https://github.com/plsft/noundry-cloud-cli/discussions)
- 📧 [Email Support](mailto:support@noundry.com)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [System.CommandLine](https://github.com/dotnet/command-line-api)
- Powered by [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- UI enhanced with [Spectre.Console](https://github.com/spectreconsole/spectre.console)

---

**NDC** - Bringing .NET Aspire to the cloud with production-ready templates and seamless service orchestration.

*Built with ❤️ for the .NET community*
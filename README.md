# NDC (Noundry Deploy CLI) - C# Implementation

A powerful .NET CLI tool that generates production-ready .NET Aspire applications for AWS, Google Cloud, and Azure using Terraform and native service orchestration.

[![Release](https://img.shields.io/github/v/release/plsft/noundry-cloud-cli)](https://github.com/plsft/noundry-cloud-cli/releases)
[![NuGet](https://img.shields.io/nuget/v/NDC.Cli)](https://www.nuget.org/packages/NDC.Cli/)
[![License](https://img.shields.io/github/license/plsft/noundry-cloud-cli)](LICENSE)

NDC leverages .NET's native template engine and Aspire's service orchestration to create cloud-native applications with seamless local development and cloud deployment.

## 🚀 Quick Start

### Installation

```bash
# Install the CLI tool
dotnet tool install --global NDC.Cli

# Install template packages
dotnet new install NDC.Templates.Aspire.Aws
# dotnet new install NDC.Templates.Aspire.Gcp    # Coming soon
# dotnet new install NDC.Templates.Aspire.Azure  # Coming soon
```

### Create Your First Project

```bash
# List available templates
ndc list

# Create an Aspire web application
ndc create aspire-webapp-aws --name MyApp

# Create with specific services
ndc create aspire-webapp-aws --name MyProject \
  --database PostgreSQL \
  --include-cache true \
  --include-storage true \
  --include-mail true \
  --include-queue true \
  --include-jobs true \
  --include-worker true
```

### Local Development

```bash
cd MyApp

# Start all services with Aspire (recommended)
dotnet run --project src/MyApp.AppHost

# Or use Docker Compose
docker compose up
```

Visit the Aspire dashboard at https://localhost:17001 to monitor your services.

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

| Template | Description | Status |
|----------|-------------|--------|
| `aspire-webapp-aws` | Aspire web app for AWS App Runner | ✅ Available |
| `aspire-fullstack-aws` | Complete app with all services | 🚧 Coming Soon |
| `aspire-webapp-gcp` | Aspire web app for Google Cloud Run | 🚧 Coming Soon |
| `aspire-webapp-azure` | Aspire web app for Azure Container Apps | 🚧 Coming Soon |

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

### Basic Web Application
```bash
ndc create aspire-webapp-aws --name BlogApi --database PostgreSQL
cd BlogApi
dotnet run --project src/BlogApi.AppHost
```

### Full-Stack Application
```bash
ndc create aspire-webapp-aws --name ECommerceApp \
  --database PostgreSQL \
  --services cache,storage,mail,queue,jobs,worker

cd ECommerceApp
dotnet run --project src/ECommerceApp.AppHost
```

### Custom Configuration
```bash
ndc create aspire-webapp-aws --name PaymentService \
  --framework net8.0 \
  --database SqlServer \
  --include-cache true \
  --include-queue true \
  --port 5000
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

## 📚 Documentation

- 🎯 [Getting Started](docs/csharp-cli-architecture.md)
- 🏗️ [Aspire Integration](docs/aspire-architecture.md)  
- ☁️ [AWS Deployment](docs/aws-deployment.md)
- 📦 [Template Development](docs/template-development.md)

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
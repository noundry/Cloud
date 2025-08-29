# NDC (Noundry Deploy CLI)

A powerful CLI tool for generating production-ready .NET deployment templates across AWS, Google Cloud, and Azure using Terraform.

[![Release](https://img.shields.io/github/v/release/plsft/noundry-cloud-cli)](https://github.com/plsft/noundry-cloud-cli/releases)
[![Go Version](https://img.shields.io/github/go-mod/go-version/plsft/noundry-cloud-cli)](https://golang.org/)
[![License](https://img.shields.io/github/license/plsft/noundry-cloud-cli)](LICENSE)

NDC streamlines cloud-native .NET development by generating complete, production-ready projects with Infrastructure as Code, containerization, and cloud-specific optimizations.

## Quick Start

### Installation

#### One-Line Install (Linux/macOS)
```bash
curl -fsSL https://raw.githubusercontent.com/plsft/noundry-cloud-cli/main/install.sh | sh
```

#### Manual Installation
1. Download the latest release for your platform from [Releases](https://github.com/plsft/noundry-cloud-cli/releases)
2. Extract the archive
3. Move the `ndc` binary to a directory in your PATH

#### Windows (PowerShell)
```powershell
# Download and extract manually from releases page
# Add ndc.exe to your PATH
```

#### Build from Source
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli
make build
make install
```

### Usage

```bash
# List available templates
ndc list

# Create a new AWS project
ndc create dotnet-webapp-aws --name my-api

# Create a GCP project with custom settings
ndc create dotnet-webapp-gcp --name my-service --port 5000 --max-instances 10

# Create an Azure project
ndc create dotnet-webapp-azure --name my-app --framework net8.0
```

## Available Templates

| Template | Cloud Provider | Service | Description |
|----------|---------------|---------|-------------|
| `dotnet-webapp-aws` | AWS | App Runner | Serverless container deployment with ECR |
| `dotnet-webapp-gcp` | Google Cloud | Cloud Run | Serverless container deployment with Artifact Registry |
| `dotnet-webapp-azure` | Azure | Container Apps | Serverless container deployment with ACR |

## Command Options

### Global Options
- `--name, -n`: Project name (required)
- `--output, -o`: Output directory (default: current directory)
- `--framework, -f`: .NET framework version (default: net9.0)
- `--port, -p`: Application port (default: 8080)
- `--min-instances`: Minimum instances (default: 1)
- `--max-instances`: Maximum instances (default: 5)
- `--cpu`: CPU allocation (cloud-specific defaults)
- `--memory`: Memory allocation (cloud-specific defaults)

### Examples

```bash
# Basic usage
ndc create dotnet-webapp-aws --name hello-world

# Custom configuration
ndc create dotnet-webapp-gcp \
  --name production-api \
  --framework net8.0 \
  --port 5000 \
  --min-instances 2 \
  --max-instances 20 \
  --cpu 2000m \
  --memory 4Gi

# Output to specific directory
ndc create dotnet-webapp-azure --name test-app --output ./projects
```

## Project Structure

Each generated project includes:

```
my-project/
├── terraform/           # Infrastructure as Code
│   ├── main.tf         # Main resources
│   ├── variables.tf    # Input variables
│   ├── provider.tf     # Cloud provider configuration
│   └── versions.tf     # Terraform version constraints
├── src/
│   └── MyProject/      # .NET application
│       ├── Program.cs
│       ├── MyProject.csproj
│       └── ...
├── Dockerfile          # Container configuration
├── MyProject.sln       # Visual Studio solution
└── README.md          # Deployment instructions
```

## Cloud-Specific Features

### AWS (App Runner)
- **Auto-scaling**: Based on HTTP requests
- **ECR Integration**: Private container registry
- **IAM Roles**: Least-privilege security
- **Health Checks**: HTTP endpoint monitoring

### Google Cloud (Cloud Run)
- **Traffic Splitting**: Blue/green deployments
- **Artifact Registry**: Secure container storage
- **Service Accounts**: Fine-grained permissions
- **VPC Connector**: Private network access

### Azure (Container Apps)
- **KEDA Scaling**: Event-driven autoscaling
- **Managed Identity**: Passwordless authentication
- **Log Analytics**: Comprehensive monitoring
- **Ingress Control**: HTTP/HTTPS traffic management

## Development Workflow

1. **Generate Project**: `noundry create <template> --name <project>`
2. **Develop Locally**: `dotnet run --project src/<project>`
3. **Build Container**: `docker build -t <project> .`
4. **Deploy Infrastructure**: `cd terraform && terraform apply`
5. **Push Container**: Follow cloud-specific instructions in README

## Prerequisites

- **Docker**: Container building and pushing
- **Terraform**: Infrastructure deployment (>= 1.0)
- **.NET SDK**: Local development (>= 8.0)
- **Cloud CLI**: Authentication and resource management
  - AWS CLI (for AWS)
  - gcloud CLI (for GCP)
  - Azure CLI (for Azure)

## Documentation

- 📖 [Getting Started](docs/getting-started.md)
- ☁️ [AWS Deployment Guide](docs/aws-deployment.md)
- 🌐 [Google Cloud Deployment Guide](docs/gcp-deployment.md)
- 🔷 [Azure Deployment Guide](docs/azure-deployment.md)
- ⚙️ [Customization Guide](docs/customization.md)

## Examples

### Real-World Deployment Example

```bash
# Create a production API
ndc create dotnet-webapp-aws --name payments-api \
  --framework net8.0 \
  --port 8080 \
  --min-instances 2 \
  --max-instances 50 \
  --cpu 2048 \
  --memory 4096

# Navigate and explore
cd payments-api
tree .

# Deploy to AWS
cd terraform
terraform init
terraform apply

# Build and push container
docker build -t payments-api .
# ... follow AWS deployment guide
```

## Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Development Setup
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli
go mod download
make dev-test  # Test CLI generation
```

## Support

- 🐛 [Report Issues](https://github.com/plsft/noundry-cloud-cli/issues)
- 💬 [Discussions](https://github.com/plsft/noundry-cloud-cli/discussions)
- 📧 [Contact](mailto:support@noundry.com)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [Cobra](https://github.com/spf13/cobra) CLI framework
- Inspired by modern DevOps and Infrastructure as Code practices
- Designed for production .NET workloads

---

**NDC** - Simplifying cloud-native .NET deployments across AWS, Google Cloud, and Azure.

*Built with ❤️ for the .NET community*
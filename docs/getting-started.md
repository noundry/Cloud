# Getting Started with NDC

NDC (Noundry Deploy CLI) makes it easy to generate production-ready .NET applications for AWS, Google Cloud, and Azure.

## Installation

### Quick Install (Linux/macOS)
```bash
curl -fsSL https://raw.githubusercontent.com/plsft/noundry-cloud-cli/main/install.sh | sh
```

### Manual Installation
1. Download the latest release for your platform from [Releases](https://github.com/plsft/noundry-cloud-cli/releases)
2. Extract the archive
3. Move the binary to a directory in your PATH

### Build from Source
```bash
git clone https://github.com/plsft/noundry-cloud-cli.git
cd noundry-cloud-cli
make build
make install
```

## First Project

Let's create your first cloud-native .NET application:

### 1. Choose Your Cloud Platform

```bash
# List available templates
ndc list
```

### 2. Create Your Project

```bash
# AWS Project
ndc create dotnet-webapp-aws --name hello-world

# Google Cloud Project
ndc create dotnet-webapp-gcp --name hello-world

# Azure Project
ndc create dotnet-webapp-azure --name hello-world
```

### 3. Explore Generated Structure

```
hello-world/
├── terraform/           # Infrastructure as Code
│   ├── main.tf         # Cloud resources
│   ├── variables.tf    # Configuration variables
│   ├── provider.tf     # Cloud provider setup
│   └── versions.tf     # Terraform version requirements
├── src/
│   └── HelloWorld/     # .NET application
│       ├── Program.cs  # Application entry point
│       ├── HelloWorld.csproj
│       └── ...
├── Dockerfile          # Container configuration
├── HelloWorld.sln      # Visual Studio solution
└── README.md          # Deployment instructions
```

### 4. Test Locally

```bash
cd hello-world
dotnet run --project src/HelloWorld
```

Your app will be available at `http://localhost:8080` with:
- `GET /` - Hello World message
- `GET /health` - Health check endpoint

### 5. Deploy to Cloud

Follow the cloud-specific instructions in the generated `README.md`:

- **AWS**: Use AWS CLI and ECR for container deployment
- **Google Cloud**: Use gcloud CLI and Artifact Registry
- **Azure**: Use Azure CLI and Container Registry

## Next Steps

- [AWS Deployment Guide](aws-deployment.md)
- [Google Cloud Deployment Guide](gcp-deployment.md)
- [Azure Deployment Guide](azure-deployment.md)
- [Customization Options](customization.md)
- [Best Practices](best-practices.md)
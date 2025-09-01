# NDC C# CLI Architecture

## Overview

NDC will be reimplemented as a .NET CLI tool using:
- **.NET Template Engine**: Native template system (`dotnet new`)
- **System.CommandLine**: Modern command-line parsing
- **NuGet Package Distribution**: Easy installation via `dotnet tool install`
- **Native Aspire Integration**: Seamless service discovery and orchestration

## Architecture Components

```
src/                               # Source code
├── NDC.Cli/                      # Main CLI tool
│   ├── Program.cs                # Entry point
│   ├── Commands/                 # Command implementations
│   │   ├── CreateCommand.cs     # Template creation
│   │   ├── ListCommand.cs       # List available templates
│   │   └── InstallCommand.cs    # Install template packages
│   ├── Services/                 # Core services
│   │   ├── TemplateService.cs   # Template management
│   │   ├── AspireService.cs     # Aspire integration
│   │   └── CloudService.cs      # Cloud provider services
│   └── NDC.Cli.csproj           # Tool project file
└── NDC.Templates.WebApp/         # Multi-cloud template package
    └── content/                  # Template content
        ├── webapp-aws/           # AWS App Runner templates
        ├── webapp-gcp/           # Google Cloud Run templates  
        ├── webapp-azure/         # Azure Container Apps templates
        └── webapp-container/     # Docker/Kubernetes templates

tests/                            # Test projects
└── NDC.Cli.Tests/               # Unit tests for CLI
    ├── Commands/                # Command tests
    ├── Services/                # Service tests
    └── Models/                  # Model tests

examples/                         # Working examples
├── working-aws-template/        # Complete AWS example
├── working-gcp-template/        # Complete GCP example
├── working-azure-template/      # Complete Azure example
└── working-container-template/  # Complete container example
```

## .NET Template Structure

Each template package contains:
```
content/                           # Template files
├── .template.config/
│   └── template.json             # Template metadata
├── Company.WebApplication1/       # Template content
│   ├── src/
│   ├── terraform/
│   └── README.md
└── nuget.config                   # NuGet configuration
```

## Installation & Usage

### Installation
```bash
# Install the CLI tool
dotnet tool install --global NDC.Cli

# Use working examples (current approach)
git clone https://github.com/Noundry/Cloud.git
cd noundry-cloud-cli

# Future: Install template packages
# ndc install NDC.Templates.WebApp
```

### Usage
```bash
# List available templates
ndc list

# Use working examples (current approach)
cp -r examples/working-aws-template MyApp

# Future CLI commands:
# ndc create webapp-aws --name MyApp --services database,cache,storage
```

## Template Parameters

Templates support rich parameter configuration:
```json
{
  "author": "Noundry",
  "classifications": ["Web", "Cloud", "Aspire"],
  "identity": "NDC.Templates.WebApp",
  "name": "Multi-Cloud Web App",
  "shortName": "webapp-aws",
  "tags": {
    "language": "C#",
    "type": "project"
  },
  "symbols": {
    "Database": {
      "type": "parameter",
      "datatype": "choice",
      "choices": [
        { "choice": "PostgreSQL", "description": "PostgreSQL database" },
        { "choice": "MySQL", "description": "MySQL database" },
        { "choice": "SqlServer", "description": "SQL Server database" }
      ],
      "defaultValue": "PostgreSQL"
    },
    "IncludeCache": {
      "type": "parameter",
      "datatype": "bool",
      "defaultValue": "true"
    },
    "Services": {
      "type": "parameter",
      "datatype": "string",
      "description": "Comma-separated services to include"
    }
  }
}
```

## Service Discovery Integration

Templates will automatically configure:

### Local Development (Aspire AppHost)
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Services configured based on template parameters
var postgres = builder.AddPostgreSQL("postgres");
var redis = builder.AddRedis("redis");
var minio = builder.AddContainer("minio", "minio/minio");

var api = builder.AddProject<Projects.MyApp_Api>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(minio);

builder.Build().Run();
```

### Cloud Deployment (Terraform)
```hcl
# Auto-generated based on local services
resource "aws_rds_cluster" "postgres" {
  engine = "aurora-postgresql"
  # ... configuration
}

resource "aws_elasticache_cluster" "redis" {
  engine = "redis"
  # ... configuration
}

resource "aws_s3_bucket" "storage" {
  # ... configuration
}
```

## Benefits of C# Implementation

1. **Native .NET Integration**: Uses standard `dotnet new` system
2. **Rich Parameter Support**: Complex template parameters and conditions
3. **Aspire-First**: Built specifically for .NET Aspire workflows
4. **NuGet Distribution**: Standard .NET package management
5. **IDE Support**: Full IntelliSense and debugging
6. **Community Familiar**: .NET developers already know the patterns

## Template Categories

### Multi-Cloud Web App Templates
- `webapp-aws` - Aspire web app for AWS (App Runner + RDS + ElastiCache)
- `webapp-gcp` - Aspire web app for Google Cloud (Cloud Run + Cloud SQL + Memorystore)
- `webapp-azure` - Aspire web app for Azure (Container Apps + SQL Database + Redis)
- `webapp-container` - Aspire web app for containers (Docker Compose + Kubernetes)

## Implementation Plan

1. **CLI Tool**: Create System.CommandLine based tool
2. **Template Packages**: Convert templates to .NET template format
3. **Service Discovery**: Implement Aspire service mapping
4. **Cloud Integration**: Generate Terraform based on services
5. **Distribution**: Publish to NuGet.org
6. **Documentation**: Update guides for C# CLI
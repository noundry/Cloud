# NDC C# CLI Architecture

## Overview

NDC will be reimplemented as a .NET CLI tool using:
- **.NET Template Engine**: Native template system (`dotnet new`)
- **System.CommandLine**: Modern command-line parsing
- **NuGet Package Distribution**: Easy installation via `dotnet tool install`
- **Native Aspire Integration**: Seamless service discovery and orchestration

## Architecture Components

```
NDC.Cli/                           # Main CLI tool
├── Program.cs                     # Entry point
├── Commands/                      # Command implementations
│   ├── CreateCommand.cs          # Template creation
│   ├── ListCommand.cs            # List available templates
│   └── InstallCommand.cs         # Install template packages
├── Services/                      # Core services
│   ├── TemplateService.cs        # Template management
│   ├── AspireService.cs          # Aspire integration
│   └── CloudService.cs           # Cloud provider services
└── NDC.Cli.csproj                # Tool project file

NDC.Templates/                     # Template packages
├── NDC.Templates.Aspire.Aws/     # AWS Aspire templates
├── NDC.Templates.Aspire.Gcp/     # GCP Aspire templates
├── NDC.Templates.Aspire.Azure/   # Azure Aspire templates
└── NDC.Templates.Simple/         # Simple webapp templates
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

# Install template packages
ndc install NDC.Templates.Aspire.Aws
ndc install NDC.Templates.Aspire.Gcp
ndc install NDC.Templates.Aspire.Azure
```

### Usage
```bash
# List available templates
ndc list

# Create projects using .NET template engine
ndc create aspire-webapp-aws --name MyApp --services database,cache,storage

# Or use dotnet new directly (once templates are installed)
dotnet new aspire-webapp-aws --name MyApp --database PostgreSQL --include-cache true
```

## Template Parameters

Templates support rich parameter configuration:
```json
{
  "author": "Noundry",
  "classifications": ["Web", "Cloud", "Aspire"],
  "identity": "NDC.Templates.Aspire.Aws",
  "name": "Aspire Web App for AWS",
  "shortName": "aspire-webapp-aws",
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

### Simple Templates
- `dotnet-webapp-aws` - Basic .NET web app
- `dotnet-webapp-gcp` - Basic .NET web app
- `dotnet-webapp-azure` - Basic .NET web app

### Aspire Templates
- `aspire-webapp-aws` - Aspire web app with service discovery
- `aspire-webapp-gcp` - Aspire web app with service discovery
- `aspire-webapp-azure` - Aspire web app with service discovery

### Full-Stack Templates
- `aspire-fullstack-aws` - Complete application with all services
- `aspire-fullstack-gcp` - Complete application with all services
- `aspire-fullstack-azure` - Complete application with all services

## Implementation Plan

1. **CLI Tool**: Create System.CommandLine based tool
2. **Template Packages**: Convert templates to .NET template format
3. **Service Discovery**: Implement Aspire service mapping
4. **Cloud Integration**: Generate Terraform based on services
5. **Distribution**: Publish to NuGet.org
6. **Documentation**: Update guides for C# CLI
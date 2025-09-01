# NDC Multi-Cloud Web App Templates

This package contains .NET project templates for creating multi-cloud web applications with Aspire local development support.

## Templates Included

- **webapp-aws** - Web app for AWS (App Runner + RDS + ElastiCache)
- **webapp-gcp** - Web app for Google Cloud (Cloud Run + Cloud SQL + Memorystore) 
- **webapp-azure** - Web app for Azure (Container Apps + SQL Database + Redis)
- **webapp-container** - Web app for containers (Docker Compose + Kubernetes)

## Installation

```bash
dotnet new install NDC.Templates.WebApp
```

## Usage

```bash
# Create AWS web app
dotnet new webapp-aws --name MyApp

# Create with specific services
dotnet new webapp-gcp --name MyApp --database true --cache true --storage true

# Create for containers with all services
dotnet new webapp-container --name MyApp --database --cache --storage --mail --queue
```

## Features

- **Aspire Local Development**: All templates include Aspire AppHost for local orchestration
- **Production Ready**: Complete infrastructure as code (Terraform, Docker, Kubernetes)  
- **Multi-Cloud**: Same developer experience across all platforms
- **Service Integration**: Database, cache, storage, messaging, and worker services
- **Container Native**: Optimized for cloud container platforms

## Template Parameters

All templates support these parameters:

- `--name` - Project name (replaces Company.WebApplication1)
- `--framework` - Target framework (net9.0, net8.0)
- `--database` - Include database support (default: true)
- `--cache` - Include cache support (default: true) 
- `--storage` - Include storage support (default: true)
- `--mail` - Include email support (default: false)
- `--queue` - Include message queue support (default: false)
- `--jobs` - Include background jobs (default: false)
- `--worker` - Include worker service (default: false)

## Architecture

Each template creates:

```
MyApp/
├── src/
│   ├── MyApp.AppHost/         # Aspire orchestration (local only)
│   ├── MyApp.Api/             # Web API (deployed to production)
│   └── MyApp.ServiceDefaults/ # Shared configuration
├── terraform/                 # Infrastructure as code
├── Dockerfile                 # Production container
└── README.md                  # Platform-specific deployment guide
```

## More Information

- [GitHub Repository](https://github.com/Noundry/Cloud)
- [Documentation](https://github.com/Noundry/Cloud/tree/main/docs)
- [Examples](https://github.com/Noundry/Cloud/tree/main/examples)
# NDC + .NET Aspire Integration Architecture

## Overview

NDC integrates with .NET Aspire to provide a complete cloud-native development experience with:
- **Local Development**: Full service orchestration with Docker
- **Cloud Deployment**: Automatic service discovery and configuration
- **Configuration-Driven**: Services discovered from appsettings.json

## Architecture Components

```mermaid
graph TB
    subgraph "Local Development"
        AppHost[".NET Aspire AppHost"]
        API[".NET API"]
        DB[(PostgreSQL/MySQL/SQL Server)]
        Redis[(Redis Cache)]
        S3[MinIO S3]
        Mail[MailHog SMTP]
        Queue[RabbitMQ]
        Jobs[Hangfire/Quartz]
    end
    
    subgraph "Cloud Deployment"
        CloudAPI[".NET API in Cloud]
        CloudDB[(Managed Database)]
        CloudCache[(Managed Cache)]
        CloudStorage[(Cloud Storage)]
        CloudMail[(Cloud SMTP)]
        CloudQueue[(Cloud Queue)]
        CloudJobs[(Cloud Jobs)]
    end
    
    AppHost --> API
    AppHost --> DB
    AppHost --> Redis
    AppHost --> S3
    AppHost --> Mail
    AppHost --> Queue
    AppHost --> Jobs
    
    API --> CloudAPI
    DB --> CloudDB
    Redis --> CloudCache
    S3 --> CloudStorage
    Mail --> CloudMail
    Queue --> CloudQueue
    Jobs --> CloudJobs
```

## Template Structure

### Enhanced Project Structure
```
my-aspire-project/
├── src/
│   ├── MyProject.AppHost/          # Aspire orchestration
│   ├── MyProject.ServiceDefaults/  # Shared service configuration  
│   ├── MyProject.Api/              # Main API application
│   └── MyProject.Worker/           # Background services (optional)
├── terraform/                      # Cloud infrastructure
├── docker-compose.yml             # Local development
├── aspire-manifest.json           # Generated Aspire manifest
├── Dockerfile.api                 # API container
├── Dockerfile.worker              # Worker container (optional)
└── README.md
```

## Service Discovery Mapping

### Configuration Format
```json
{
  "Services": {
    "Database": {
      "Type": "PostgreSQL",
      "Local": {
        "ConnectionString": "Host=localhost;Port=5432;Database=myapp;Username=dev;Password=dev"
      },
      "Cloud": {
        "AWS": "RDS",
        "GCP": "Cloud SQL", 
        "Azure": "PostgreSQL Flexible Server"
      }
    },
    "Cache": {
      "Type": "Redis",
      "Local": {
        "ConnectionString": "localhost:6379"
      },
      "Cloud": {
        "AWS": "ElastiCache",
        "GCP": "Memorystore",
        "Azure": "Cache for Redis"
      }
    },
    "Storage": {
      "Type": "S3",
      "Local": {
        "Endpoint": "http://localhost:9000",
        "AccessKey": "minioadmin",
        "SecretKey": "minioadmin"
      },
      "Cloud": {
        "AWS": "S3",
        "GCP": "Cloud Storage",
        "Azure": "Blob Storage"
      }
    },
    "Mail": {
      "Type": "SMTP",
      "Local": {
        "Host": "localhost",
        "Port": 1025,
        "Username": "",
        "Password": ""
      },
      "Cloud": {
        "AWS": "SES",
        "GCP": "SendGrid",
        "Azure": "Communication Services"
      }
    },
    "MessageQueue": {
      "Type": "RabbitMQ",
      "Local": {
        "ConnectionString": "amqp://guest:guest@localhost:5672/"
      },
      "Cloud": {
        "AWS": "SQS + EventBridge",
        "GCP": "Pub/Sub",
        "Azure": "Service Bus"
      }
    },
    "Jobs": {
      "Type": "Hangfire",
      "Local": {
        "Storage": "Memory"
      },
      "Cloud": {
        "Storage": "Database"
      }
    }
  }
}
```

## New CLI Commands

```bash
# Use working examples (current approach)
cp -r examples/working-aws-template my-app
cp -r examples/working-gcp-template my-app
cp -r examples/working-azure-template my-app
cp -r examples/working-container-template my-app

# Future CLI commands:
# ndc create webapp-aws --name my-app --services database,cache,storage

# Future: Create with all services
# ndc create webapp-aws --name my-app --services all --worker
```

## Service Options

### Database Options
- **PostgreSQL** (default)
- **MySQL**
- **SQL Server**
- **SQLite** (local only)

### Cache Options
- **Redis** (default)
- **In-Memory** (local fallback)

### Storage Options
- **S3-Compatible** (MinIO local, cloud-specific remote)
- **File System** (local fallback)

### Messaging Options
- **RabbitMQ** (local)
- **Cloud-specific** (SQS, Pub/Sub, Service Bus)

### Job Options
- **Hangfire** (default)
- **Quartz.NET**
- **Cloud-specific** (Lambda, Cloud Functions, Container Jobs)

## Implementation Plan

1. **Aspire Templates**: Create AppHost and ServiceDefaults projects
2. **Service Integration**: Add service-specific packages and configuration
3. **Docker Compose**: Generate docker-compose.yml for local development  
4. **Cloud Mapping**: Terraform modules for cloud-specific services
5. **Configuration**: Automatic service discovery and connection strings
6. **Documentation**: Comprehensive guides for each service type
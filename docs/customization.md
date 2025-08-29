# Customization Guide

Learn how to customize your NDC-generated projects for your specific needs.

## Command Line Options

### Basic Options
```bash
ndc create dotnet-webapp-aws --name my-api \
  --framework net8.0 \
  --port 5000 \
  --output ./projects
```

### Scaling Options
```bash
ndc create dotnet-webapp-gcp --name my-service \
  --min-instances 2 \
  --max-instances 50 \
  --cpu 2000m \
  --memory 4Gi
```

### Complete Example
```bash
ndc create dotnet-webapp-azure --name production-api \
  --framework net8.0 \
  --port 5000 \
  --min-instances 3 \
  --max-instances 100 \
  --cpu 2.0 \
  --memory 4.0Gi \
  --output ./my-projects
```

## .NET Application Customization

### Adding Dependencies
```bash
# Navigate to your project
cd my-api/src/MyApi

# Add common packages
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Serilog.AspNetCore
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### Database Integration
```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add your services
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Add API endpoints
app.MapGet("/users", async (IUserService userService) =>
    await userService.GetAllUsersAsync());

app.MapPost("/users", async (CreateUserRequest request, IUserService userService) =>
    await userService.CreateUserAsync(request));

app.Run();
```

### Logging Configuration
```csharp
// Program.cs
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Add structured logging middleware
app.UseSerilogRequestLogging();

app.MapGet("/", (ILogger<Program> logger) => {
    logger.LogInformation("Hello endpoint called at {Timestamp}", DateTime.UtcNow);
    return Results.Ok("Hello World");
});

app.Run();
```

### Configuration Management
```csharp
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyApi;Trusted_Connection=true;"
  },
  "ApiSettings": {
    "MaxRequestSize": 1000000,
    "CacheExpirationMinutes": 30
  }
}

// Program.cs
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();
builder.Services.AddSingleton(apiSettings);
```

## Infrastructure Customization

### Environment Variables
Add to Terraform `main.tf`:

**AWS (App Runner)**:
```hcl
resource "aws_apprunner_service" "api" {
  source_configuration {
    image_repository {
      image_configuration {
        port = var.port
        runtime_environment_variables = {
          "ASPNETCORE_ENVIRONMENT" = "Production"
          "CONNECTION_STRING"      = var.database_connection_string
          "API_KEY"               = var.external_api_key
        }
      }
    }
  }
}
```

**Google Cloud (Cloud Run)**:
```hcl
resource "google_cloud_run_v2_service" "app" {
  template {
    containers {
      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name  = "CONNECTION_STRING"
        value = var.database_connection_string
      }
      env {
        name = "API_SECRET"
        value_source {
          secret_key_ref {
            secret  = google_secret_manager_secret.api_secret.secret_id
            version = "latest"
          }
        }
      }
    }
  }
}
```

**Azure (Container Apps)**:
```hcl
resource "azurerm_container_app" "app" {
  template {
    container {
      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name  = "CONNECTION_STRING"
        value = var.database_connection_string
      }
      env {
        name        = "API_SECRET"
        secret_name = "api-secret"
      }
    }
  }

  secret {
    name  = "api-secret"
    value = var.api_secret_value
  }
}
```

### Custom Domain Configuration

**AWS (App Runner)**:
```hcl
resource "aws_apprunner_custom_domain_association" "api" {
  domain_name = "api.yourdomain.com"
  service_arn = aws_apprunner_service.api.arn
}

# Add to Route 53
resource "aws_route53_record" "api" {
  zone_id = var.route53_zone_id
  name    = "api"
  type    = "CNAME"
  ttl     = 300
  records = [aws_apprunner_custom_domain_association.api.certificate_validation_records[0].value]
}
```

**Google Cloud (Cloud Run)**:
```hcl
resource "google_cloud_run_domain_mapping" "api" {
  location = var.region
  name     = "api.yourdomain.com"

  metadata {
    namespace = var.project_id
  }

  spec {
    route_name = google_cloud_run_v2_service.app.name
  }
}
```

**Azure (Container Apps)**:
```hcl
resource "azurerm_container_app_custom_domain" "api" {
  name             = "api.yourdomain.com"
  container_app_id = azurerm_container_app.app.id
  
  certificate_binding_type = "SniEnabled"
  certificate_id          = azurerm_container_app_managed_certificate.api.id
}
```

### Database Integration

**AWS (RDS)**:
```hcl
resource "aws_db_instance" "api" {
  engine         = "postgres"
  engine_version = "15"
  instance_class = "db.t3.micro"
  
  db_name  = "myapi"
  username = "apiuser"
  password = var.db_password
  
  allocated_storage = 20
  storage_encrypted = true
  
  vpc_security_group_ids = [aws_security_group.db.id]
  db_subnet_group_name   = aws_db_subnet_group.api.name
}
```

**Google Cloud (Cloud SQL)**:
```hcl
resource "google_sql_database_instance" "api" {
  name             = "my-api-db"
  database_version = "POSTGRES_15"
  region           = var.region

  settings {
    tier = "db-f1-micro"
    
    database_flags {
      name  = "log_statement"
      value = "all"
    }
  }
}
```

**Azure (SQL Database)**:
```hcl
resource "azurerm_mssql_server" "api" {
  name                         = "my-api-sql"
  resource_group_name          = azurerm_resource_group.app.name
  location                     = azurerm_resource_group.app.location
  version                      = "12.0"
  administrator_login          = "apiuser"
  administrator_login_password = var.sql_password
}

resource "azurerm_mssql_database" "api" {
  name      = "MyApiDB"
  server_id = azurerm_mssql_server.api.id
  sku_name  = "Basic"
}
```

## Docker Customization

### Multi-stage Optimization
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only .csproj files first for better caching
COPY ./src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY ./src/ ./

# Build and publish
RUN dotnet publish -c Release -o /out \
    -p:PublishReadyToRun=true \
    -p:PublishSingleFile=true \
    -p:InvariantGlobalization=true \
    -p:TieredPGO=true \
    --self-contained=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install additional tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN useradd -r -u 10001 appuser

# Copy application
COPY --from=build /out/ /app/
RUN chown -R appuser:appuser /app

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
USER 10001
ENTRYPOINT ["./MyApi"]
```

### Development Dockerfile
```dockerfile
# Dockerfile.dev
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /src

# Install development tools
RUN dotnet tool install --global dotnet-ef
RUN dotnet tool install --global dotnet-watch
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy project files
COPY . .
RUN dotnet restore

# Development server
EXPOSE 8080
CMD ["dotnet", "watch", "run", "--project", "src/MyApi", "--urls", "http://0.0.0.0:8080"]
```

## Testing Setup

### Unit Tests
```bash
# Add test project
cd my-api
dotnet new xunit -n MyApi.Tests
dotnet sln add MyApi.Tests/MyApi.Tests.csproj
cd MyApi.Tests
dotnet add reference ../src/MyApi/MyApi.csproj

# Add testing packages
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package FluentAssertions
dotnet add package Moq
```

### Integration Tests
```csharp
// Tests/IntegrationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_ReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Health_ReturnsHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

### Docker Compose for Development
```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile.dev
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MyApi;User Id=sa;Password=YourStrong@Passw0rd;
    volumes:
      - .:/src
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - db_data:/var/opt/mssql

volumes:
  db_data:
```

## CI/CD Integration

### GitHub Actions
Create `.github/workflows/deploy.yml`:
```yaml
name: Deploy to Cloud

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Run tests
      run: dotnet test
    
    - name: Build and push container
      # Cloud-specific steps here
      run: |
        docker build -t my-api .
        # Push to registry
    
    - name: Deploy infrastructure
      # Terraform deployment
      run: |
        cd terraform
        terraform init
        terraform apply -auto-approve
```

This guide covers the most common customization scenarios. For specific use cases, refer to the cloud-specific deployment guides.
# NDC Azure Container Apps Example

This example demonstrates deploying a .NET 9 API to Azure Container Apps with supporting infrastructure.

## Architecture

- **Local Development**: Aspire orchestrates SQL Server, Redis, and other services in containers
- **Production**: API container deploys to Azure Container Apps with managed Azure services

## What's Included

- **API Application**: .NET 9 minimal API with health checks, OpenAPI
- **Database**: Azure SQL Database
- **Cache**: Azure Redis Cache
- **Storage**: Azure Blob Storage
- **Queue**: Azure Service Bus (optional)
- **Infrastructure**: Complete Terraform configuration
- **Container Registry**: Azure Container Registry

## Prerequisites

1. **Azure CLI**: Install and authenticate
   ```bash
   # Install Azure CLI
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   
   # Login to Azure
   az login
   
   # Set subscription (optional)
   az account set --subscription "your-subscription-id"
   ```

2. **Required Tools**:
   - .NET 9.0 SDK
   - Docker Desktop
   - Terraform >= 1.0

## Local Development

1. **Start Aspire orchestration**:
   ```bash
   dotnet run --project src/MyApp.AppHost
   ```

2. **Access services**:
   - API: http://localhost:8080
   - Aspire Dashboard: https://localhost:17001

3. **Test the API**:
   ```bash
   # Health check
   curl http://localhost:8080/health
   
   # Create user
   curl -X POST http://localhost:8080/users \
     -H "Content-Type: application/json" \
     -d '{"name":"Jane Doe","email":"jane@example.com"}'
   
   # Get users
   curl http://localhost:8080/users
   ```

## Azure Deployment

### 1. Configure Infrastructure

1. **Set variables**:
   ```bash
   cd terraform
   
   # Copy and edit terraform.tfvars
   cp terraform.tfvars.example terraform.tfvars
   ```

2. **Edit terraform.tfvars**:
   ```hcl
   service_name = "myapp-api"
   location     = "East US"
   
   # Database configuration
   enable_database          = true
   database_sku            = "Basic"
   database_admin_username = "sqladmin"
   database_admin_password = "SecurePassword123!"
   
   # Cache configuration
   enable_cache    = true
   redis_sku       = "Basic"
   redis_capacity  = 0
   
   # Storage
   enable_storage = true
   
   # Optional: Service Bus
   enable_queue = false
   ```

3. **Deploy infrastructure**:
   ```bash
   terraform init
   terraform plan
   terraform apply
   ```

### 2. Build and Deploy Container

1. **Get Container Registry credentials**:
   ```bash
   # Get ACR login server
   ACR_LOGIN_SERVER=$(terraform output -raw container_registry_login_server)
   
   # Login to ACR
   az acr login --name ${ACR_LOGIN_SERVER%.*}
   ```

2. **Build and push container**:
   ```bash
   cd ..
   
   # Build the API container
   docker build -t $ACR_LOGIN_SERVER/myapp-api:latest .
   
   # Push to ACR
   docker push $ACR_LOGIN_SERVER/myapp-api:latest
   ```

3. **Update Container App** (if needed):
   ```bash
   # Container App automatically updates with new images
   # Check deployment status
   cd terraform && terraform refresh
   
   # Get service URL
   echo "Service URL: $(terraform output -raw service_url)"
   ```

### 3. Test Production Deployment

```bash
# Get the service URL
SERVICE_URL=$(cd terraform && terraform output -raw service_url)

# Test health endpoint
curl $SERVICE_URL/health

# Test API endpoints
curl -X POST $SERVICE_URL/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Azure User","email":"azure@example.com"}'

curl $SERVICE_URL/users
```

## Infrastructure Components

### Azure Container Apps
- **CPU**: 1.0 CPU
- **Memory**: 2.0 GiB
- **Auto-scaling**: 1-5 replicas
- **Health checks**: `/health` endpoint
- **Ingress**: HTTPS enabled, external traffic allowed

### Azure SQL Database
- **Version**: SQL Server (latest)
- **SKU**: Basic (development) / Standard (production)
- **Storage**: 20 GB
- **Backup**: Automated backup retention
- **Security**: Firewall rules, encrypted connections

### Azure Redis Cache
- **Version**: Redis (latest)
- **SKU**: Basic C0 (development) / Standard (production)
- **Memory**: 250 MB (C0) / 1 GB (C1)
- **SSL**: Required, non-SSL port disabled

### Azure Blob Storage
- **Performance**: Standard
- **Replication**: LRS (Locally Redundant)
- **Access Tier**: Hot
- **Security**: Private access, managed identity authentication

### Azure Service Bus (Optional)
- **SKU**: Basic (development) / Standard (production)
- **Queue**: Single queue for message processing
- **Features**: Dead letter queue, message TTL

## Environment Variables

The application automatically receives these environment variables in production:

```bash
# Database
DATABASE_CONNECTION_STRING=Server=<sql-server>.database.windows.net;Database=<db-name>;User Id=<username>;Password=<password>;Encrypt=True;

# Cache
REDIS_CONNECTION_STRING=<redis-name>.redis.cache.windows.net:6380,password=<key>,ssl=True,abortConnect=False

# Storage
STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net
STORAGE_CONTAINER_NAME=<container-name>

# Service Bus (if enabled)
SERVICEBUS_CONNECTION_STRING=Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<key>
SERVICEBUS_QUEUE_NAME=<queue-name>

# Application
ASPNETCORE_ENVIRONMENT=Production
```

## Monitoring and Logging

1. **Container App Logs**:
   ```bash
   az containerapp logs show --name myapp-api --resource-group rg-myapp-api-xxxxx
   ```

2. **Log Analytics**: Integrated with Azure Monitor for comprehensive logging

3. **Application Insights**: Add for detailed application telemetry (optional)

## Scaling Configuration

The deployment includes auto-scaling based on:

- **HTTP requests**: Scale up with increased load
- **CPU utilization**: Target 70% CPU usage
- **Memory utilization**: Scale before memory pressure
- **Custom metrics**: Can be configured for specific needs

Modify scaling rules in the Container App configuration.

## Security Features

- **Managed Identity**: User-assigned identity for secure resource access
- **Azure Role-Based Access Control (RBAC)**: Minimal required permissions
- **Network Security**: Private endpoints where applicable
- **SSL/TLS**: HTTPS enforcement for all traffic
- **Azure Key Vault**: Can be integrated for secrets management
- **Container Security**: Non-root user, security scanning

## Cost Optimization

This configuration is optimized for development and small production workloads:

- **Container Apps**: Pay per vCPU-second and memory usage
- **SQL Database**: Basic tier with minimal DTU allocation
- **Redis Cache**: Basic tier, smallest size (C0)
- **Storage**: Standard tier with cost-effective replication
- **Log Analytics**: 30-day retention to control costs

## High Availability Setup

For production workloads, consider these upgrades:

1. **SQL Database**: Standard or Premium tier with geo-replication
2. **Redis Cache**: Standard tier with clustering
3. **Container Apps**: Multi-region deployment
4. **Storage**: Geo-redundant replication (GRS)

## Cleanup

To avoid ongoing charges:

```bash
cd terraform
terraform destroy
```

This will remove all created resources including the resource group.

## Troubleshooting

### Common Issues

1. **Container deployment failures**: Check ACR permissions and image availability
2. **Database connectivity**: Verify firewall rules and connection strings
3. **Scaling issues**: Check resource quotas and limits

### Debugging

1. **View Container App logs**:
   ```bash
   az containerapp logs show --name myapp-api --resource-group <rg-name> --follow
   ```

2. **Check Container App status**:
   ```bash
   az containerapp show --name myapp-api --resource-group <rg-name>
   ```

3. **Test database connection**:
   ```bash
   sqlcmd -S <server>.database.windows.net -d <database> -U <username> -P <password>
   ```

4. **Monitor scaling events**:
   ```bash
   az monitor activity-log list --resource-group <rg-name> --caller Microsoft.App
   ```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Deploy to Azure Container Apps

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Login to Azure
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Build and push image
        uses: azure/docker-login@v1
        with:
          login-server: ${{ secrets.ACR_LOGIN_SERVER }}
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}
      
      - run: |
          docker build -t ${{ secrets.ACR_LOGIN_SERVER }}/myapp-api:${{ github.sha }} .
          docker push ${{ secrets.ACR_LOGIN_SERVER }}/myapp-api:${{ github.sha }}
      
      - name: Deploy to Container Apps
        uses: azure/container-apps-deploy-action@v1
        with:
          containerAppName: myapp-api
          resourceGroup: ${{ secrets.RESOURCE_GROUP }}
          imageToDeploy: ${{ secrets.ACR_LOGIN_SERVER }}/myapp-api:${{ github.sha }}
```

## Next Steps

- Add Application Insights for telemetry
- Configure custom domain and managed certificates
- Set up staging environments
- Implement blue-green deployments
- Add comprehensive monitoring and alerting
- Configure backup and disaster recovery strategies
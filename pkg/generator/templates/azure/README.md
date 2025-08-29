# {{.ProjectName}} - Azure Deployment

This project was generated using **Noundry CLI** for Azure Container Apps deployment.

## Architecture

- **Azure Container Apps**: Serverless container service
- **Azure Container Registry**: Container registry
- **Terraform**: Infrastructure as Code

## Prerequisites

- Azure CLI configured with appropriate subscription
- Docker installed
- Terraform >= 1.0
- .NET {{.Framework}} SDK

## Quick Start

### 1. Login to Azure

```bash
az login
```

### 2. Deploy Infrastructure First

```bash
cd terraform
terraform init
terraform plan
terraform apply
```

### 3. Get Registry Credentials

```bash
# Get registry URL
ACR_URL=$(terraform output -raw container_registry_url)

# Login to ACR
az acr login --name $(echo $ACR_URL | cut -d'.' -f1)
```

### 4. Build and Push Container

```bash
# Build the container
docker build -t {{.ServiceName}} .

# Tag for ACR
docker tag {{.ServiceName}} $ACR_URL/{{.ServiceName}}:latest

# Push to ACR
docker push $ACR_URL/{{.ServiceName}}:latest
```

### 5. Access Your Application

After deployment, Terraform will output the application URL:

```bash
terraform output container_app_url
```

## Configuration

### Environment Variables

The application runs with:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:{{.Port}}`

### Scaling Configuration

- **Min instances**: {{.MinInstances}}
- **Max instances**: {{.MaxInstances}}
- **CPU**: {{.CPU}}
- **Memory**: {{.Memory}}

### Health Check

The service includes a health endpoint at `/health` that returns:

```json
{
  "status": "ok",
  "service": "{{.ProjectName}}",
  "timestamp": "2024-01-01T00:00:00.000Z"
}
```

## Development

Run locally with:

```bash
dotnet run --project src/{{.ProjectName}}
```

The application will be available at `http://localhost:{{.Port}}`

## Cleanup

To destroy all resources:

```bash
cd terraform
terraform destroy
```

---

*Generated with [Noundry CLI](https://github.com/noundry/noundry-cli)*
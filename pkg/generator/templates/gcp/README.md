# {{.ProjectName}} - Google Cloud Deployment

This project was generated using **Noundry CLI** for Google Cloud Run deployment.

## Architecture

- **Google Cloud Run**: Serverless container service
- **Google Artifact Registry**: Container registry
- **Terraform**: Infrastructure as Code

## Prerequisites

- Google Cloud CLI configured with appropriate project
- Docker installed
- Terraform >= 1.0
- .NET {{.Framework}} SDK

## Quick Start

### 1. Set Project ID

```bash
export PROJECT_ID="your-gcp-project-id"
```

### 2. Build and Push Container

```bash
# Build the container
docker build -t {{.ServiceName}} .

# Tag for Artifact Registry
docker tag {{.ServiceName}} {{.Region}}-docker.pkg.dev/$PROJECT_ID/{{.ServiceName}}/{{.ServiceName}}:latest

# Configure Docker for Artifact Registry
gcloud auth configure-docker {{.Region}}-docker.pkg.dev

# Push to Artifact Registry
docker push {{.Region}}-docker.pkg.dev/$PROJECT_ID/{{.ServiceName}}/{{.ServiceName}}:latest
```

### 3. Deploy Infrastructure

```bash
cd terraform
terraform init
terraform plan -var="project_id=$PROJECT_ID"
terraform apply -var="project_id=$PROJECT_ID"
```

### 4. Access Your Application

After deployment, Terraform will output the service URL:

```bash
terraform output service_url
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
terraform destroy -var="project_id=$PROJECT_ID"
```

---

*Generated with [Noundry CLI](https://github.com/noundry/noundry-cli)*
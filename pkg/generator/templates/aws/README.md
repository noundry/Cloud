# {{.ProjectName}} - AWS Deployment

This project was generated using **Noundry CLI** for AWS App Runner deployment.

## Architecture

- **AWS App Runner**: Serverless container service
- **Amazon ECR**: Container registry
- **Terraform**: Infrastructure as Code

## Prerequisites

- AWS CLI configured with appropriate permissions
- Docker installed
- Terraform >= 1.0
- .NET {{.Framework}} SDK

## Quick Start

### 1. Build and Push Container

```bash
# Build the container
docker build -t {{.ServiceName}} .

# Tag for ECR (replace ACCOUNT_ID and REGION)
docker tag {{.ServiceName}} <ACCOUNT_ID>.dkr.ecr.<REGION>.amazonaws.com/{{.ECRRepoName}}:latest

# Login to ECR
aws ecr get-login-password --region <REGION> | docker login --username AWS --password-stdin <ACCOUNT_ID>.dkr.ecr.<REGION>.amazonaws.com

# Push to ECR
docker push <ACCOUNT_ID>.dkr.ecr.<REGION>.amazonaws.com/{{.ECRRepoName}}:latest
```

### 2. Deploy Infrastructure

```bash
cd terraform
terraform init
terraform plan
terraform apply
```

### 3. Access Your Application

After deployment, Terraform will output the service URL:

```bash
terraform output apprunner_service_url
```

## Configuration

### Environment Variables

The application runs with:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:{{.Port}}`

### Scaling Configuration

- **Min instances**: {{.MinInstances}}
- **Max instances**: {{.MaxInstances}}
- **CPU**: {{.CPU}} (1024 = 1 vCPU)
- **Memory**: {{.Memory}}MB

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
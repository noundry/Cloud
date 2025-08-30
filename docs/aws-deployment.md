# AWS Deployment Guide

Deploy .NET applications to AWS using App Runner and ECR.

## Prerequisites

- AWS CLI configured with appropriate permissions
- Docker installed and running
- Terraform >= 1.0

## IAM Permissions Required

Your AWS user/role needs these permissions:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "apprunner:*",
        "ecr:*",
        "iam:CreateRole",
        "iam:AttachRolePolicy",
        "iam:PassRole"
      ],
      "Resource": "*"
    }
  ]
}
```

## Step-by-Step Deployment

### 1. Create Project
```bash
cp -r examples/working-aws-template my-api
cd my-api
```

### 2. Deploy Infrastructure First
```bash
cd terraform
terraform init
terraform plan
terraform apply
```

This creates:
- ECR repository
- App Runner service (initially failing - no image)
- IAM roles and policies

### 3. Build and Push Container
```bash
# Get AWS account ID and region
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
AWS_REGION=$(aws configure get region)

# Build the container
docker build -t my-api .

# Tag for ECR
docker tag my-api $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/noundry/my-api:latest

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | \
  docker login --username AWS --password-stdin \
  $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Push to ECR
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/noundry/my-api:latest
```

### 4. Verify Deployment
```bash
# Get service URL
terraform output apprunner_service_url

# Test the application
curl https://your-service-url.amazonaws.com/
curl https://your-service-url.amazonaws.com/health
```

## Configuration Options

### Scaling Configuration
Edit `terraform/variables.tf`:
```hcl
variable "min_instances" {
  default = 1  # Minimum instances
}

variable "max_instances" {
  default = 10  # Maximum instances
}
```

### Resource Allocation
```hcl
variable "cpu" {
  default = "1024"  # 1 vCPU (256, 512, 1024, 2048, 4096)
}

variable "memory" {
  default = "2048"  # 2GB (512, 1024, 2048, 3072, 4096, 8192, 12288)
}
```

### Environment Variables
Add to `main.tf`:
```hcl
resource "aws_apprunner_service" "api" {
  # ... existing configuration

  source_configuration {
    image_repository {
      # ... existing configuration
      image_configuration {
        port = "8080"
        runtime_environment_variables = {
          "ENVIRONMENT" = "production"
          "CUSTOM_VAR" = "value"
        }
      }
    }
  }
}
```

## Monitoring and Logs

### CloudWatch Logs
App Runner automatically sends logs to CloudWatch:
```bash
# View logs
aws logs describe-log-groups --log-group-name-prefix "/aws/apprunner/my-api"
```

### Custom Metrics
Add to your .NET application:
```csharp
// Program.cs
app.MapGet("/metrics", () => {
    return Results.Ok(new {
        requests_total = requestCount,
        uptime_seconds = (DateTime.UtcNow - startTime).TotalSeconds
    });
});
```

## Troubleshooting

### Service Deployment Failed
```bash
# Check App Runner service status
aws apprunner describe-service --service-arn $(terraform output -raw service_arn)

# Check logs
aws logs tail /aws/apprunner/my-api/service --follow
```

### Container Build Issues
```bash
# Build locally to test
docker build -t my-api .
docker run -p 8080:8080 my-api

# Check container logs
docker logs <container-id>
```

### Permission Issues
```bash
# Verify ECR permissions
aws ecr describe-repositories

# Check IAM role
aws sts assume-role --role-arn $(terraform output -raw apprunner_role_arn) --role-session-name test
```

## Cost Optimization

### Auto-pause (for development)
```hcl
# In main.tf - not recommended for production
resource "aws_apprunner_service" "api" {
  # ... existing configuration
  
  instance_configuration {
    cpu    = "256"   # Smallest CPU
    memory = "512"   # Smallest memory
  }
}
```

### Lifecycle Policies
ECR lifecycle policy is automatically configured to keep only the last 30 images.

## Cleanup

```bash
# Destroy all resources
cd terraform
terraform destroy
```

**Warning**: This will delete your ECR repository and all container images.
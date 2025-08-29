# ---------------------------
# IAM Role for App Runner
# ---------------------------
data "aws_iam_policy_document" "apprunner_trust" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["build.apprunner.amazonaws.com", "tasks.apprunner.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "apprunner_ecr_access" {
  name               = "{{.ServiceName}}-ecr-access"
  assume_role_policy = data.aws_iam_policy_document.apprunner_trust.json
}

resource "aws_iam_role_policy_attachment" "apprunner_ecr_access" {
  role       = aws_iam_role.apprunner_ecr_access.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSAppRunnerServicePolicyForECRAccess"
}

# ---------------------------
# Auto Scaling Configuration
# ---------------------------
resource "aws_apprunner_auto_scaling_configuration_version" "basic" {
  auto_scaling_configuration_name = "{{.ServiceName}}-asc"
  max_concurrency                 = 80
  min_size                        = {{.MinInstances}}
  max_size                        = {{.MaxInstances}}
}

# ---------------------------
# App Runner Service
# ---------------------------
data "aws_caller_identity" "me" {}
data "aws_region" "cur" {}

locals {
  image = "${data.aws_caller_identity.me.account_id}.dkr.ecr.${data.aws_region.cur.id}.amazonaws.com/{{.ECRRepoName}}:latest"
}

resource "aws_apprunner_service" "api" {
  service_name                   = "{{.ServiceName}}"
  auto_scaling_configuration_arn = aws_apprunner_auto_scaling_configuration_version.basic.arn
  
  source_configuration {
    image_repository {
      image_repository_type = "ECR"
      image_identifier      = local.image
      image_configuration {
        port = "{{.Port}}"
      }
    }

    authentication_configuration {
      access_role_arn = aws_iam_role.apprunner_ecr_access.arn
    }

    auto_deployments_enabled = true
  }

  instance_configuration {
    cpu    = "{{.CPU}}"
    memory = "{{.Memory}}"
  }

  health_check_configuration {
    protocol            = "HTTP"
    path                = "/health"
    interval            = 10
    timeout             = 5
    healthy_threshold   = 1
    unhealthy_threshold = 3
  }

  tags = {
    Name        = "{{.ServiceName}}"
    Environment = "production"
    ManagedBy   = "noundry"
  }
}

# ---------------------------
# ECR Repository
# ---------------------------
resource "aws_ecr_repository" "app" {
  name                 = "{{.ECRRepoName}}"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  lifecycle_policy {
    policy = jsonencode({
      rules = [
        {
          rulePriority = 1
          description  = "Keep last 30 images"
          selection = {
            tagStatus   = "any"
            countType   = "imageCountMoreThan"
            countNumber = 30
          }
          action = {
            type = "expire"
          }
        }
      ]
    })
  }

  tags = {
    Name      = "{{.ECRRepoName}}"
    ManagedBy = "noundry"
  }
}

# ---------------------------
# Outputs
# ---------------------------
output "apprunner_service_url" {
  value       = aws_apprunner_service.api.service_url
  description = "The URL of the App Runner service"
}

output "ecr_repository_url" {
  value       = aws_ecr_repository.app.repository_url
  description = "The URL of the ECR repository"
}

output "service_name" {
  value       = aws_apprunner_service.api.service_name
  description = "The name of the App Runner service"
}
# NDC CLI Examples - All Platforms

Complete examples for creating and deploying .NET applications across all supported clouds and platforms.

## 🚀 Installation

```bash
# Install NDC CLI
dotnet tool install --global NDC.Cli

# Install template packages
dotnet new install NDC.Templates.WebApp.Aws
dotnet new install NDC.Templates.WebApp.Gcp  
dotnet new install NDC.Templates.WebApp.Azure
dotnet new install NDC.Templates.WebApp.Docker
dotnet new install NDC.Templates.WebApp.K8s
```

## 📋 Quick Reference

```bash
# List all available templates
ndc list

# Basic project creation
ndc create webapp-{platform} --name MyApp

# With database only
ndc create webapp-{platform} --name MyApp --database PostgreSQL

# With common services
ndc create webapp-{platform} --name MyApp --services database,cache

# Full-featured application
ndc create webapp-{platform} --name MyApp --services all
```

---

## ☁️ **Cloud Platforms**

### 🔶 AWS App Runner

#### Basic Web App
```bash
ndc create webapp-aws --name BlogApi
```

#### With Database and Cache
```bash
ndc create webapp-aws --name EcommerceApi \
  --database PostgreSQL \
  --include-cache true
```

#### Full-Featured Application
```bash
ndc create webapp-aws --name PaymentService \
  --database PostgreSQL \
  --services cache,storage,mail,queue,jobs,worker \
  --framework net8.0 \
  --port 5000
```

#### Deployment
```bash
cd PaymentService

# Local development with Aspire
dotnet run --project src/PaymentService.AppHost

# Deploy to AWS
cd terraform
terraform init && terraform apply

# Build and push
docker build -t payment-service .
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ECR_URL
docker tag payment-service $ECR_URL:latest
docker push $ECR_URL:latest
```

### 🌐 Google Cloud Run

#### Basic Web App
```bash
ndc create webapp-gcp --name BlogApi
```

#### With Database and Storage
```bash
ndc create webapp-gcp --name FileManagerApi \
  --database PostgreSQL \
  --include-storage true \
  --include-cache true
```

#### Full-Stack E-commerce
```bash
ndc create webapp-gcp --name EcommerceApp \
  --database MySQL \
  --services cache,storage,mail,queue,jobs \
  --include-worker true \
  --min-instances 2 \
  --max-instances 50
```

#### Deployment
```bash
cd EcommerceApp

# Local development
dotnet run --project src/EcommerceApp.AppHost

# Deploy to Google Cloud
export PROJECT_ID="your-gcp-project"
cd terraform
terraform init && terraform apply -var="project_id=$PROJECT_ID"

# Build and push
docker build -t ecommerce-app .
gcloud auth configure-docker us-central1-docker.pkg.dev
docker tag ecommerce-app us-central1-docker.pkg.dev/$PROJECT_ID/ecommerce-app/ecommerce-app:latest
docker push us-central1-docker.pkg.dev/$PROJECT_ID/ecommerce-app/ecommerce-app:latest
```

### 🔷 Azure Container Apps

#### Basic Web App
```bash
ndc create webapp-azure --name CatalogApi
```

#### With SQL Server and Cache
```bash
ndc create webapp-azure --name OrderApi \
  --database SqlServer \
  --include-cache true \
  --include-queue true
```

#### Enterprise Application
```bash
ndc create webapp-azure --name EnterpriseApi \
  --database SqlServer \
  --services cache,storage,mail,queue,jobs,worker \
  --framework net8.0 \
  --cpu 2.0 \
  --memory 4.0Gi
```

#### Deployment
```bash
cd EnterpriseApi

# Local development
dotnet run --project src/EnterpriseApi.AppHost

# Deploy to Azure
az login
cd terraform
terraform init && terraform apply

# Build and push
docker build -t enterprise-api .
ACR_URL=$(terraform output -raw container_registry_url)
az acr login --name $(echo $ACR_URL | cut -d'.' -f1)
docker tag enterprise-api $ACR_URL/enterprise-api:latest
docker push $ACR_URL/enterprise-api:latest
```

---

## 🐳 **Container Platforms**

### Docker Compose

#### Development Setup
```bash
ndc create webapp-docker --name CrmApp \
  --database PostgreSQL \
  --services cache,storage,mail
```

#### Production Deployment
```bash
cd CrmApp

# Local development with Aspire
dotnet run --project src/CrmApp.AppHost

# Production deployment with external services
docker compose -f docker-compose.prod.yml up -d

# With environment file
cp .env.example .env.production
# Edit .env.production with your cloud service endpoints
docker compose --env-file .env.production -f docker-compose.prod.yml up -d
```

### 🚢 Kubernetes

#### Basic Setup
```bash
ndc create webapp-k8s --name MicroserviceApi \
  --database PostgreSQL \
  --include-cache true
```

#### Advanced Setup
```bash
ndc create webapp-k8s --name DistributedApp \
  --database PostgreSQL \
  --services cache,storage,mail,queue,jobs,worker \
  --include-worker true
```

#### Deployment to Any K8s Cluster
```bash
cd DistributedApp

# Local development
dotnet run --project src/DistributedApp.AppHost

# Build container
docker build -t distributed-app .

# Push to your registry
docker tag distributed-app your-registry/distributed-app:latest
docker push your-registry/distributed-app:latest

# Update image in deployment.yaml, then deploy
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/ingress.yaml

# Check deployment
kubectl get pods -l app=distributed-app
kubectl get service distributed-app-service
```

---

## 🌟 **Platform-as-a-Service**

### 🚂 Railway

```bash
ndc create webapp-railway --name StartupApi \
  --database PostgreSQL \
  --include-cache true

cd StartupApi

# Local development
dotnet run --project src/StartupApi.AppHost

# Deploy to Railway
railway login
railway link
railway deploy

# Add environment variables via Railway dashboard
railway variables set DATABASE_URL=postgresql://...
railway variables set REDIS_URL=redis://...
```

### 🎨 Render

```bash
ndc create webapp-render --name PortfolioApi \
  --database PostgreSQL \
  --include-storage true

cd PortfolioApi

# Deploy via Render dashboard or CLI
render deploy

# Configure via render.yaml or dashboard:
# - Database: PostgreSQL service
# - Storage: External S3/R2 bucket
```

### ✈️ Fly.io

```bash
ndc create webapp-fly --name GlobalApi \
  --database PostgreSQL \
  --services cache,storage

cd GlobalApi

# Deploy to Fly.io
fly auth login
fly launch --no-deploy  # Generate fly.toml
fly deploy

# Add secrets
fly secrets set DATABASE_URL=postgresql://...
fly secrets set REDIS_URL=redis://...
fly secrets set S3_BUCKET_NAME=my-bucket
```

### 💜 Heroku

```bash
ndc create webapp-heroku --name LegacyApi \
  --database PostgreSQL \
  --include-cache true

cd LegacyApi

# Deploy to Heroku
heroku create legacy-api
heroku addons:create heroku-postgresql:mini
heroku addons:create heroku-redis:mini
git push heroku main
```

---

## 🔧 **Advanced Examples**

### Multi-Database Support
```bash
# PostgreSQL (default)
ndc create webapp-aws --name PostgresApp --database PostgreSQL

# MySQL
ndc create webapp-gcp --name MySQLApp --database MySQL

# SQL Server  
ndc create webapp-azure --name SqlServerApp --database SqlServer

# No database
ndc create webapp-docker --name StatelessApi --database None
```

### Service Combinations
```bash
# API + Database only
ndc create webapp-aws --name SimpleApi --services database

# API + Cache + Database
ndc create webapp-gcp --name CachedApi --services database,cache

# Full microservice
ndc create webapp-azure --name MicroService \
  --services database,cache,storage,mail,queue,jobs,worker

# Custom service selection
ndc create webapp-k8s --name CustomApi \
  --include-database true \
  --include-cache true \
  --include-storage true \
  --include-mail false \
  --include-queue false \
  --include-jobs true
```

### Framework and Resource Options
```bash
# .NET 8 with custom resources
ndc create webapp-aws --name LegacyApi \
  --framework net8.0 \
  --cpu 2048 \
  --memory 4096 \
  --min-instances 3 \
  --max-instances 100

# Custom port
ndc create webapp-gcp --name CustomPortApi \
  --port 5000 \
  --services database,cache

# Development vs Production
ndc create webapp-azure --name DevApi \
  --services database,cache \
  --min-instances 1 \
  --max-instances 5    # Development settings

ndc create webapp-azure --name ProdApi \
  --services all \
  --min-instances 5 \
  --max-instances 200  # Production settings
```

---

## 🔄 **Workflow Examples**

### Startup → Scale Workflow
```bash
# 1. Start simple
ndc create webapp-aws --name StartupApp --services database

# 2. Add caching as you grow
# Edit appsettings.json: "Cache": { "Enabled": true }
# Update terraform/variables.tf: include_cache = true
terraform apply

# 3. Add file storage
# Edit appsettings.json: "Storage": { "Enabled": true }
# Update terraform/variables.tf: include_storage = true  
terraform apply

# 4. Add background processing
# Edit appsettings.json: "Jobs": { "Enabled": true }
# Add worker project as needed
```

### Multi-Environment Deployment
```bash
# Create base project
ndc create webapp-aws --name MyApp --services database,cache

cd MyApp

# Development environment
ndc deploy --platform aws --environment development

# Staging environment
ndc deploy --platform aws --environment staging

# Production environment  
ndc deploy --platform aws --environment production \
  --min-instances 3 \
  --max-instances 50
```

### Local → Cloud Migration
```bash
# Start with local development
ndc create webapp-docker --name MyApp --services all

# Develop locally with Aspire
dotnet run --project src/MyApp.AppHost

# When ready, migrate to cloud
ndc create webapp-aws --name MyApp --services all
# Copy your API code from docker version
# Deploy with terraform
```

### Cross-Platform Testing
```bash
# Create for multiple platforms to test portability
ndc create webapp-aws --name MyApp --services database,cache,storage
ndc create webapp-gcp --name MyApp --services database,cache,storage  
ndc create webapp-azure --name MyApp --services database,cache,storage

# Same API code, different cloud services
# Test configuration flexibility across platforms
```

---

## 📊 **Common Service Configurations**

### Blog/Content Management
```bash
ndc create webapp-{platform} --name BlogApi \
  --database PostgreSQL \
  --include-storage true \
  --include-cache true
# Services: Database for content, Storage for media, Cache for performance
```

### E-commerce Platform
```bash
ndc create webapp-{platform} --name EcommerceApi \
  --database PostgreSQL \
  --services cache,storage,mail,queue,jobs \
  --include-worker true
# Full stack: DB, Cache, File Storage, Email, Queue processing, Background jobs
```

### Real-time Chat Application
```bash
ndc create webapp-{platform} --name ChatApi \
  --database PostgreSQL \
  --include-cache true \
  --include-queue true
# Services: Database for users/messages, Cache for sessions, Queue for real-time messaging
```

### Analytics/Reporting Service
```bash
ndc create webapp-{platform} --name AnalyticsApi \
  --database PostgreSQL \
  --services cache,storage,queue,jobs \
  --include-worker true
# Services: Database for data, Cache for queries, Storage for reports, Queue+Jobs for processing
```

### Authentication Service
```bash
ndc create webapp-{platform} --name AuthApi \
  --database PostgreSQL \
  --include-cache true \
  --include-mail true
# Services: Database for users, Cache for sessions/tokens, Email for verification
```

### File Processing Service
```bash
ndc create webapp-{platform} --name FileProcessorApi \
  --database PostgreSQL \
  --services storage,queue,jobs \
  --include-worker true
# Services: Database for metadata, Storage for files, Queue+Jobs for processing
```

---

## 🎯 **Template Quick Reference**

| Template | Best For | Services | Platform |
|----------|----------|----------|----------|
| `webapp-aws` | AWS-native apps | RDS, ElastiCache, S3, SES, SQS | AWS App Runner |
| `webapp-gcp` | Google Cloud apps | Cloud SQL, Memorystore, GCS, SendGrid, Pub/Sub | Cloud Run |
| `webapp-azure` | Azure-native apps | PostgreSQL, Azure Cache, Blob, Email, Service Bus | Container Apps |
| `webapp-docker` | Self-hosted/hybrid | Configurable external services | Docker Compose |
| `webapp-k8s` | Kubernetes deployments | External cloud services | Any K8s cluster |
| `webapp-railway` | Rapid prototyping | Railway add-ons | Railway |
| `webapp-render` | Full-stack apps | Render services | Render |
| `webapp-fly` | Global distribution | Fly.io services | Fly.io |
| `webapp-heroku` | Traditional PaaS | Heroku add-ons | Heroku |

---

## 🚀 **Complete Workflow Examples**

### Example 1: Simple Blog API (AWS)
```bash
# 1. Create project
ndc create webapp-aws --name BlogApi \
  --database PostgreSQL \
  --include-cache true \
  --include-storage true

cd BlogApi

# 2. Local development
dotnet run --project src/BlogApi.AppHost
# ✅ PostgreSQL + Redis + MinIO running locally
# ✅ API at http://localhost:8080
# ✅ Aspire dashboard at https://localhost:17001

# 3. Test API endpoints
curl http://localhost:8080/          # Hello message
curl http://localhost:8080/health    # Health check
curl http://localhost:8080/users     # Database endpoint
curl http://localhost:8080/cache/test # Cache endpoint

# 4. Deploy to AWS
cd terraform
terraform init
terraform apply

# 5. Build and push container (API only)
docker build -t blog-api .
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $(terraform output -raw ecr_repository_url)
docker tag blog-api $(terraform output -raw ecr_repository_url):latest
docker push $(terraform output -raw ecr_repository_url):latest

# 6. Access deployed app
curl $(terraform output -raw app_runner_service_url)
```

### Example 2: E-commerce Platform (Google Cloud)
```bash
# 1. Create full-stack project
ndc create webapp-gcp --name EcommerceApi \
  --database PostgreSQL \
  --services cache,storage,mail,queue,jobs,worker \
  --port 8080 \
  --framework net9.0

cd EcommerceApi

# 2. Local development with all services
dotnet run --project src/EcommerceApi.AppHost
# ✅ PostgreSQL, Redis, MinIO, MailHog, RabbitMQ
# ✅ API + Worker projects
# ✅ Background job processing
# ✅ Full service ecosystem

# 3. Deploy to Google Cloud  
export PROJECT_ID="my-ecommerce-gcp"
cd terraform
terraform init
terraform apply -var="project_id=$PROJECT_ID"

# 4. Build and deploy
docker build -t ecommerce-api .
gcloud auth configure-docker us-central1-docker.pkg.dev
docker tag ecommerce-api us-central1-docker.pkg.dev/$PROJECT_ID/ecommerce-api/ecommerce-api:latest
docker push us-central1-docker.pkg.dev/$PROJECT_ID/ecommerce-api/ecommerce-api:latest

# ✅ Cloud Run deploys API only
# ✅ Connects to Cloud SQL, Memorystore, Cloud Storage, Pub/Sub
```

### Example 3: Kubernetes Microservice (Multi-Cloud)
```bash
# 1. Create Kubernetes-ready project
ndc create webapp-k8s --name UserService \
  --database PostgreSQL \
  --services cache,storage,queue \
  --framework net9.0

cd UserService

# 2. Local development
dotnet run --project src/UserService.AppHost

# 3. Build for production
docker build -t user-service .

# 4. Deploy to any Kubernetes cluster
# Update k8s/configmap.yaml with your cloud service endpoints
kubectl apply -f k8s/

# Example for AWS EKS with external RDS + ElastiCache
kubectl create secret generic user-service-secrets \
  --from-literal=database-connection-string="Host=my-rds.amazonaws.com;Database=users;Username=app;Password=secret"

kubectl create configmap user-service-config \
  --from-literal=redis-connection-string="my-elasticache.cache.amazonaws.com:6379" \
  --from-literal=s3-bucket-name="my-user-data-bucket"

kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/ingress.yaml
```

### Example 4: Multi-Platform Deployment
```bash
# 1. Create platform-agnostic project
ndc create webapp-docker --name FlexibleApi \
  --services database,cache,storage

cd FlexibleApi

# 2. Local development
dotnet run --project src/FlexibleApi.AppHost

# 3. Deploy to multiple platforms using the same API

# Docker Swarm
docker stack deploy -c docker-compose.prod.yml flexible-api

# Railway
railway link && railway deploy

# Render  
render deploy

# Fly.io
fly launch && fly deploy

# Same API code, different deployment targets!
```

---

## ⚙️ **Service Configuration Examples**

### Database-Only Application
```bash
ndc create webapp-aws --name SimpleApi --services database
# ✅ Just RDS PostgreSQL
# ✅ Minimal infrastructure
# ✅ Perfect for CRUD APIs
```

### High-Performance Cached API
```bash
ndc create webapp-gcp --name FastApi \
  --database PostgreSQL \
  --include-cache true \
  --cpu 2000m \
  --memory 4Gi \
  --max-instances 100
# ✅ Database + Redis caching
# ✅ High-performance configuration
# ✅ Aggressive auto-scaling
```

### File Processing Pipeline
```bash
ndc create webapp-azure --name FileProcessor \
  --database PostgreSQL \
  --services storage,queue,jobs,worker \
  --include-worker true
# ✅ Database for metadata
# ✅ Blob storage for files
# ✅ Service Bus for queue
# ✅ Background job processing
```

### Notification Service
```bash
ndc create webapp-aws --name NotificationApi \
  --database PostgreSQL \
  --services cache,mail,queue \
  --include-worker true
# ✅ Database for users/preferences
# ✅ Cache for frequently accessed data
# ✅ SES for email delivery
# ✅ SQS for message processing
```

---

## 📋 **Parameter Quick Reference**

### Core Parameters
```bash
--name MyApp                    # Project name (required)
--framework net9.0             # Target framework (net9.0, net8.0)  
--port 8080                    # Application port
--database PostgreSQL          # Database type (PostgreSQL, MySQL, SqlServer, None)
--output ./projects            # Output directory
```

### Service Flags
```bash
--include-cache                # Add Redis cache
--include-storage              # Add S3-compatible storage
--include-mail                 # Add email service
--include-queue                # Add message queue  
--include-jobs                 # Add background jobs
--include-worker               # Add worker service project
--services all                 # Include all services
--services database,cache      # Specific services only
```

### Cloud Configuration  
```bash
--cpu 1024                     # CPU allocation (cloud-specific)
--memory 2048                  # Memory allocation (cloud-specific)  
--min-instances 1              # Minimum instances
--max-instances 10             # Maximum instances
--region us-east-1             # Cloud region
--environment production       # Deployment environment
```

### Example Combinations
```bash
# Minimal microservice
ndc create webapp-aws --name UserApi --services database

# Standard web application
ndc create webapp-gcp --name WebApp --services database,cache,storage

# Enterprise application
ndc create webapp-azure --name EnterpriseApp \
  --services all \
  --framework net8.0 \
  --min-instances 3 \
  --max-instances 100

# Development setup
ndc create webapp-docker --name DevApp \
  --services database,cache \
  --framework net9.0 \
  --port 5000
```

This comprehensive guide covers every major deployment scenario with NDC!
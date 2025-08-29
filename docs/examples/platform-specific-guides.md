# Platform-Specific Deployment Guides

Detailed guides for deploying NDC-generated applications to specific platforms.

---

## ☁️ **Cloud Platforms**

### 🔶 **AWS App Runner** 

#### Prerequisites
```bash
# Install AWS CLI
aws --version

# Configure credentials
aws configure
# OR use IAM roles, AWS SSO, etc.
```

#### Complete Workflow
```bash
# 1. Create project
ndc create webapp-aws --name PaymentApi \
  --database PostgreSQL \
  --services cache,storage,mail,queue \
  --min-instances 2 \
  --max-instances 20

cd PaymentApi

# 2. Local development with Aspire
dotnet run --project src/PaymentApi.AppHost
# ✅ All services running locally
# ✅ Test at http://localhost:8080

# 3. Deploy infrastructure
cd terraform
terraform init
terraform apply
# ✅ Creates: RDS PostgreSQL, ElastiCache Redis, S3 bucket, SES, SQS

# 4. Build and push API container
cd ..
docker build -t payment-api .

# Get ECR URL from Terraform
ECR_URL=$(cd terraform && terraform output -raw ecr_repository_url)

# Login and push
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ECR_URL
docker tag payment-api $ECR_URL:latest
docker push $ECR_URL:latest

# 5. Access your app
APP_URL=$(cd terraform && terraform output -raw app_runner_service_url)
curl $APP_URL/health
```

#### AWS-Specific Features
- **Auto-scaling**: Based on HTTP requests and CPU
- **ECR Integration**: Private container registry
- **IAM Roles**: Least-privilege access to AWS services
- **CloudWatch**: Automatic logging and monitoring
- **VPC Connector**: Private network access (optional)

---

### 🌐 **Google Cloud Run**

#### Prerequisites
```bash
# Install gcloud CLI
gcloud --version

# Login and set project
gcloud auth login
gcloud config set project YOUR_PROJECT_ID
```

#### Complete Workflow
```bash
# 1. Create project
ndc create webapp-gcp --name AnalyticsApi \
  --database MySQL \
  --services cache,storage,queue \
  --cpu 2000m \
  --memory 4Gi

cd AnalyticsApi

# 2. Local development
dotnet run --project src/AnalyticsApi.AppHost

# 3. Deploy infrastructure  
export PROJECT_ID="your-analytics-project"
cd terraform
terraform init
terraform apply -var="project_id=$PROJECT_ID"
# ✅ Creates: Cloud SQL MySQL, Memorystore Redis, Cloud Storage, Pub/Sub

# 4. Build and push
cd ..
docker build -t analytics-api .

# Configure Artifact Registry
gcloud auth configure-docker us-central1-docker.pkg.dev

# Tag and push
docker tag analytics-api us-central1-docker.pkg.dev/$PROJECT_ID/analytics-api/analytics-api:latest
docker push us-central1-docker.pkg.dev/$PROJECT_ID/analytics-api/analytics-api:latest

# 5. Access your app
SERVICE_URL=$(cd terraform && terraform output -raw service_url)
curl $SERVICE_URL/health
```

#### GCP-Specific Features
- **Scale-to-Zero**: Pay only when serving requests
- **Global Load Balancing**: Multi-region deployment
- **Cloud IAM**: Fine-grained permissions
- **Cloud Logging**: Structured logging integration
- **VPC Connector**: Private service access

---

### 🔷 **Azure Container Apps**

#### Prerequisites  
```bash
# Install Azure CLI
az --version

# Login
az login
az account set --subscription "Your Subscription"
```

#### Complete Workflow
```bash
# 1. Create project
ndc create webapp-azure --name CrmApi \
  --database SqlServer \
  --services cache,storage,mail \
  --cpu 1.5 \
  --memory 3.0Gi

cd CrmApi

# 2. Local development
dotnet run --project src/CrmApi.AppHost

# 3. Deploy infrastructure
cd terraform  
terraform init
terraform apply
# ✅ Creates: PostgreSQL Flexible Server, Azure Cache for Redis, Blob Storage

# 4. Build and push
cd ..
docker build -t crm-api .

# Get ACR URL
ACR_URL=$(cd terraform && terraform output -raw container_registry_url)
az acr login --name $(echo $ACR_URL | cut -d'.' -f1)

# Tag and push
docker tag crm-api $ACR_URL/crm-api:latest
docker push $ACR_URL/crm-api:latest

# 5. Access your app
APP_URL=$(cd terraform && terraform output -raw container_app_url)
curl $APP_URL/health
```

#### Azure-Specific Features
- **Consumption-Based Pricing**: Pay per request
- **KEDA Scaling**: Event-driven autoscaling
- **Managed Identity**: Passwordless service access
- **Log Analytics**: Comprehensive monitoring
- **VNet Integration**: Private network connectivity

---

## 🐳 **Container Platforms**

### **Docker Compose (Self-Hosted)**

#### Local Development
```bash
ndc create webapp-docker --name SelfHostedApi \
  --services database,cache,storage

cd SelfHostedApi

# Local development with Aspire
dotnet run --project src/SelfHostedApi.AppHost

# Alternative: Docker Compose local
docker compose up
```

#### Production Deployment
```bash
# 1. Configure external services in .env.production
cat > .env.production << EOF
DATABASE_CONNECTION_STRING=Host=my-postgres-server;Database=api;Username=app;Password=secret
REDIS_CONNECTION_STRING=my-redis-server:6379
S3_BUCKET_NAME=my-s3-bucket
AWS_ACCESS_KEY_ID=AKIAEXAMPLE
AWS_SECRET_ACCESS_KEY=secretkey
EOF

# 2. Deploy with external services
docker compose --env-file .env.production -f docker-compose.prod.yml up -d

# 3. Scale if needed
docker compose -f docker-compose.prod.yml up --scale api=3

# 4. Check status
curl http://localhost:8080/health
```

---

### ☸️ **Kubernetes (Any Cluster)**

#### EKS (AWS)
```bash
ndc create webapp-k8s --name K8sApi --services database,cache,storage

cd K8sApi

# Configure for AWS services
kubectl create secret generic k8s-api-secrets \
  --from-literal=database-connection-string="Host=my-rds.amazonaws.com;Database=api;Username=app;Password=secret" \
  --from-literal=aws-access-key-id="AKIAEXAMPLE" \
  --from-literal=aws-secret-access-key="secretkey"

kubectl create configmap k8s-api-config \
  --from-literal=redis-connection-string="my-elasticache.cache.amazonaws.com:6379" \
  --from-literal=s3-bucket-name="my-api-bucket" \
  --from-literal=aws-region="us-east-1"

# Deploy
kubectl apply -f k8s/
```

#### GKE (Google Cloud)
```bash
ndc create webapp-k8s --name GkeApi --services database,cache,storage

cd GkeApi

# Configure for GCP services
kubectl create secret generic gke-api-secrets \
  --from-literal=database-connection-string="Host=10.1.1.1;Database=api;Username=app;Password=secret"

kubectl create configmap gke-api-config \
  --from-literal=redis-connection-string="10.1.1.2:6379" \
  --from-literal=gcs-bucket-name="my-gke-bucket"

kubectl apply -f k8s/
```

#### AKS (Azure)
```bash
ndc create webapp-k8s --name AksApi --services database,cache,storage

cd AksApi

# Configure for Azure services
kubectl create secret generic aks-api-secrets \
  --from-literal=database-connection-string="Server=my-azure-sql.database.windows.net;Database=api;User Id=app;Password=secret;"

kubectl create configmap aks-api-config \
  --from-literal=redis-connection-string="my-azure-cache.redis.cache.windows.net:6380,ssl=True" \
  --from-literal=storage-account="mystorageaccount" \
  --from-literal=storage-container="api-files"

kubectl apply -f k8s/
```

---

## 🌟 **PaaS Platforms**

### 🚂 **Railway**

```bash
# 1. Create project
ndc create webapp-railway --name RailwayApi \
  --services database,cache

cd RailwayApi

# 2. Local development
dotnet run --project src/RailwayApi.AppHost

# 3. Deploy to Railway
railway login
railway link  # Link to existing project or create new
railway up    # Deploy

# 4. Add services via Railway dashboard
# - PostgreSQL add-on
# - Redis add-on

# 5. Configure environment variables in Railway dashboard
# DATABASE_URL=postgresql://...   (auto-set by PostgreSQL add-on)
# REDIS_URL=redis://...          (auto-set by Redis add-on)
```

### 🎨 **Render**

```bash
# 1. Create project  
ndc create webapp-render --name RenderApi \
  --services database,cache,storage

cd RenderApi

# 2. Create render.yaml (if not using dashboard)
cat > render.yaml << EOF
services:
  - type: web
    name: render-api
    env: docker
    dockerfilePath: ./Dockerfile
    envVars:
      - key: DATABASE_URL
        fromDatabase:
          name: render-api-db
          property: connectionString
      - key: REDIS_URL  
        fromService:
          type: redis
          name: render-api-redis
          property: connectionString
EOF

# 3. Deploy
render deploy

# Services auto-created via Render dashboard:
# - PostgreSQL database  
# - Redis instance
# - External storage (configure S3/R2)
```

### ✈️ **Fly.io**

```bash
# 1. Create project
ndc create webapp-fly --name FlyApi \
  --services database,cache

cd FlyApi

# 2. Local development
dotnet run --project src/FlyApi.AppHost

# 3. Deploy to Fly
fly auth login
fly launch --no-deploy  # Creates fly.toml

# 4. Add Fly services
fly postgres create      # Creates PostgreSQL cluster  
fly redis create        # Creates Redis instance

# 5. Configure secrets
fly secrets set DATABASE_URL=postgresql://...  # From postgres create output
fly secrets set REDIS_URL=redis://...          # From redis create output

# 6. Deploy application
fly deploy

# 7. Scale globally (optional)
fly scale count 3         # 3 instances
fly regions add fra       # Add Frankfurt region
fly scale count 1 --region fra
```

### 💜 **Heroku**

```bash
# 1. Create project
ndc create webapp-heroku --name HerokuApi \
  --services database,cache,mail

cd HerokuApi

# 2. Local development  
dotnet run --project src/HerokuApi.AppHost

# 3. Deploy to Heroku
heroku create heroku-api
heroku stack:set container    # Use Docker deployment

# 4. Add Heroku add-ons
heroku addons:create heroku-postgresql:mini
heroku addons:create heroku-redis:mini  
heroku addons:create sendgrid:starter

# 5. Deploy
git push heroku main

# Environment variables auto-set by add-ons:
# DATABASE_URL=postgresql://...
# REDIS_URL=redis://...
# SENDGRID_API_KEY=...
```

---

## 🔧 **Advanced Deployment Scenarios**

### Multi-Environment Pipeline
```bash
# 1. Create base project
ndc create webapp-aws --name MyApi --services database,cache,storage

# 2. Deploy to development
cd terraform
terraform workspace new development
terraform apply -var="environment=development" -var="min_instances=1"

# 3. Deploy to staging  
terraform workspace new staging
terraform apply -var="environment=staging" -var="min_instances=2"

# 4. Deploy to production
terraform workspace new production  
terraform apply -var="environment=production" -var="min_instances=5" -var="max_instances=100"
```

### Hybrid Cloud Deployment
```bash
# Database in cloud, everything else self-hosted
ndc create webapp-docker --name HybridApi --services database,cache,storage

# Configure .env.production
DATABASE_CONNECTION_STRING=Host=my-cloud-db.amazonaws.com;...  # Cloud database
REDIS_CONNECTION_STRING=redis:6379                            # Local Redis
S3_ENDPOINT=http://minio:9000                                 # Local MinIO

docker compose -f docker-compose.prod.yml up
```

### Blue/Green Deployment
```bash
# Create identical environments
ndc create webapp-gcp --name BlueGreenApi --services all

# Deploy blue environment
cd terraform
terraform apply -var="environment=blue"

# Test blue environment
# ... testing ...

# Deploy green environment
terraform apply -var="environment=green"

# Switch traffic via load balancer
# Switch DNS or ingress configuration
```

### Disaster Recovery Setup
```bash
# Primary region
ndc create webapp-aws --name PrimaryApi \
  --services all \
  --region us-east-1

# DR region
ndc create webapp-aws --name DrApi \
  --services all \
  --region us-west-2

# Cross-region database replication
# Configure in terraform/main.tf:
# - RDS cross-region read replicas
# - S3 cross-region replication  
# - ElastiCache Global Datastore
```

---

## 📊 **Performance Optimization Examples**

### High-Throughput API
```bash
ndc create webapp-gcp --name HighThroughputApi \
  --database PostgreSQL \
  --include-cache true \
  --cpu 4000m \
  --memory 8Gi \
  --min-instances 10 \
  --max-instances 500

# Optimize in terraform/variables.tf:
# database_instance_class = "db-custom-8-32768"  # 8 vCPU, 32GB RAM
# cache_node_type = "cache.r6g.2xlarge"          # High-performance Redis
```

### Cost-Optimized API
```bash
ndc create webapp-azure --name CostOptimizedApi \
  --database PostgreSQL \
  --include-cache false \
  --cpu 0.5 \
  --memory 1.0Gi \
  --min-instances 0 \
  --max-instances 5

# Scale-to-zero configuration
# Minimal database configuration
# No cache to reduce costs
```

### Global Distribution
```bash
# Primary deployment
ndc create webapp-aws --name GlobalApi \
  --services all \
  --region us-east-1

# Regional deployments
ndc create webapp-aws --name GlobalApi-EU \
  --services cache,storage \
  --region eu-west-1

# Configure global load balancing and data synchronization
```

---

## 🛡️ **Security-Focused Deployments**

### Private Network Deployment (AWS)
```bash
ndc create webapp-aws --name SecureApi \
  --services database,cache

# Configure in terraform/main.tf:
# - VPC with private subnets
# - RDS in private subnet
# - ElastiCache in private subnet
# - VPC Endpoint for S3
# - App Runner VPC Connector
```

### Zero-Trust Architecture (Azure)
```bash
ndc create webapp-azure --name ZeroTrustApi \
  --services all

# Configure in terraform:
# - Private Container App Environment
# - Azure Private Link for services
# - Managed Identity for authentication
# - Key Vault for secrets
# - Private DNS zones
```

### Compliance-Ready (Multi-Cloud)
```bash
ndc create webapp-k8s --name ComplianceApi \
  --services database,cache,storage

# Configure compliance features:
# - Pod Security Standards
# - Network Policies
# - Resource Quotas
# - Audit logging
# - Encrypted storage
# - Regular security scanning
```

---

## 🚀 **Real-World Examples**

### E-commerce Platform
```bash
# Product Catalog Service
ndc create webapp-gcp --name ProductCatalog \
  --database MySQL \
  --services cache,storage \
  --max-instances 50

# Order Processing Service  
ndc create webapp-aws --name OrderProcessor \
  --database PostgreSQL \
  --services cache,queue,jobs,worker \
  --include-worker true

# Payment Service
ndc create webapp-azure --name PaymentService \
  --database SqlServer \
  --services cache,queue \
  --min-instances 3

# Deploy each service independently
# Configure service-to-service communication
# Set up API Gateway for unified API
```

### SaaS Application
```bash
# User Management
ndc create webapp-aws --name UserService \
  --database PostgreSQL \
  --services cache,mail

# Billing Service
ndc create webapp-aws --name BillingService \
  --database PostgreSQL \
  --services cache,queue,jobs

# Analytics Service
ndc create webapp-gcp --name AnalyticsService \
  --database PostgreSQL \
  --services cache,storage,queue \
  --include-worker true

# Frontend API Gateway
ndc create webapp-azure --name ApiGateway \
  --services cache \
  --min-instances 2
```

### Content Management System
```bash
# Content API
ndc create webapp-gcp --name ContentApi \
  --database PostgreSQL \
  --services cache,storage,queue

# Media Processing Service
ndc create webapp-aws --name MediaProcessor \
  --database PostgreSQL \
  --services storage,queue,jobs,worker \
  --include-worker true \
  --cpu 4096 \
  --memory 8192

# CDN and Static Assets
# Configure Cloud Storage/S3/Blob with CDN
# Set up image optimization pipeline
```

This guide provides real-world, production-ready deployment patterns for every major platform!
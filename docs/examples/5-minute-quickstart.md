# 5-Minute Quick Start Guide

Get up and running with NDC in 5 minutes or less!

## ⏱️ **Option 1: AWS (5 minutes)**

### Prerequisites (1 minute)
```bash
# Install if not already installed
dotnet tool install --global NDC.Cli
dotnet new install NDC.Templates.WebApp.Aws
aws configure  # Have AWS credentials ready
```

### Create and Run (2 minutes)
```bash
# Create a full-featured app
ndc create webapp-aws --name QuickDemo \
  --services database,cache,storage

cd QuickDemo

# Start local development (Aspire orchestrates everything)
dotnet run --project src/QuickDemo.AppHost
```

**🎉 You now have:**
- ✅ Web API at http://localhost:8080
- ✅ PostgreSQL, Redis, MinIO running automatically  
- ✅ Aspire dashboard at https://localhost:17001
- ✅ Ready-to-deploy AWS infrastructure

### Deploy to Cloud (2 minutes)
```bash
# Deploy infrastructure
cd terraform && terraform init && terraform apply -auto-approve

# Build and deploy API container  
cd .. && docker build -t quick-demo .
ECR_URL=$(cd terraform && terraform output -raw ecr_repository_url)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ECR_URL
docker tag quick-demo $ECR_URL:latest && docker push $ECR_URL:latest

# Get your live URL
terraform output app_runner_service_url
```

**🎉 You now have a production API running on AWS with managed PostgreSQL, Redis, and S3!**

---

## ⏱️ **Option 2: Docker (2 minutes)**

### Super Quick Local Setup
```bash
# 1. Create project (30 seconds)
ndc create webapp-docker --name InstantApp --services database,cache
cd InstantApp

# 2. Start everything (90 seconds)  
dotnet run --project src/InstantApp.AppHost

# 🎉 Full-stack app running at http://localhost:8080
# 🎉 Aspire dashboard at https://localhost:17001  
# 🎉 PostgreSQL + Redis automatically orchestrated
```

**Perfect for:** Learning, prototyping, local development

---

## ⏱️ **Option 3: Google Cloud (4 minutes)**

### Quick Cloud Deployment
```bash
# 1. Create project (30 seconds)
ndc create webapp-gcp --name CloudDemo --services database,cache,storage
cd CloudDemo

# 2. Local development (30 seconds)
dotnet run --project src/CloudDemo.AppHost

# 3. Deploy to Google Cloud (3 minutes)
export PROJECT_ID="your-gcp-project-id"
cd terraform
terraform init && terraform apply -var="project_id=$PROJECT_ID" -auto-approve

# 4. Build and deploy
cd ..
docker build -t cloud-demo .
gcloud auth configure-docker us-central1-docker.pkg.dev
docker tag cloud-demo us-central1-docker.pkg.dev/$PROJECT_ID/cloud-demo/cloud-demo:latest
docker push us-central1-docker.pkg.dev/$PROJECT_ID/cloud-demo/cloud-demo:latest

# 🎉 Live on Google Cloud Run with Cloud SQL + Memorystore + Cloud Storage
```

---

## 🎯 **Use Case Quick Starts**

### Blog/CMS (3 minutes)
```bash
ndc create webapp-aws --name BlogApi \
  --database PostgreSQL \
  --services cache,storage

cd BlogApi && dotnet run --project src/BlogApi.AppHost
# ✅ Database for posts, Cache for performance, Storage for media
```

### E-commerce Backend (4 minutes)
```bash
ndc create webapp-gcp --name ShopApi \
  --database MySQL \
  --services cache,storage,mail,queue,jobs \
  --include-worker true

cd ShopApi && dotnet run --project src/ShopApi.AppHost
# ✅ Full e-commerce stack with order processing
```

### File Processing Service (3 minutes)
```bash
ndc create webapp-azure --name FileProcessor \
  --database PostgreSQL \
  --services storage,queue,jobs,worker \
  --include-worker true

cd FileProcessor && dotnet run --project src/FileProcessor.AppHost
# ✅ File upload, processing queue, background jobs
```

### Real-time Chat API (2 minutes)
```bash
ndc create webapp-docker --name ChatApi \
  --database PostgreSQL \
  --services cache,queue

cd ChatApi && dotnet run --project src/ChatApi.AppHost
# ✅ Database for messages, Cache for sessions, Queue for real-time
```

---

## 🏆 **Production-Ready in 10 Minutes**

### Complete SaaS Backend
```bash
# 1. Create comprehensive project (1 minute)
ndc create webapp-aws --name SaasApi \
  --database PostgreSQL \
  --services all \
  --include-worker true \
  --framework net9.0 \
  --min-instances 3 \
  --max-instances 100

cd SaasApi

# 2. Local development and testing (3 minutes)
dotnet run --project src/SaasApi.AppHost

# Test all endpoints:
curl http://localhost:8080/health      # Health check
curl http://localhost:8080/users       # Database
curl http://localhost:8080/cache/test  # Redis
curl http://localhost:8080/files       # Storage  
# Visit http://localhost:1025 for MailHog
# Visit https://localhost:17001 for Aspire dashboard

# 3. Deploy full production infrastructure (4 minutes)
cd terraform
terraform init
terraform apply -auto-approve
# ✅ RDS PostgreSQL cluster
# ✅ ElastiCache Redis cluster
# ✅ S3 bucket with encryption
# ✅ SES email service
# ✅ SQS queue with DLQ
# ✅ App Runner with auto-scaling

# 4. Deploy application (2 minutes)
cd ..
docker build -t saas-api .
ECR_URL=$(cd terraform && terraform output -raw ecr_repository_url)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ECR_URL
docker tag saas-api $ECR_URL:latest
docker push $ECR_URL:latest

# 🎉 Production SaaS API running with full managed service stack!
APP_URL=$(cd terraform && terraform output -raw app_runner_service_url)  
echo "Your API is live at: $APP_URL"
```

---

## 🎨 **Creative Examples**

### API-First Development
```bash
# 1. Start with minimal API
ndc create webapp-docker --name ApiFirst --services database

# 2. Add services as you build features
# Edit appsettings.json to enable cache
"Services": { "Cache": { "Enabled": true } }

# 3. Migrate to cloud when ready
ndc create webapp-aws --name ApiFirst --services database,cache
# Copy your API code over
```

### Microservice Architecture
```bash
# User Service
ndc create webapp-k8s --name UserService --services database,cache --port 8081

# Product Service  
ndc create webapp-k8s --name ProductService --services database,cache,storage --port 8082

# Order Service
ndc create webapp-k8s --name OrderService --services database,cache,queue,jobs --port 8083

# API Gateway
ndc create webapp-k8s --name ApiGateway --services cache --port 8080

# Deploy all to same K8s cluster
# Configure service mesh (Istio/Linkerd)
# Set up distributed tracing
```

### Serverless-First Development  
```bash
# Start serverless-ready
ndc create webapp-gcp --name ServerlessApi \
  --services database,cache,storage \
  --min-instances 0 \
  --max-instances 1000

# Benefits:
# ✅ Scale to zero when not used
# ✅ Pay per request
# ✅ Global distribution ready
# ✅ Cold start optimized
```

### Development → Production Pipeline
```bash
# 1. Local development
ndc create webapp-docker --name MyApp --services all
cd MyApp && dotnet run --project src/MyApp.AppHost

# 2. Staging deployment
ndc create webapp-gcp --name MyApp-staging --services all
# Deploy to Cloud Run for easy testing

# 3. Production deployment
ndc create webapp-aws --name MyApp-prod --services all  
# Deploy to AWS for enterprise features

# Same API code, different environments!
```

These examples cover every common scenario and get you productive immediately with NDC!
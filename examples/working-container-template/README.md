# NDC Container Platform Example

This example demonstrates deploying a .NET 9 API using generic container orchestration (Docker Compose and Kubernetes).

## Architecture

- **Local Development**: Aspire orchestrates PostgreSQL, Redis, and other services in containers
- **Production**: API deployed via Docker Compose or Kubernetes with containerized services

## What's Included

- **API Application**: .NET 9 minimal API with health checks, OpenAPI
- **Database**: PostgreSQL container
- **Cache**: Redis container
- **Storage**: File system storage (or S3-compatible)
- **Reverse Proxy**: Nginx with load balancing and SSL termination
- **Orchestration**: Docker Compose and Kubernetes manifests

## Prerequisites

**For Docker Compose:**
- Docker and Docker Compose
- .NET 9.0 SDK (for local development)

**For Kubernetes:**
- Kubernetes cluster (local or cloud)
- kubectl configured
- Helm (optional, recommended)

## Local Development with Aspire

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
     -d '{"name":"Container User","email":"container@example.com"}'
   
   # Get users
   curl http://localhost:8080/users
   ```

## Docker Compose Deployment

### 1. Production with Docker Compose

1. **Build the application**:
   ```bash
   # Build the API container
   docker build -t myapp-api:latest .
   ```

2. **Configure environment variables**:
   ```bash
   # Copy and edit .env file
   cp .env.example .env
   ```

3. **Edit .env file**:
   ```bash
   # Database
   POSTGRES_DB=myapp_db
   POSTGRES_USER=appuser
   POSTGRES_PASSWORD=SecurePassword123!
   
   # Email (optional)
   SMTP_HOST=smtp.gmail.com
   SMTP_PORT=587
   SMTP_USERNAME=your-email@gmail.com
   SMTP_PASSWORD=your-app-password
   FROM_EMAIL=noreply@yourdomain.com
   FROM_NAME=MyApp
   ```

4. **Start services**:
   ```bash
   docker-compose up -d
   ```

5. **Access the application**:
   - API (via Nginx): http://localhost
   - Direct API: http://localhost:8080
   - Health check: http://localhost/health

### 2. SSL/TLS Configuration (Production)

1. **Get SSL certificates** (Let's Encrypt example):
   ```bash
   # Using certbot
   sudo certbot certonly --standalone -d yourdomain.com
   
   # Copy certificates
   sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem ./ssl/
   sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem ./ssl/
   sudo chown $USER:$USER ./ssl/*.pem
   ```

2. **Update nginx.conf** to enable HTTPS (uncomment SSL section)

3. **Restart services**:
   ```bash
   docker-compose restart nginx
   ```

### 3. Scaling Services

```bash
# Scale API instances
docker-compose up -d --scale api=3

# Scale with specific configuration
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --scale api=5
```

## Kubernetes Deployment

### 1. Deploy to Kubernetes

1. **Create namespace**:
   ```bash
   kubectl create namespace myapp
   kubectl config set-context --current --namespace=myapp
   ```

2. **Deploy secrets and config**:
   ```bash
   # Create secrets (modify values as needed)
   kubectl create secret generic app-secrets \
     --from-literal=database-connection-string="Host=postgres-service;Database=myapp_db;Username=appuser;Password=SecurePassword123!;Port=5432" \
     --from-literal=redis-connection-string="redis-service:6379" \
     --from-literal=smtp-username="your-username" \
     --from-literal=smtp-password="your-password"
   
   # Apply configuration
   kubectl apply -f k8s/configmap.yaml
   ```

3. **Deploy database and cache** (if not using external services):
   ```bash
   # PostgreSQL
   kubectl apply -f k8s/postgres-deployment.yaml
   
   # Redis
   kubectl apply -f k8s/redis-deployment.yaml
   ```

4. **Deploy the API**:
   ```bash
   # Update image in deployment.yaml first
   kubectl apply -f k8s/deployment.yaml
   ```

5. **Configure ingress**:
   ```bash
   # Install ingress controller (if not already installed)
   kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml
   
   # Apply ingress configuration
   kubectl apply -f k8s/ingress.yaml
   ```

### 2. Monitor Deployment

```bash
# Check pod status
kubectl get pods

# View logs
kubectl logs -l app=myapp-api -f

# Check services
kubectl get services

# Check ingress
kubectl get ingress
```

### 3. Scale Application

```bash
# Scale deployment
kubectl scale deployment myapp-api --replicas=5

# Auto-scaling (HPA)
kubectl autoscale deployment myapp-api --cpu-percent=70 --min=2 --max=10
```

## Cloud Platform Deployment

### AWS ECS/Fargate

```bash
# Build and push to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

docker tag myapp-api:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/myapp-api:latest
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/myapp-api:latest

# Deploy using ECS CLI or CloudFormation
```

### Google Cloud Run

```bash
# Build and deploy
gcloud builds submit --tag gcr.io/PROJECT-ID/myapp-api
gcloud run deploy --image gcr.io/PROJECT-ID/myapp-api --platform managed
```

### Azure Container Instances

```bash
# Deploy to ACI
az container create \
  --resource-group myapp-rg \
  --name myapp-api \
  --image myregistry.azurecr.io/myapp-api:latest \
  --dns-name-label myapp-api \
  --ports 8080
```

## Monitoring and Logging

### 1. Container Logs

**Docker Compose:**
```bash
# View all logs
docker-compose logs -f

# View API logs only
docker-compose logs -f api

# View database logs
docker-compose logs -f postgres
```

**Kubernetes:**
```bash
# All pods in namespace
kubectl logs -l app=myapp-api -f --all-containers=true

# Specific pod
kubectl logs myapp-api-<pod-id> -f

# Previous container logs (if pod restarted)
kubectl logs myapp-api-<pod-id> -p
```

### 2. Health Monitoring

**Built-in Health Checks:**
- Docker: Health check configured in Dockerfile
- Kubernetes: Liveness, readiness, and startup probes configured

**External Monitoring Tools:**
- Prometheus + Grafana
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Jaeger for distributed tracing

### 3. Metrics Collection

Example Prometheus configuration for API metrics:

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'myapp-api'
    static_configs:
      - targets: ['api:8080']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

## Backup and Persistence

### Database Backup

**Docker Compose:**
```bash
# Create backup
docker-compose exec postgres pg_dump -U appuser myapp_db > backup.sql

# Restore backup
docker-compose exec -T postgres psql -U appuser myapp_db < backup.sql
```

**Kubernetes:**
```bash
# Create backup job
kubectl create job --from=cronjob/postgres-backup manual-backup-$(date +%s)
```

### Volume Management

**Docker Compose:**
- Data persisted in named volumes
- Volumes survive container recreation
- Backup volumes regularly

**Kubernetes:**
- Persistent Volume Claims for data storage
- Storage classes for different performance tiers
- Automated backup via operators

## Security Best Practices

### 1. Container Security

- **Non-root user**: Container runs as non-privileged user
- **Minimal base image**: Using Microsoft's official .NET runtime image
- **Security scanning**: Regular vulnerability scans
- **Read-only filesystem**: Where possible

### 2. Network Security

- **TLS encryption**: All external communication encrypted
- **Network policies**: Kubernetes network policies restrict traffic
- **Firewall rules**: Limit exposed ports
- **Service mesh**: Consider Istio/Linkerd for advanced security

### 3. Secrets Management

- **Kubernetes secrets**: Base64 encoded (not secure for production)
- **External secrets**: Use HashiCorp Vault, AWS Secrets Manager, etc.
- **Sealed secrets**: Encrypted secrets stored in Git
- **Secret rotation**: Regular rotation of sensitive credentials

## Performance Optimization

### 1. Application Performance

- **Connection pooling**: Database connection pooling enabled
- **Caching**: Redis for application-level caching
- **Static files**: Serve via Nginx for better performance

### 2. Container Optimization

- **Multi-stage builds**: Minimize final image size
- **Layer caching**: Optimize Dockerfile for build caching
- **Resource limits**: Set appropriate CPU/memory limits

### 3. Scaling Strategies

- **Horizontal Pod Autoscaling**: Scale based on CPU/memory/custom metrics
- **Vertical Pod Autoscaling**: Adjust resource requests automatically
- **Cluster autoscaling**: Add/remove nodes based on demand

## Troubleshooting

### Common Issues

1. **Container won't start**:
   ```bash
   # Check logs
   docker logs <container-id>
   kubectl logs <pod-name>
   
   # Check resource constraints
   kubectl describe pod <pod-name>
   ```

2. **Database connection issues**:
   ```bash
   # Test connectivity
   kubectl exec -it <api-pod> -- nslookup postgres-service
   
   # Check service endpoints
   kubectl get endpoints
   ```

3. **Performance issues**:
   ```bash
   # Check resource usage
   kubectl top pods
   kubectl top nodes
   
   # Monitor application metrics
   curl http://localhost/health
   ```

### Debugging Tools

```bash
# Enter container for debugging
docker-compose exec api bash
kubectl exec -it <pod-name> -- bash

# Network troubleshooting
kubectl run -it --rm debug --image=busybox --restart=Never -- sh

# Port forwarding for local access
kubectl port-forward service/myapp-api-service 8080:80
```

## CI/CD Pipeline Examples

### GitHub Actions

```yaml
name: Deploy to Kubernetes

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build image
        run: docker build -t myapp-api:${{ github.sha }} .
      
      - name: Deploy to Kubernetes
        run: |
          kubectl set image deployment/myapp-api api=myapp-api:${{ github.sha }}
          kubectl rollout status deployment/myapp-api
```

### GitLab CI

```yaml
stages:
  - build
  - deploy

build:
  stage: build
  script:
    - docker build -t $CI_REGISTRY_IMAGE/myapp-api:$CI_COMMIT_SHA .
    - docker push $CI_REGISTRY_IMAGE/myapp-api:$CI_COMMIT_SHA

deploy:
  stage: deploy
  script:
    - kubectl set image deployment/myapp-api api=$CI_REGISTRY_IMAGE/myapp-api:$CI_COMMIT_SHA
    - kubectl rollout status deployment/myapp-api
```

## Next Steps

- Set up monitoring and alerting
- Implement proper backup strategies
- Add service mesh for advanced networking
- Set up multi-environment deployments (dev/staging/prod)
- Implement GitOps with ArgoCD or Flux
- Add comprehensive logging and distributed tracing
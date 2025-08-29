# NDC Aspire Templates for AWS

This package contains .NET Aspire templates optimized for AWS deployment with complete service orchestration.

## Installation

```bash
# Install the CLI tool
dotnet tool install --global NDC.Cli

# Install the template package
dotnet new install NDC.Templates.Aspire.Aws
```

## Available Templates

### aspire-webapp-aws
Creates an Aspire-enabled web application optimized for AWS App Runner with configurable services.

**Usage:**
```bash
# Basic web app with database and cache
ndc create aspire-webapp-aws --name MyApp

# Full-featured application
ndc create aspire-webapp-aws --name MyProject \
  --database PostgreSQL \
  --include-cache true \
  --include-storage true \
  --include-mail true \
  --include-queue true \
  --include-jobs true \
  --include-worker true
```

**Parameters:**
- `--framework`: Target framework (net9.0, net8.0) - default: net9.0
- `--database`: Database provider (PostgreSQL, MySQL, SqlServer, None) - default: PostgreSQL
- `--include-cache`: Include Redis cache - default: true
- `--include-storage`: Include S3-compatible storage - default: false
- `--include-mail`: Include email service - default: false
- `--include-queue`: Include message queue - default: false
- `--include-jobs`: Include background jobs - default: false
- `--include-worker`: Include worker service project - default: false
- `--port`: Application port - default: 8080

## Generated Project Structure

```
MyApp/
├── src/
│   ├── MyApp.AppHost/           # Aspire orchestration
│   ├── MyApp.Api/               # Main web API
│   ├── MyApp.ServiceDefaults/   # Shared service configuration
│   └── MyApp.Worker/            # Background worker (optional)
├── terraform/                   # AWS infrastructure
├── docker-compose.yml          # Local development
├── README.md                   # Deployment guide
└── MyApp.sln                   # Visual Studio solution
```

## Local Development

```bash
# Start all services with Aspire
dotnet run --project src/MyApp.AppHost

# Or use Docker Compose
docker compose up
```

The Aspire dashboard will be available at https://localhost:17001

## AWS Deployment

1. **Configure AWS credentials**
2. **Deploy infrastructure:**
   ```bash
   cd terraform
   terraform init
   terraform plan
   terraform apply
   ```
3. **Build and push container:**
   ```bash
   # Follow the generated README.md instructions
   ```

## Service Mapping

### Local Development (Aspire)
- **Database**: PostgreSQL/MySQL/SQL Server container
- **Cache**: Redis container
- **Storage**: MinIO (S3-compatible)
- **Mail**: MailHog SMTP server
- **Queue**: RabbitMQ container

### AWS Cloud Deployment
- **Database**: RDS (Aurora PostgreSQL/MySQL) or RDS SQL Server
- **Cache**: ElastiCache for Redis
- **Storage**: Amazon S3
- **Mail**: Amazon SES
- **Queue**: Amazon SQS + EventBridge
- **Compute**: AWS App Runner

## Features

✅ **Production-Ready**: Optimized Dockerfiles and cloud configurations
✅ **Service Discovery**: Automatic service resolution between local and cloud
✅ **Health Checks**: Built-in health monitoring
✅ **Observability**: OpenTelemetry integration
✅ **Security**: Non-root containers, least-privilege IAM
✅ **Scalability**: Auto-scaling configurations
✅ **Developer Experience**: Full local development environment

## Requirements

- .NET 9.0 SDK or later
- Docker Desktop
- AWS CLI (for deployment)
- Terraform >= 1.0

## Support

- 📖 [Documentation](https://github.com/plsft/noundry-cloud-cli/tree/main/docs)
- 🐛 [Issues](https://github.com/plsft/noundry-cloud-cli/issues)
- 💬 [Discussions](https://github.com/plsft/noundry-cloud-cli/discussions)

## License

MIT License - see [LICENSE](https://github.com/plsft/noundry-cloud-cli/blob/main/LICENSE) for details.
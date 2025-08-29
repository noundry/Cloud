# Contributing to NDC (Noundry Deploy CLI)

Thank you for your interest in contributing to NDC! This document provides guidelines for contributing to the project.

## Getting Started

### Prerequisites
- Go 1.21 or later
- Docker (for testing generated containers)
- Terraform >= 1.0 (for testing infrastructure)
- Git

### Development Setup
1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/yourusername/noundry-cloud-cli.git
   cd noundry-cloud-cli
   ```
3. Install dependencies:
   ```bash
   go mod download
   ```
4. Test the CLI:
   ```bash
   make dev-test
   ```

## Development Workflow

### 1. Create a Feature Branch
```bash
git checkout -b feature/your-feature-name
```

### 2. Make Your Changes
- Follow Go conventions and best practices
- Add tests for new functionality
- Update documentation as needed

### 3. Test Your Changes
```bash
# Run unit tests
make test

# Test CLI generation
make dev-test

# Clean up test output
make dev-clean

# Format code
make fmt

# Lint code (requires golangci-lint)
make lint
```

### 4. Commit Your Changes
```bash
git add .
git commit -m "feat: add amazing new feature"
```

Use conventional commit format:
- `feat:` for new features
- `fix:` for bug fixes
- `docs:` for documentation changes
- `test:` for test additions/changes
- `refactor:` for code refactoring
- `ci:` for CI/CD changes

### 5. Push and Create PR
```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

## Code Structure

```
noundry-cloud-cli/
├── cmd/                    # CLI commands (Cobra)
│   ├── root.go            # Root command
│   ├── create.go          # Create command
│   └── list.go            # List command
├── pkg/
│   └── generator/         # Template generation logic
│       ├── config.go      # Configuration structures
│       ├── generator.go   # Main generation logic
│       └── templates/     # Embedded templates
│           ├── aws/       # AWS templates
│           ├── gcp/       # Google Cloud templates
│           └── azure/     # Azure templates
├── docs/                  # Documentation
├── .github/               # GitHub workflows
└── Makefile              # Build and development tasks
```

## Adding New Templates

### 1. Create Template Directory
```bash
mkdir -p pkg/generator/templates/newcloud
```

### 2. Add Template Files
Create the necessary files with Go template syntax:
- `terraform/` - Infrastructure as Code
- `src/` - .NET application template
- `Dockerfile` - Container configuration
- `README.md` - Deployment instructions

### 3. Update Configuration
Add your template to `pkg/generator/config.go`:
```go
var supportedTemplates = map[string]bool{
    "dotnet-webapp-newcloud": true,
    // ... existing templates
}
```

### 4. Update CLI Commands
Add your template to `cmd/list.go` and update help text.

### 5. Add Documentation
Create a deployment guide in `docs/newcloud-deployment.md`.

### 6. Test Thoroughly
```bash
# Test template generation
go run . create dotnet-webapp-newcloud --name test-app --output ./test
cd test/test-app

# Test container build
docker build -t test-app .

# Test Terraform (if possible)
cd terraform
terraform init
terraform validate
```

## Template Best Practices

### .NET Application Templates
- Use .NET 9 by default
- Include health check endpoint at `/health`
- Use structured logging
- Follow security best practices (non-root user)
- Include comprehensive appsettings.json

### Dockerfile Templates
- Use multi-stage builds
- Optimize for production
- Use specific base image tags
- Create non-root user
- Include health checks where supported

### Terraform Templates
- Follow cloud provider best practices
- Use least-privilege security
- Include comprehensive outputs
- Add resource tags
- Support variable customization

### Documentation
- Provide step-by-step deployment guides
- Include troubleshooting sections
- Add cost optimization tips
- Show real-world examples

## Testing Guidelines

### Unit Tests
- Test configuration validation
- Test template generation logic
- Test helper functions

### Integration Tests
- Test CLI command execution
- Test template file generation
- Validate generated Terraform

### Manual Testing
- Generate all template types
- Verify file structure and content
- Test container builds
- Validate Terraform syntax

## Pull Request Guidelines

### PR Title
Use conventional commit format for the title:
```
feat: add support for new cloud provider
fix: resolve template generation issue
docs: update deployment guide
```

### PR Description
Include:
- What changes were made
- Why the changes were needed
- How to test the changes
- Any breaking changes
- Screenshots (if applicable)

### PR Checklist
- [ ] Tests pass
- [ ] Code is formatted (`make fmt`)
- [ ] Code is linted (`make lint`)
- [ ] Documentation updated
- [ ] Templates tested manually
- [ ] Commit messages follow conventional format

## Reporting Issues

### Bug Reports
Include:
- NDC version (`ndc --version`)
- Operating system and architecture
- Go version (if building from source)
- Steps to reproduce
- Expected vs actual behavior
- Error messages or logs

### Feature Requests
Include:
- Use case description
- Proposed solution
- Alternative solutions considered
- Additional context

## Code Review Process

1. All PRs require at least one review
2. Maintainers will review for:
   - Code quality and Go best practices
   - Template functionality and best practices
   - Documentation completeness
   - Test coverage
3. Address reviewer feedback
4. Once approved, maintainers will merge

## Release Process

1. Update version in relevant files
2. Create release notes
3. Tag the release
4. GitHub Actions builds and publishes binaries
5. Update documentation if needed

## Getting Help

- 📖 Check existing documentation
- 🔍 Search existing issues
- 💬 Start a discussion for questions
- 🐛 Create an issue for bugs
- 📧 Email maintainers for sensitive issues

Thank you for contributing to NDC! 🎉
package generator

import (
	"errors"
	"fmt"
)

type ProjectConfig struct {
	Name         string
	ProjectName  string
	Template     string
	OutputDir    string
	Framework    string
	Port         int
	MinInstances int
	MaxInstances int
	CPU          string
	Memory       string
}

var supportedTemplates = map[string]bool{
	"dotnet-webapp-aws":   true,
	"dotnet-webapp-gcp":   true,
	"dotnet-webapp-azure": true,
}

func ValidateTemplate(template string) error {
	if !supportedTemplates[template] {
		return fmt.Errorf("unsupported template '%s'. Run 'noundry list' to see available templates", template)
	}
	return nil
}

func (c *ProjectConfig) Validate() error {
	if c.Name == "" {
		return errors.New("project name is required")
	}
	
	if c.Port <= 0 || c.Port > 65535 {
		return errors.New("port must be between 1 and 65535")
	}
	
	if c.MinInstances < 0 {
		return errors.New("min-instances must be >= 0")
	}
	
	if c.MaxInstances <= 0 {
		return errors.New("max-instances must be > 0")
	}
	
	if c.MinInstances > c.MaxInstances {
		return errors.New("min-instances cannot be greater than max-instances")
	}
	
	return ValidateTemplate(c.Template)
}

func (c *ProjectConfig) GetCloudDefaults() {
	switch c.Template {
	case "dotnet-webapp-aws":
		if c.CPU == "" {
			c.CPU = "1024"  // 1 vCPU
		}
		if c.Memory == "" {
			c.Memory = "2048"  // 2 GB
		}
	case "dotnet-webapp-gcp":
		if c.CPU == "" {
			c.CPU = "1000m"  // 1 CPU
		}
		if c.Memory == "" {
			c.Memory = "2Gi"  // 2 GB
		}
	case "dotnet-webapp-azure":
		if c.CPU == "" {
			c.CPU = "1.0"  // 1 CPU
		}
		if c.Memory == "" {
			c.Memory = "2.0Gi"  // 2 GB
		}
	}
}
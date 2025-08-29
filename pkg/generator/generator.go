package generator

import (
	"embed"
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"text/template"
	"crypto/rand"
	"unicode"
)

//go:embed templates/*
var templatesFS embed.FS

func GenerateProject(config ProjectConfig) error {
	if err := config.Validate(); err != nil {
		return err
	}
	
	config.GetCloudDefaults()
	
	outputPath := filepath.Join(config.OutputDir, config.Name)
	
	if err := os.MkdirAll(outputPath, 0755); err != nil {
		return fmt.Errorf("failed to create output directory: %v", err)
	}
	
	var templateDir string
	switch config.Template {
	case "dotnet-webapp-aws":
		templateDir = "templates/aws"
	case "dotnet-webapp-gcp":
		templateDir = "templates/gcp"
	case "dotnet-webapp-azure":
		templateDir = "templates/azure"
	}
	
	return processTemplateDir(templateDir, outputPath, config)
}

func processTemplateDir(templateDir, outputPath string, config ProjectConfig) error {
	entries, err := templatesFS.ReadDir(templateDir)
	if err != nil {
		return fmt.Errorf("failed to read template directory: %v", err)
	}
	
	for _, entry := range entries {
		sourcePath := filepath.Join(templateDir, entry.Name())
		destName := processFileName(entry.Name(), config)
		destPath := filepath.Join(outputPath, destName)
		
		if entry.IsDir() {
			if err := os.MkdirAll(destPath, 0755); err != nil {
				return fmt.Errorf("failed to create directory %s: %v", destPath, err)
			}
			if err := processTemplateDir(sourcePath, destPath, config); err != nil {
				return err
			}
		} else {
			if err := processTemplateFile(sourcePath, destPath, config); err != nil {
				return err
			}
		}
	}
	
	return nil
}

func processFileName(fileName string, config ProjectConfig) string {
	// Replace template placeholders in file names
	result := strings.ReplaceAll(fileName, "{{.ProjectName}}", config.ProjectName)
	return result
}

func processTemplateFile(sourcePath, destPath string, config ProjectConfig) error {
	content, err := templatesFS.ReadFile(sourcePath)
	if err != nil {
		return fmt.Errorf("failed to read template file %s: %v", sourcePath, err)
	}
	
	tmpl, err := template.New("template").Parse(string(content))
	if err != nil {
		return fmt.Errorf("failed to parse template %s: %v", sourcePath, err)
	}
	
	destFile, err := os.Create(destPath)
	if err != nil {
		return fmt.Errorf("failed to create file %s: %v", destPath, err)
	}
	defer destFile.Close()
	
	templateData := map[string]interface{}{
		"Name":           config.Name,
		"ProjectName":    toTitleCase(config.Name),
		"Framework":      config.Framework,
		"Port":           config.Port,
		"MinInstances":   config.MinInstances,
		"MaxInstances":   config.MaxInstances,
		"CPU":            config.CPU,
		"Memory":         config.Memory,
		"ECRRepoName":    fmt.Sprintf("%s/%s", "noundry", strings.ToLower(config.Name)),
		"ServiceName":    strings.ToLower(config.Name),
		"Region":         getDefaultRegion(config.Template),
		
		// GUIDs for solution projects
		"ProjectGuid":         generateGUID(),
		"AppHostGuid":         generateGUID(),
		"ApiGuid":            generateGUID(),
		"ServiceDefaultsGuid": generateGUID(),
		"WorkerGuid":         generateGUID(),
		
		// Aspire-specific configuration
		"Database":            getDefaultDatabase(config),
		"IncludeCache":       config.IncludeCache || isAspireTemplate(config.Template),
		"IncludeStorage":     config.IncludeStorage || isFullStackTemplate(config.Template),
		"IncludeMail":        config.IncludeMail || isFullStackTemplate(config.Template),
		"IncludeMessageQueue": config.IncludeMessageQueue || isFullStackTemplate(config.Template),
		"IncludeJobs":        config.IncludeJobs || isFullStackTemplate(config.Template),
		"IncludeWorker":      config.IncludeWorker || isFullStackTemplate(config.Template),
	}
	
	if err := tmpl.Execute(destFile, templateData); err != nil {
		return fmt.Errorf("failed to execute template %s: %v", sourcePath, err)
	}
	
	return nil
}

func toTitleCase(s string) string {
	// Convert to title case, handling hyphens and underscores
	words := strings.FieldsFunc(s, func(c rune) bool {
		return c == '-' || c == '_' || unicode.IsSpace(c)
	})
	
	for i, word := range words {
		if len(word) > 0 {
			words[i] = strings.ToUpper(string(word[0])) + strings.ToLower(word[1:])
		}
	}
	
	return strings.Join(words, "")
}

func generateGUID() string {
	// Generate a simple GUID-like string for Visual Studio solution files
	b := make([]byte, 16)
	rand.Read(b)
	return fmt.Sprintf("%08X-%04X-%04X-%04X-%012X", 
		b[0:4], b[4:6], b[6:8], b[8:10], b[10:16])
}

func getDefaultRegion(template string) string {
	switch template {
	case "dotnet-webapp-aws", "aspire-webapp-aws", "aspire-fullstack-aws":
		return "us-east-1"
	case "dotnet-webapp-gcp", "aspire-webapp-gcp", "aspire-fullstack-gcp":
		return "us-central1"
	case "dotnet-webapp-azure", "aspire-webapp-azure", "aspire-fullstack-azure":
		return "eastus"
	default:
		return "us-east-1"
	}
}

func getDefaultDatabase(config ProjectConfig) string {
	if config.Database != "" {
		return config.Database
	}
	
	// Default database per cloud provider
	switch config.Template {
	case "aspire-webapp-aws", "aspire-fullstack-aws":
		return "PostgreSQL"
	case "aspire-webapp-gcp", "aspire-fullstack-gcp":
		return "PostgreSQL"
	case "aspire-webapp-azure", "aspire-fullstack-azure":
		return "SqlServer"
	default:
		return "PostgreSQL"
	}
}

func isAspireTemplate(template string) bool {
	return strings.HasPrefix(template, "aspire-")
}

func isFullStackTemplate(template string) bool {
	return strings.Contains(template, "fullstack")
}
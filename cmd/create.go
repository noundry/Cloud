package cmd

import (
	"fmt"
	"path/filepath"
	"strings"
	"unicode"

	"github.com/plsft/noundry-cloud-cli/pkg/generator"
	"github.com/spf13/cobra"
)

var createCmd = &cobra.Command{
	Use:   "create [template]",
	Short: "Create a new project from a template",
	Long: `Create a new .NET project with cloud deployment configuration.

Available templates:
  dotnet-webapp-aws    - .NET web app for AWS App Runner
  dotnet-webapp-gcp    - .NET web app for Google Cloud Run  
  dotnet-webapp-azure  - .NET web app for Azure Container Apps`,
	Args: cobra.ExactArgs(1),
	Run:  createProject,
}

var (
	projectName   string
	outputDir     string
	framework     string
	port          int
	minInstances  int
	maxInstances  int
	cpu           string
	memory        string
	
	// Aspire-specific options
	database      string
	includeCache  bool
	includeStorage bool
	includeMail   bool
	includeMessageQueue bool
	includeJobs   bool
	includeWorker bool
	servicesFlag  string
)

func init() {
	rootCmd.AddCommand(createCmd)
	
	createCmd.Flags().StringVarP(&projectName, "name", "n", "", "Project name (required)")
	createCmd.Flags().StringVarP(&outputDir, "output", "o", ".", "Output directory")
	createCmd.Flags().StringVarP(&framework, "framework", "f", "net9.0", ".NET framework version")
	createCmd.Flags().IntVarP(&port, "port", "p", 8080, "Application port")
	createCmd.Flags().IntVar(&minInstances, "min-instances", 1, "Minimum instances")
	createCmd.Flags().IntVar(&maxInstances, "max-instances", 5, "Maximum instances")
	createCmd.Flags().StringVar(&cpu, "cpu", "", "CPU allocation (cloud-specific)")
	createCmd.Flags().StringVar(&memory, "memory", "", "Memory allocation (cloud-specific)")
	
	// Aspire-specific flags
	createCmd.Flags().StringVar(&database, "database", "", "Database type (PostgreSQL, MySQL, SqlServer)")
	createCmd.Flags().BoolVar(&includeCache, "cache", false, "Include Redis cache")
	createCmd.Flags().BoolVar(&includeStorage, "storage", false, "Include S3-compatible storage")
	createCmd.Flags().BoolVar(&includeMail, "mail", false, "Include email service")
	createCmd.Flags().BoolVar(&includeMessageQueue, "queue", false, "Include message queue")
	createCmd.Flags().BoolVar(&includeJobs, "jobs", false, "Include background jobs")
	createCmd.Flags().BoolVar(&includeWorker, "worker", false, "Include worker service")
	createCmd.Flags().StringVar(&servicesFlag, "services", "", "Comma-separated services (database,cache,storage,mail,queue,jobs,worker,all)")
	
	createCmd.MarkFlagRequired("name")
}

func createProject(cmd *cobra.Command, args []string) {
	template := args[0]
	
	// Parse services flag
	services := parseServicesFlag(servicesFlag)
	
	config := generator.ProjectConfig{
		Name:         projectName,
		ProjectName:  toTitleCase(projectName),
		Template:     template,
		OutputDir:    outputDir,
		Framework:    framework,
		Port:         port,
		MinInstances: minInstances,
		MaxInstances: maxInstances,
		CPU:          cpu,
		Memory:       memory,
		
		// Aspire-specific options
		Database:            database,
		IncludeCache:        includeCache || services["cache"] || services["all"],
		IncludeStorage:      includeStorage || services["storage"] || services["all"],
		IncludeMail:         includeMail || services["mail"] || services["all"],
		IncludeMessageQueue: includeMessageQueue || services["queue"] || services["all"],
		IncludeJobs:         includeJobs || services["jobs"] || services["all"],
		IncludeWorker:       includeWorker || services["worker"] || services["all"],
	}
	
	if err := generator.ValidateTemplate(template); err != nil {
		fmt.Printf("Error: %v\n", err)
		return
	}
	
	outputPath := filepath.Join(outputDir, projectName)
	fmt.Printf("Creating %s project '%s' in %s\n", template, projectName, outputPath)
	
	if err := generator.GenerateProject(config); err != nil {
		fmt.Printf("Error creating project: %v\n", err)
		return
	}
	
	fmt.Printf("✅ Successfully created %s project!\n", projectName)
	fmt.Printf("📁 Location: %s\n", outputPath)
	fmt.Printf("🚀 Next steps:\n")
	fmt.Printf("   cd %s\n", projectName)
	
	switch template {
	case "dotnet-webapp-aws":
		fmt.Printf("   # Configure AWS credentials\n")
		fmt.Printf("   # Update terraform/variables.tf as needed\n")
		fmt.Printf("   cd terraform && terraform init && terraform plan\n")
	case "dotnet-webapp-gcp":
		fmt.Printf("   # Configure GCP credentials\n")
		fmt.Printf("   # Update terraform/variables.tf as needed\n")
		fmt.Printf("   cd terraform && terraform init && terraform plan\n")
	case "dotnet-webapp-azure":
		fmt.Printf("   # Configure Azure credentials\n")
		fmt.Printf("   # Update terraform/variables.tf as needed\n")
		fmt.Printf("   cd terraform && terraform init && terraform plan\n")
	}
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
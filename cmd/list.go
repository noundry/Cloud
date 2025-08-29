package cmd

import (
	"fmt"

	"github.com/spf13/cobra"
)

var listCmd = &cobra.Command{
	Use:   "list",
	Short: "List available templates",
	Long:  `Display all available project templates with descriptions.`,
	Run:   listTemplates,
}

func init() {
	rootCmd.AddCommand(listCmd)
}

func listTemplates(cmd *cobra.Command, args []string) {
	fmt.Println("Available NDC Templates:")
	fmt.Println()
	
	templates := []struct {
		Name        string
		Cloud       string
		Service     string
		Description string
	}{
		{
			Name:        "dotnet-webapp-aws",
			Cloud:       "AWS",
			Service:     "App Runner",
			Description: ".NET web application deployed to AWS App Runner with ECR",
		},
		{
			Name:        "dotnet-webapp-gcp",
			Cloud:       "Google Cloud",
			Service:     "Cloud Run",
			Description: ".NET web application deployed to Google Cloud Run with Artifact Registry",
		},
		{
			Name:        "dotnet-webapp-azure",
			Cloud:       "Azure",
			Service:     "Container Apps",
			Description: ".NET web application deployed to Azure Container Apps with ACR",
		},
	}
	
	for _, t := range templates {
		fmt.Printf("📦 %s\n", t.Name)
		fmt.Printf("   Cloud: %s\n", t.Cloud)
		fmt.Printf("   Service: %s\n", t.Service)
		fmt.Printf("   Description: %s\n", t.Description)
		fmt.Println()
	}
	
	fmt.Println("Usage:")
	fmt.Println("  ndc create <template> --name <project-name>")
	fmt.Println()
	fmt.Println("Example:")
	fmt.Println("  ndc create dotnet-webapp-aws --name my-api")
}
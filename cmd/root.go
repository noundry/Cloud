package cmd

import (
	"fmt"
	"os"

	"github.com/spf13/cobra"
)

var rootCmd = &cobra.Command{
	Use:   "ndc",
	Short: "NDC (Noundry Deploy CLI) - Generate cloud-native .NET deployment templates",
	Long: `NDC is a CLI tool that generates production-ready .NET application 
deployment templates for AWS, Google Cloud, and Azure using Terraform.

Create containerized .NET applications with best practices for:
- Container optimization
- Cloud-native services
- Infrastructure as Code
- Auto-scaling configurations`,
	Version: "1.0.0",
}

func Execute() {
	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}

func init() {
	rootCmd.Flags().BoolP("version", "v", false, "Show version information")
}
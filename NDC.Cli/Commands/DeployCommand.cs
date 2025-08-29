using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDC.Cli.Services;
using Spectre.Console;

namespace NDC.Cli.Commands;

public class DeployCommand : Command
{
    private readonly IServiceProvider _serviceProvider;
    
    public DeployCommand(IServiceProvider serviceProvider) 
        : base("deploy", "Deploy your application to the cloud")
    {
        _serviceProvider = serviceProvider;
        
        var platformOption = new Option<string>(
            name: "--platform",
            description: "Deployment platform (aws, gcp, azure, docker, k8s)")
        { IsRequired = true };
        AddOption(platformOption);
        
        var projectPathOption = new Option<string>(
            name: "--project",
            description: "Path to project directory",
            getDefaultValue: () => Directory.GetCurrentDirectory());
        AddOption(projectPathOption);
        
        var environmentOption = new Option<string>(
            name: "--environment",
            description: "Deployment environment",
            getDefaultValue: () => "development");
        AddOption(environmentOption);
        
        var buildOption = new Option<bool>(
            name: "--build",
            description: "Build and push container image",
            getDefaultValue: () => true);
        AddOption(buildOption);
        
        var infraOption = new Option<bool>(
            name: "--infrastructure",
            description: "Deploy infrastructure with Terraform",
            getDefaultValue: () => true);
        AddOption(infraOption);
        
        var dryRunOption = new Option<bool>(
            name: "--dry-run",
            description: "Show what would be deployed without actually deploying");
        AddOption(dryRunOption);
        
        this.SetHandler(HandleAsync, platformOption, projectPathOption, environmentOption, buildOption, infraOption, dryRunOption);
    }
    
    private async Task<int> HandleAsync(string platform, string projectPath, string environment, bool build, bool infrastructure, bool dryRun)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<DeployCommand>>();
        var deployService = _serviceProvider.GetRequiredService<IDeploymentService>();
        
        try
        {
            AnsiConsole.MarkupLine($"[blue]🚀 Deploying to {platform.ToUpperInvariant()}...[/]");
            
            if (dryRun)
            {
                AnsiConsole.MarkupLine("[yellow]🔍 DRY RUN - No actual deployment will occur[/]");
            }
            
            var deploymentConfig = new DeploymentConfiguration
            {
                Platform = platform,
                ProjectPath = projectPath,
                Environment = environment,
                BuildContainer = build,
                DeployInfrastructure = infrastructure,
                DryRun = dryRun
            };
            
            await AnsiConsole.Status()
                .StartAsync("Preparing deployment...", async ctx =>
                {
                    // Validate project structure
                    ctx.Status("Validating project structure...");
                    var validation = await deployService.ValidateProjectAsync(projectPath);
                    if (!validation.IsValid)
                    {
                        AnsiConsole.MarkupLine($"[red]❌ Project validation failed: {validation.ErrorMessage}[/]");
                        return;
                    }
                    
                    // Read project configuration
                    ctx.Status("Reading project configuration...");
                    var projectConfig = await deployService.ReadProjectConfigurationAsync(projectPath);
                    
                    AnsiConsole.MarkupLine($"[green]📋 Project: {projectConfig.Name}[/]");
                    AnsiConsole.MarkupLine($"[green]🎯 Target: {platform} ({environment})[/]");
                    
                    if (projectConfig.EnabledServices.Any())
                    {
                        AnsiConsole.MarkupLine("[yellow]📦 Enabled services:[/]");
                        foreach (var service in projectConfig.EnabledServices)
                        {
                            AnsiConsole.MarkupLine($"  • [cyan]{service}[/]");
                        }
                    }
                    
                    if (dryRun)
                    {
                        ctx.Status("Planning deployment (dry run)...");
                        var plan = await deployService.PlanDeploymentAsync(deploymentConfig, projectConfig);
                        ShowDeploymentPlan(plan);
                        return;
                    }
                    
                    // Build container if requested
                    if (build)
                    {
                        ctx.Status("Building container image...");
                        var buildResult = await deployService.BuildContainerAsync(projectPath, projectConfig);
                        if (!buildResult.Success)
                        {
                            AnsiConsole.MarkupLine($"[red]❌ Container build failed: {buildResult.ErrorMessage}[/]");
                            return;
                        }
                        AnsiConsole.MarkupLine($"[green]✅ Container built: {buildResult.ImageTag}[/]");
                    }
                    
                    // Deploy infrastructure if requested
                    if (infrastructure)
                    {
                        ctx.Status("Deploying infrastructure...");
                        var infraResult = await deployService.DeployInfrastructureAsync(deploymentConfig, projectConfig);
                        if (!infraResult.Success)
                        {
                            AnsiConsole.MarkupLine($"[red]❌ Infrastructure deployment failed: {infraResult.ErrorMessage}[/]");
                            return;
                        }
                        AnsiConsole.MarkupLine("[green]✅ Infrastructure deployed[/]");
                    }
                    
                    // Deploy application
                    ctx.Status("Deploying application...");
                    var deployResult = await deployService.DeployApplicationAsync(deploymentConfig, projectConfig);
                    if (!deployResult.Success)
                    {
                        AnsiConsole.MarkupLine($"[red]❌ Application deployment failed: {deployResult.ErrorMessage}[/]");
                        return;
                    }
                    
                    AnsiConsole.MarkupLine("[green]✅ Application deployed successfully![/]");
                    
                    if (!string.IsNullOrEmpty(deployResult.ServiceUrl))
                    {
                        AnsiConsole.MarkupLine($"[blue]🌐 Service URL: {deployResult.ServiceUrl}[/]");
                    }
                });
            
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during deployment");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
    
    private void ShowDeploymentPlan(DeploymentPlan plan)
    {
        AnsiConsole.MarkupLine("[yellow]📋 Deployment Plan:[/]");
        AnsiConsole.WriteLine();
        
        if (plan.ContainerActions.Any())
        {
            AnsiConsole.MarkupLine("[blue]🐳 Container Actions:[/]");
            foreach (var action in plan.ContainerActions)
            {
                AnsiConsole.MarkupLine($"  • {action}");
            }
            AnsiConsole.WriteLine();
        }
        
        if (plan.InfrastructureActions.Any())
        {
            AnsiConsole.MarkupLine("[blue]☁️ Infrastructure Actions:[/]");
            foreach (var action in plan.InfrastructureActions)
            {
                AnsiConsole.MarkupLine($"  • {action}");
            }
            AnsiConsole.WriteLine();
        }
        
        if (plan.ApplicationActions.Any())
        {
            AnsiConsole.MarkupLine("[blue]🎯 Application Actions:[/]");
            foreach (var action in plan.ApplicationActions)
            {
                AnsiConsole.MarkupLine($"  • {action}");
            }
        }
    }
}
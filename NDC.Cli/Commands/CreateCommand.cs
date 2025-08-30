using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDC.Cli.Models;
using NDC.Cli.Services;
using Spectre.Console;

namespace NDC.Cli.Commands;

public class CreateCommand : Command
{
    private readonly IServiceProvider _serviceProvider;
    
    public CreateCommand(IServiceProvider serviceProvider) 
        : base("create", "Create a new project from a template")
    {
        _serviceProvider = serviceProvider;
        
        // Template argument
        var templateArgument = new Argument<string>(
            name: "template",
            description: "Template to use (aws, gcp, azure, container, aspire-aws, aspire-gcp, aspire-azure)");
        AddArgument(templateArgument);
        
        // Required options
        var nameOption = new Option<string>(
            name: "--name",
            description: "Project name")
        { IsRequired = true };
        nameOption.AddAlias("-n");
        AddOption(nameOption);
        
        // Optional options
        var outputOption = new Option<string>(
            name: "--output",
            description: "Output directory",
            getDefaultValue: () => Directory.GetCurrentDirectory());
        outputOption.AddAlias("-o");
        AddOption(outputOption);
        
        var frameworkOption = new Option<string>(
            name: "--framework",
            description: ".NET target framework",
            getDefaultValue: () => "net9.0");
        frameworkOption.AddAlias("-f");
        AddOption(frameworkOption);
        
        var portOption = new Option<int>(
            name: "--port",
            description: "Application port",
            getDefaultValue: () => 8080);
        portOption.AddAlias("-p");
        AddOption(portOption);
        
        // Cloud-specific options
        var minInstancesOption = new Option<int>(
            name: "--min-instances",
            description: "Minimum instances",
            getDefaultValue: () => 1);
        AddOption(minInstancesOption);
        
        var maxInstancesOption = new Option<int>(
            name: "--max-instances", 
            description: "Maximum instances",
            getDefaultValue: () => 5);
        AddOption(maxInstancesOption);
        
        // Aspire-specific options
        var databaseOption = new Option<string>(
            name: "--database",
            description: "Database type (PostgreSQL, MySQL, SqlServer)");
        AddOption(databaseOption);
        
        var servicesOption = new Option<string>(
            name: "--services",
            description: "Services to include (comma-separated): database,cache,storage,mail,queue,jobs,worker,all");
        AddOption(servicesOption);
        
        var cacheOption = new Option<bool>(
            name: "--cache",
            description: "Include Redis cache");
        AddOption(cacheOption);
        
        var storageOption = new Option<bool>(
            name: "--storage", 
            description: "Include S3-compatible storage");
        AddOption(storageOption);
        
        var mailOption = new Option<bool>(
            name: "--mail",
            description: "Include email service");
        AddOption(mailOption);
        
        var queueOption = new Option<bool>(
            name: "--queue",
            description: "Include message queue");
        AddOption(queueOption);
        
        var jobsOption = new Option<bool>(
            name: "--jobs",
            description: "Include background jobs");
        AddOption(jobsOption);
        
        var workerOption = new Option<bool>(
            name: "--worker",
            description: "Include worker service");
        AddOption(workerOption);
        
        // Set handler
        this.SetHandler((InvocationContext context) => HandleAsync(
            context.ParseResult.GetValueForArgument(templateArgument),
            context.ParseResult.GetValueForOption(nameOption)!,
            context.ParseResult.GetValueForOption(outputOption)!,
            context.ParseResult.GetValueForOption(frameworkOption)!,
            context.ParseResult.GetValueForOption(portOption),
            context.ParseResult.GetValueForOption(minInstancesOption),
            context.ParseResult.GetValueForOption(maxInstancesOption),
            context.ParseResult.GetValueForOption(databaseOption),
            context.ParseResult.GetValueForOption(servicesOption),
            context.ParseResult.GetValueForOption(cacheOption),
            context.ParseResult.GetValueForOption(storageOption),
            context.ParseResult.GetValueForOption(mailOption),
            context.ParseResult.GetValueForOption(queueOption),
            context.ParseResult.GetValueForOption(jobsOption),
            context.ParseResult.GetValueForOption(workerOption)));
    }
    
    private async Task<int> HandleAsync(
        string template, string name, string output, string framework, int port,
        int minInstances, int maxInstances, string? database, string? services,
        bool cache, bool storage, bool mail, bool queue, bool jobs, bool worker)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateCommand>>();
        var templateService = _serviceProvider.GetRequiredService<ITemplateService>();
        var aspireService = _serviceProvider.GetRequiredService<IAspireService>();
        
        try
        {
            AnsiConsole.MarkupLine($"[green]Creating project '{name}' using template '{template}'...[/]");
            
            // Parse services
            var serviceConfig = ParseServices(services, cache, storage, mail, queue, jobs, worker);
            
            // Create project configuration
            var config = new ProjectConfiguration
            {
                Name = name,
                Template = template,
                OutputDirectory = output,
                Framework = framework,
                Port = port,
                MinInstances = minInstances,
                MaxInstances = maxInstances,
                Database = database,
                Services = serviceConfig
            };
            
            // Validate template exists
            if (!await templateService.TemplateExistsAsync(template))
            {
                AnsiConsole.MarkupLine($"[red]Template '{template}' not found. Use 'ndc list' to see available templates.[/]");
                return 1;
            }
            
            // Create the project
            var result = await templateService.CreateProjectAsync(config);
            
            if (result.Success)
            {
                AnsiConsole.MarkupLine($"[green]✅ Successfully created project '{name}'![/]");
                AnsiConsole.MarkupLine($"[blue]📁 Location: {Path.Combine(output, name)}[/]");
                
                // Show next steps
                ShowNextSteps(template, name);
                
                return 0;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]❌ Failed to create project: {result.ErrorMessage}[/]");
                return 1;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating project");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
    
    private ServiceConfiguration ParseServices(string? services, bool cache, bool storage, bool mail, bool queue, bool jobs, bool worker)
    {
        var config = new ServiceConfiguration
        {
            IncludeCache = cache,
            IncludeStorage = storage,
            IncludeMail = mail,
            IncludeMessageQueue = queue,
            IncludeJobs = jobs,
            IncludeWorker = worker
        };
        
        if (!string.IsNullOrEmpty(services))
        {
            var serviceList = services.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLowerInvariant());
            
            foreach (var service in serviceList)
            {
                switch (service)
                {
                    case "all":
                        config.IncludeCache = true;
                        config.IncludeStorage = true;
                        config.IncludeMail = true;
                        config.IncludeMessageQueue = true;
                        config.IncludeJobs = true;
                        config.IncludeWorker = true;
                        break;
                    case "cache":
                        config.IncludeCache = true;
                        break;
                    case "storage":
                        config.IncludeStorage = true;
                        break;
                    case "mail":
                        config.IncludeMail = true;
                        break;
                    case "queue":
                        config.IncludeMessageQueue = true;
                        break;
                    case "jobs":
                        config.IncludeJobs = true;
                        break;
                    case "worker":
                        config.IncludeWorker = true;
                        break;
                }
            }
        }
        
        return config;
    }
    
    private void ShowNextSteps(string template, string projectName)
    {
        AnsiConsole.MarkupLine("[yellow]🚀 Next steps:[/]");
        AnsiConsole.MarkupLine($"   cd {projectName}");
        
        if (template.StartsWith("aspire-"))
        {
            AnsiConsole.MarkupLine("   [dim]# Start the Aspire AppHost for local development[/]");
            AnsiConsole.MarkupLine($"   dotnet run --project src/{projectName}.AppHost");
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("   [dim]# Or deploy to cloud[/]");
        }
        
        var cloudProvider = template.Contains("aws") ? "AWS" : 
                          template.Contains("gcp") ? "Google Cloud" : 
                          template.Contains("azure") ? "Azure" : 
                          template.Contains("container") ? "Container Platform" : "cloud";
        
        AnsiConsole.MarkupLine($"   [dim]# Configure {cloudProvider} credentials[/]");
        AnsiConsole.MarkupLine("   cd terraform");
        AnsiConsole.MarkupLine("   terraform init");
        AnsiConsole.MarkupLine("   terraform plan");
        AnsiConsole.MarkupLine("   terraform apply");
    }
}
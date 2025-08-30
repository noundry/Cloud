using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDC.Cli.Services;
using Spectre.Console;

namespace NDC.Cli.Commands;

public class ListCommand : Command
{
    private readonly IServiceProvider _serviceProvider;
    
    public ListCommand(IServiceProvider serviceProvider) 
        : base("list", "List available templates")
    {
        _serviceProvider = serviceProvider;
        
        var allOption = new Option<bool>(
            name: "--all",
            description: "Show all templates including those not installed");
        AddOption(allOption);
        
        this.SetHandler(HandleAsync, allOption);
    }
    
    private async Task<int> HandleAsync(bool showAll)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<ListCommand>>();
        var templateService = _serviceProvider.GetRequiredService<ITemplateService>();
        
        try
        {
            AnsiConsole.MarkupLine("[blue]Available NDC Templates:[/]");
            AnsiConsole.WriteLine();
            
            var templates = await templateService.GetAvailableTemplatesAsync(showAll);
            
            if (!templates.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No templates found. Use 'ndc install' to install template packages.[/]");
                AnsiConsole.MarkupLine("[dim]Example: ndc install NDC.Templates.WebApp[/]");
                return 0;
            }
            
            // Create table
            var table = new Table();
            table.AddColumn("[bold]Template[/]");
            table.AddColumn("[bold]Cloud[/]");
            table.AddColumn("[bold]Type[/]");
            table.AddColumn("[bold]Description[/]");
            table.AddColumn("[bold]Status[/]");
            
            foreach (var template in templates.OrderBy(t => t.CloudProvider).ThenBy(t => t.Name))
            {
                var status = template.IsInstalled ? "[green]Installed[/]" : "[yellow]Available[/]";
                var type = template.IsAspire ? "Aspire" : "Simple";
                
                table.AddRow(
                    $"[cyan]{template.ShortName}[/]",
                    GetCloudIcon(template.CloudProvider),
                    type,
                    template.Description,
                    status
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Show usage examples
            AnsiConsole.MarkupLine("[yellow]Usage Examples:[/]");
            AnsiConsole.MarkupLine("  [dim]# Use working examples (current)[/]");
            AnsiConsole.MarkupLine("  cp -r examples/working-aws-template my-api");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  [dim]# Future CLI commands[/]");
            AnsiConsole.MarkupLine("  ndc create webapp-aws --name my-app --services database,cache,storage");
            
            if (!showAll)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Use --all to see all available templates including those not installed.[/]");
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing templates");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
    
    private static string GetCloudIcon(string cloudProvider)
    {
        return cloudProvider.ToLowerInvariant() switch
        {
            "aws" => "☁️ AWS",
            "gcp" or "google" => "🌐 Google Cloud",
            "azure" => "🔷 Azure",
            _ => cloudProvider
        };
    }
}
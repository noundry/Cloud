using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDC.Cli.Services;
using Spectre.Console;

namespace NDC.Cli.Commands;

public class InstallCommand : Command
{
    private readonly IServiceProvider _serviceProvider;
    
    public InstallCommand(IServiceProvider serviceProvider) 
        : base("install", "Install template packages")
    {
        _serviceProvider = serviceProvider;
        
        var packageArgument = new Argument<string>(
            name: "package",
            description: "Template package to install (e.g., NDC.Templates.Aspire.Aws)");
        AddArgument(packageArgument);
        
        var versionOption = new Option<string>(
            name: "--version",
            description: "Specific version to install");
        AddOption(versionOption);
        
        var prereleaseOption = new Option<bool>(
            name: "--prerelease",
            description: "Include prerelease versions");
        AddOption(prereleaseOption);
        
        this.SetHandler(HandleAsync, packageArgument, versionOption, prereleaseOption);
    }
    
    private async Task<int> HandleAsync(string packageName, string? version, bool includePrerelease)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<InstallCommand>>();
        var nugetService = _serviceProvider.GetRequiredService<INuGetService>();
        
        try
        {
            AnsiConsole.MarkupLine($"[blue]Installing template package '{packageName}'...[/]");
            
            await AnsiConsole.Status()
                .StartAsync("Installing package...", async ctx =>
                {
                    var result = await nugetService.InstallTemplatePackageAsync(packageName, version, includePrerelease);
                    
                    if (result.Success)
                    {
                        AnsiConsole.MarkupLine($"[green]✅ Successfully installed '{packageName}' v{result.InstalledVersion}[/]");
                        
                        // Show installed templates
                        if (result.InstalledTemplates?.Any() == true)
                        {
                            AnsiConsole.MarkupLine("[yellow]📦 Available templates:[/]");
                            foreach (var template in result.InstalledTemplates)
                            {
                                AnsiConsole.MarkupLine($"  • [cyan]{template}[/]");
                            }
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]❌ Failed to install package: {result.ErrorMessage}[/]");
                    }
                });
            
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error installing package");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}

public class UninstallCommand : Command
{
    private readonly IServiceProvider _serviceProvider;
    
    public UninstallCommand(IServiceProvider serviceProvider) 
        : base("uninstall", "Uninstall template packages")
    {
        _serviceProvider = serviceProvider;
        
        var packageArgument = new Argument<string>(
            name: "package",
            description: "Template package to uninstall");
        AddArgument(packageArgument);
        
        this.SetHandler(HandleAsync, packageArgument);
    }
    
    private async Task<int> HandleAsync(string packageName)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<UninstallCommand>>();
        var nugetService = _serviceProvider.GetRequiredService<INuGetService>();
        
        try
        {
            AnsiConsole.MarkupLine($"[blue]Uninstalling template package '{packageName}'...[/]");
            
            var result = await nugetService.UninstallTemplatePackageAsync(packageName);
            
            if (result.Success)
            {
                AnsiConsole.MarkupLine($"[green]✅ Successfully uninstalled '{packageName}'[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]❌ Failed to uninstall package: {result.ErrorMessage}[/]");
                return 1;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uninstalling package");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}
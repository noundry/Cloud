using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NDC.Cli.Commands;
using NDC.Cli.Services;
using Spectre.Console;

namespace NDC.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Create host for dependency injection
        var host = CreateHost();
        
        // Create root command
        var rootCommand = new RootCommand("NDC (Noundry Deploy CLI) - Generate cloud-native .NET applications with Aspire");
        
        // Get services
        var serviceProvider = host.Services;
        
        // Add commands
        rootCommand.AddCommand(new CreateCommand(serviceProvider));
        rootCommand.AddCommand(new ListCommand(serviceProvider));
        rootCommand.AddCommand(new InstallCommand(serviceProvider));
        rootCommand.AddCommand(new UninstallCommand(serviceProvider));
        
        // Add global options
        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose logging");
        rootCommand.AddGlobalOption(verboseOption);
        
        // Configure logging based on verbose option
        rootCommand.SetHandler((bool verbose) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            if (verbose)
            {
                logger.LogInformation("Verbose logging enabled");
            }
        }, verboseOption);

        try
        {
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
    
    private static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<ITemplateService, TemplateService>();
                services.AddSingleton<IAspireService, AspireService>();
                services.AddSingleton<ICloudService, CloudService>();
                services.AddSingleton<INuGetService, NuGetService>();
                
                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            })
            .Build();
    }
}
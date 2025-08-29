using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NDC.Cli.Models;

namespace NDC.Cli.Services;

public class TemplateService : ITemplateService
{
    private readonly ILogger<TemplateService> _logger;
    private readonly IAspireService _aspireService;
    private readonly ICloudService _cloudService;

    public TemplateService(
        ILogger<TemplateService> logger,
        IAspireService aspireService,
        ICloudService cloudService)
    {
        _logger = logger;
        _aspireService = aspireService;
        _cloudService = cloudService;
    }

    public async Task<bool> TemplateExistsAsync(string templateName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new {templateName} --dry-run",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if template exists: {TemplateName}", templateName);
            return false;
        }
    }

    public async Task<IEnumerable<TemplateInfo>> GetAvailableTemplatesAsync(bool includeNotInstalled = false)
    {
        var templates = new List<TemplateInfo>();

        // Get installed templates
        var installedTemplates = await GetInstalledTemplatesAsync();
        templates.AddRange(installedTemplates);

        if (includeNotInstalled)
        {
            // Add known template packages that might not be installed
            var knownTemplates = GetKnownTemplatePackages();
            foreach (var known in knownTemplates)
            {
                if (!templates.Any(t => t.ShortName == known.ShortName))
                {
                    known.IsInstalled = false;
                    templates.Add(known);
                }
            }
        }

        return templates;
    }

    public async Task<ProjectCreationResult> CreateProjectAsync(ProjectConfiguration configuration)
    {
        try
        {
            _logger.LogInformation("Creating project {ProjectName} with template {Template}", 
                configuration.Name, configuration.Template);

            var outputPath = Path.Combine(configuration.OutputDirectory, configuration.Name);

            // Build dotnet new arguments
            var args = BuildDotnetNewArguments(configuration);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = args,
                WorkingDirectory = configuration.OutputDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new ProjectCreationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to start dotnet process"
                };
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return new ProjectCreationResult
                {
                    Success = false,
                    ErrorMessage = $"dotnet new failed: {error}"
                };
            }

            // Post-process for Aspire templates
            if (configuration.Template.StartsWith("aspire-"))
            {
                await PostProcessAspireProject(outputPath, configuration);
            }

            // Generate cloud infrastructure
            var cloudProvider = ExtractCloudProvider(configuration.Template);
            if (!string.IsNullOrEmpty(cloudProvider))
            {
                await _cloudService.GenerateCloudInfrastructureAsync(outputPath, cloudProvider, configuration.Services);
            }

            return new ProjectCreationResult
            {
                Success = true,
                ProjectPath = outputPath,
                GeneratedFiles = GetGeneratedFiles(outputPath)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return new ProjectCreationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TemplateInfo?> GetTemplateInfoAsync(string templateName)
    {
        var templates = await GetAvailableTemplatesAsync(true);
        return templates.FirstOrDefault(t => t.ShortName == templateName);
    }

    private async Task<List<TemplateInfo>> GetInstalledTemplatesAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "new list",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return new List<TemplateInfo>();

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0) return new List<TemplateInfo>();

            return ParseDotnetNewListOutput(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting installed templates");
            return new List<TemplateInfo>();
        }
    }

    private List<TemplateInfo> ParseDotnetNewListOutput(string output)
    {
        var templates = new List<TemplateInfo>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            if (line.Contains("aspire-webapp-") || line.Contains("aspire-fullstack-") || line.Contains("dotnet-webapp-"))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var shortName = parts[^1]; // Last part is typically the short name
                    var name = string.Join(" ", parts[..^1]); // Everything before last part
                    
                    templates.Add(new TemplateInfo
                    {
                        Name = name.Trim(),
                        ShortName = shortName.Trim(),
                        Description = GetTemplateDescription(shortName),
                        CloudProvider = ExtractCloudProvider(shortName),
                        IsAspire = shortName.StartsWith("aspire-"),
                        IsInstalled = true
                    });
                }
            }
        }

        return templates;
    }

    private List<TemplateInfo> GetKnownTemplatePackages()
    {
        return new List<TemplateInfo>
        {
            // Simple templates
            new() { ShortName = "dotnet-webapp-aws", Name = ".NET Web App for AWS", Description = "Simple .NET web application for AWS App Runner", CloudProvider = "AWS", IsAspire = false },
            new() { ShortName = "dotnet-webapp-gcp", Name = ".NET Web App for Google Cloud", Description = "Simple .NET web application for Google Cloud Run", CloudProvider = "GCP", IsAspire = false },
            new() { ShortName = "dotnet-webapp-azure", Name = ".NET Web App for Azure", Description = "Simple .NET web application for Azure Container Apps", CloudProvider = "Azure", IsAspire = false },
            
            // Aspire templates
            new() { ShortName = "aspire-webapp-aws", Name = "Aspire Web App for AWS", Description = "Aspire web application with service discovery for AWS", CloudProvider = "AWS", IsAspire = true },
            new() { ShortName = "aspire-webapp-gcp", Name = "Aspire Web App for Google Cloud", Description = "Aspire web application with service discovery for Google Cloud", CloudProvider = "GCP", IsAspire = true },
            new() { ShortName = "aspire-webapp-azure", Name = "Aspire Web App for Azure", Description = "Aspire web application with service discovery for Azure", CloudProvider = "Azure", IsAspire = true },
            
            // Full-stack templates
            new() { ShortName = "aspire-fullstack-aws", Name = "Aspire Full-Stack App for AWS", Description = "Complete Aspire application with all services for AWS", CloudProvider = "AWS", IsAspire = true },
            new() { ShortName = "aspire-fullstack-gcp", Name = "Aspire Full-Stack App for Google Cloud", Description = "Complete Aspire application with all services for Google Cloud", CloudProvider = "GCP", IsAspire = true },
            new() { ShortName = "aspire-fullstack-azure", Name = "Aspire Full-Stack App for Azure", Description = "Complete Aspire application with all services for Azure", CloudProvider = "Azure", IsAspire = true }
        };
    }

    private string GetTemplateDescription(string shortName)
    {
        return shortName switch
        {
            "dotnet-webapp-aws" => "Simple .NET web application for AWS App Runner",
            "dotnet-webapp-gcp" => "Simple .NET web application for Google Cloud Run",
            "dotnet-webapp-azure" => "Simple .NET web application for Azure Container Apps",
            "aspire-webapp-aws" => "Aspire web application with service discovery for AWS",
            "aspire-webapp-gcp" => "Aspire web application with service discovery for Google Cloud",
            "aspire-webapp-azure" => "Aspire web application with service discovery for Azure",
            "aspire-fullstack-aws" => "Complete Aspire application with all services for AWS",
            "aspire-fullstack-gcp" => "Complete Aspire application with all services for Google Cloud",
            "aspire-fullstack-azure" => "Complete Aspire application with all services for Azure",
            _ => "Cloud-native .NET application template"
        };
    }

    private string ExtractCloudProvider(string templateName)
    {
        if (templateName.Contains("-aws")) return "AWS";
        if (templateName.Contains("-gcp")) return "GCP";
        if (templateName.Contains("-azure")) return "Azure";
        return "";
    }

    private string BuildDotnetNewArguments(ProjectConfiguration config)
    {
        var args = new List<string>
        {
            "new",
            config.Template,
            "--name", config.Name,
            "--framework", config.Framework,
            "--force" // Overwrite existing files if they exist
        };

        // Add template-specific parameters
        if (!string.IsNullOrEmpty(config.Database))
        {
            args.AddRange(new[] { "--database", config.Database });
        }

        if (config.Port != 8080)
        {
            args.AddRange(new[] { "--port", config.Port.ToString() });
        }

        // Add service flags
        if (config.Services.IncludeCache)
            args.AddRange(new[] { "--include-cache", "true" });
        
        if (config.Services.IncludeStorage)
            args.AddRange(new[] { "--include-storage", "true" });
        
        if (config.Services.IncludeMail)
            args.AddRange(new[] { "--include-mail", "true" });
        
        if (config.Services.IncludeMessageQueue)
            args.AddRange(new[] { "--include-queue", "true" });
        
        if (config.Services.IncludeJobs)
            args.AddRange(new[] { "--include-jobs", "true" });
        
        if (config.Services.IncludeWorker)
            args.AddRange(new[] { "--include-worker", "true" });

        return string.Join(" ", args);
    }

    private async Task PostProcessAspireProject(string projectPath, ProjectConfiguration configuration)
    {
        // Generate Aspire manifest
        await _aspireService.GenerateAspireManifestAsync(projectPath, configuration.Services);
        
        // Configure service discovery
        await _aspireService.ConfigureServiceDiscoveryAsync(projectPath, configuration.Services);
    }

    private List<string> GetGeneratedFiles(string projectPath)
    {
        var files = new List<string>();
        
        if (Directory.Exists(projectPath))
        {
            files.AddRange(Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(projectPath, f)));
        }

        return files;
    }
}
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
            if (line.Contains("webapp-"))
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
            // Web App Templates (Aspire for local DX, deploy API only)
            new() { ShortName = "webapp-aws", Name = "Web App for AWS App Runner", Description = "Web application for AWS App Runner (Aspire local DX)", CloudProvider = "AWS", IsAspire = true },
            new() { ShortName = "webapp-gcp", Name = "Web App for Google Cloud Run", Description = "Web application for Google Cloud Run (Aspire local DX)", CloudProvider = "GCP", IsAspire = true },
            new() { ShortName = "webapp-azure", Name = "Web App for Azure Container Apps", Description = "Web application for Azure Container Apps (Aspire local DX)", CloudProvider = "Azure", IsAspire = true },
            
            // Container Platform Templates
            new() { ShortName = "webapp-docker", Name = "Web App for Docker", Description = "Web application with Docker Compose deployment", CloudProvider = "Docker", IsAspire = true },
            new() { ShortName = "webapp-k8s", Name = "Web App for Kubernetes", Description = "Web application with Kubernetes manifests", CloudProvider = "Kubernetes", IsAspire = true },
            
            // Platform-specific templates
            new() { ShortName = "webapp-railway", Name = "Web App for Railway", Description = "Web application optimized for Railway deployment", CloudProvider = "Railway", IsAspire = true },
            new() { ShortName = "webapp-render", Name = "Web App for Render", Description = "Web application optimized for Render deployment", CloudProvider = "Render", IsAspire = true },
            new() { ShortName = "webapp-fly", Name = "Web App for Fly.io", Description = "Web application optimized for Fly.io deployment", CloudProvider = "Fly.io", IsAspire = true },
            new() { ShortName = "webapp-heroku", Name = "Web App for Heroku", Description = "Web application optimized for Heroku deployment", CloudProvider = "Heroku", IsAspire = true }
        };
    }

    private string GetTemplateDescription(string shortName)
    {
        return shortName switch
        {
            "webapp-aws" => "Multi-cloud web application for AWS (App Runner + RDS + ElastiCache)",
            "webapp-gcp" => "Multi-cloud web application for Google Cloud (Cloud Run + Cloud SQL + Memorystore)",
            "webapp-azure" => "Multi-cloud web application for Azure (Container Apps + SQL Database + Redis)",
            "webapp-container" => "Multi-cloud web application for containers (Docker Compose + Kubernetes)",
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
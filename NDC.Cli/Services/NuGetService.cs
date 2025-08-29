using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NDC.Cli.Models;

namespace NDC.Cli.Services;

public class NuGetService : INuGetService
{
    private readonly ILogger<NuGetService> _logger;

    public NuGetService(ILogger<NuGetService> logger)
    {
        _logger = logger;
    }

    public async Task<PackageInstallResult> InstallTemplatePackageAsync(string packageName, string? version = null, bool includePrerelease = false)
    {
        try
        {
            var args = new List<string>
            {
                "new", "install", packageName
            };

            if (!string.IsNullOrEmpty(version))
            {
                args.AddRange(new[] { "--version", version });
            }

            if (includePrerelease)
            {
                args.Add("--prerelease");
            }

            var result = await ExecuteDotnetCommandAsync(string.Join(" ", args));

            if (result.Success)
            {
                // Parse installed templates from output
                var installedTemplates = await GetInstalledTemplatesFromPackageAsync(packageName);

                return new PackageInstallResult
                {
                    Success = true,
                    InstalledVersion = version ?? "latest",
                    InstalledTemplates = installedTemplates
                };
            }

            return new PackageInstallResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing template package {PackageName}", packageName);
            return new PackageInstallResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PackageInstallResult> UninstallTemplatePackageAsync(string packageName)
    {
        try
        {
            var result = await ExecuteDotnetCommandAsync($"new uninstall {packageName}");

            return new PackageInstallResult
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uninstalling template package {PackageName}", packageName);
            return new PackageInstallResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<IEnumerable<TemplateInfo>> SearchTemplatePackagesAsync(string? searchTerm = null)
    {
        try
        {
            // For now, return known NDC template packages
            // In a real implementation, this would search NuGet.org
            var knownPackages = new[]
            {
                "NDC.Templates.Simple",
                "NDC.Templates.Aspire.Aws",
                "NDC.Templates.Aspire.Gcp",
                "NDC.Templates.Aspire.Azure"
            };

            var templates = new List<TemplateInfo>();

            foreach (var package in knownPackages)
            {
                if (string.IsNullOrEmpty(searchTerm) || package.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    var isInstalled = await IsPackageInstalledAsync(package);
                    
                    templates.Add(new TemplateInfo
                    {
                        Name = GetPackageDisplayName(package),
                        ShortName = GetShortNameFromPackage(package),
                        Description = GetPackageDescription(package),
                        CloudProvider = GetCloudProviderFromPackage(package),
                        IsAspire = package.Contains("Aspire"),
                        IsInstalled = isInstalled,
                        PackageName = package
                    });
                }
            }

            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching template packages");
            return Enumerable.Empty<TemplateInfo>();
        }
    }

    public async Task<bool> IsPackageInstalledAsync(string packageName)
    {
        try
        {
            var result = await ExecuteDotnetCommandAsync("new list");
            return result.Success && result.Output?.Contains(packageName) == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if package is installed {PackageName}", packageName);
            return false;
        }
    }

    private async Task<CommandResult> ExecuteDotnetCommandAsync(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new CommandResult
                {
                    Success = false,
                    ErrorMessage = "Failed to start dotnet process"
                };
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new CommandResult
            {
                Success = process.ExitCode == 0,
                Output = output,
                ErrorMessage = process.ExitCode != 0 ? error : null
            };
        }
        catch (Exception ex)
        {
            return new CommandResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<List<string>> GetInstalledTemplatesFromPackageAsync(string packageName)
    {
        var templates = new List<string>();

        // Map package names to their templates
        var packageTemplates = packageName switch
        {
            "NDC.Templates.Simple" => new[] { "dotnet-webapp-aws", "dotnet-webapp-gcp", "dotnet-webapp-azure" },
            "NDC.Templates.Aspire.Aws" => new[] { "aspire-webapp-aws", "aspire-fullstack-aws" },
            "NDC.Templates.Aspire.Gcp" => new[] { "aspire-webapp-gcp", "aspire-fullstack-gcp" },
            "NDC.Templates.Aspire.Azure" => new[] { "aspire-webapp-azure", "aspire-fullstack-azure" },
            _ => Array.Empty<string>()
        };

        templates.AddRange(packageTemplates);
        return templates;
    }

    private string GetPackageDisplayName(string packageName)
    {
        return packageName switch
        {
            "NDC.Templates.Simple" => "NDC Simple Templates",
            "NDC.Templates.Aspire.Aws" => "NDC Aspire Templates for AWS",
            "NDC.Templates.Aspire.Gcp" => "NDC Aspire Templates for Google Cloud",
            "NDC.Templates.Aspire.Azure" => "NDC Aspire Templates for Azure",
            _ => packageName
        };
    }

    private string GetShortNameFromPackage(string packageName)
    {
        return packageName switch
        {
            "NDC.Templates.Simple" => "dotnet-webapp-*",
            "NDC.Templates.Aspire.Aws" => "aspire-*-aws",
            "NDC.Templates.Aspire.Gcp" => "aspire-*-gcp",
            "NDC.Templates.Aspire.Azure" => "aspire-*-azure",
            _ => packageName
        };
    }

    private string GetPackageDescription(string packageName)
    {
        return packageName switch
        {
            "NDC.Templates.Simple" => "Simple .NET web application templates for all clouds",
            "NDC.Templates.Aspire.Aws" => "Aspire-enabled templates for AWS deployment",
            "NDC.Templates.Aspire.Gcp" => "Aspire-enabled templates for Google Cloud deployment",
            "NDC.Templates.Aspire.Azure" => "Aspire-enabled templates for Azure deployment",
            _ => "NDC template package"
        };
    }

    private string GetCloudProviderFromPackage(string packageName)
    {
        if (packageName.Contains("Aws")) return "AWS";
        if (packageName.Contains("Gcp")) return "GCP";
        if (packageName.Contains("Azure")) return "Azure";
        return "Multi-Cloud";
    }

    private class CommandResult
    {
        public bool Success { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
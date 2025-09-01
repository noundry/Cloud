using NDC.Cli.Models;

namespace NDC.Cli.Services;

public interface ITemplateService
{
    Task<bool> TemplateExistsAsync(string templateName);
    Task<IEnumerable<TemplateInfo>> GetAvailableTemplatesAsync(bool includeNotInstalled = false);
    Task<ProjectCreationResult> CreateProjectAsync(ProjectConfiguration configuration);
    Task<TemplateInfo?> GetTemplateInfoAsync(string templateName);
}

public interface IAspireService
{
    Task<IEnumerable<CloudServiceMapping>> GetServiceMappingsAsync(string cloudProvider, ServiceConfiguration services);
    Task GenerateAspireManifestAsync(string projectPath, ServiceConfiguration services);
    Task ConfigureServiceDiscoveryAsync(string projectPath, ServiceConfiguration services);
}

public interface ICloudService
{
    string GetDefaultDatabase(string cloudProvider);
    string GetDefaultRegion(string cloudProvider);
    Task<Dictionary<string, object>> GetCloudDefaultsAsync(string cloudProvider, string serviceType);
    Task GenerateCloudInfrastructureAsync(string projectPath, string cloudProvider, ServiceConfiguration services);
}

public interface INuGetService
{
    Task<PackageInstallResult> InstallTemplatePackageAsync(string packageName, string? version = null, bool includePrerelease = false);
    Task<PackageInstallResult> UninstallTemplatePackageAsync(string packageName);
    Task<IEnumerable<TemplateInfo>> SearchTemplatePackagesAsync(string? searchTerm = null);
    Task<bool> IsPackageInstalledAsync(string packageName);
}
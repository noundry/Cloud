using NDC.Cli.Models;

namespace NDC.Cli.Services;

public interface IDeploymentService
{
    Task<ProjectValidationResult> ValidateProjectAsync(string projectPath);
    Task<ProjectConfiguration> ReadProjectConfigurationAsync(string projectPath);
    Task<DeploymentPlan> PlanDeploymentAsync(DeploymentConfiguration deployment, ProjectConfiguration project);
    Task<ContainerBuildResult> BuildContainerAsync(string projectPath, ProjectConfiguration project);
    Task<InfrastructureDeployResult> DeployInfrastructureAsync(DeploymentConfiguration deployment, ProjectConfiguration project);
    Task<ApplicationDeployResult> DeployApplicationAsync(DeploymentConfiguration deployment, ProjectConfiguration project);
}

public class DeploymentConfiguration
{
    public string Platform { get; set; } = "";
    public string ProjectPath { get; set; } = "";
    public string Environment { get; set; } = "development";
    public bool BuildContainer { get; set; } = true;
    public bool DeployInfrastructure { get; set; } = true;
    public bool DryRun { get; set; } = false;
}

public class ProjectValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class DeploymentPlan
{
    public List<string> ContainerActions { get; set; } = new();
    public List<string> InfrastructureActions { get; set; } = new();
    public List<string> ApplicationActions { get; set; } = new();
}

public class ContainerBuildResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ImageTag { get; set; }
    public string? RegistryUrl { get; set; }
}

public class InfrastructureDeployResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Outputs { get; set; } = new();
}

public class ApplicationDeployResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ServiceUrl { get; set; }
    public Dictionary<string, string> Endpoints { get; set; } = new();
}
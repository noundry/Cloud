namespace NDC.Cli.Models;

public class ProjectConfiguration
{
    public string Name { get; set; } = "";
    public string Template { get; set; } = "";
    public string OutputDirectory { get; set; } = "";
    public string Framework { get; set; } = "net9.0";
    public int Port { get; set; } = 8080;
    public int MinInstances { get; set; } = 1;
    public int MaxInstances { get; set; } = 5;
    public string? Database { get; set; }
    public ServiceConfiguration Services { get; set; } = new();
}

public class ServiceConfiguration
{
    public bool IncludeCache { get; set; }
    public bool IncludeStorage { get; set; }
    public bool IncludeMail { get; set; }
    public bool IncludeMessageQueue { get; set; }
    public bool IncludeJobs { get; set; }
    public bool IncludeWorker { get; set; }
    
    public bool HasAnyService => 
        IncludeCache || IncludeStorage || IncludeMail || 
        IncludeMessageQueue || IncludeJobs || IncludeWorker;
}

public class TemplateInfo
{
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Description { get; set; } = "";
    public string CloudProvider { get; set; } = "";
    public bool IsAspire { get; set; }
    public bool IsInstalled { get; set; }
    public string? PackageName { get; set; }
    public string? Version { get; set; }
}

public class ProjectCreationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProjectPath { get; set; }
    public List<string> GeneratedFiles { get; set; } = new();
}

public class PackageInstallResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? InstalledVersion { get; set; }
    public List<string>? InstalledTemplates { get; set; }
}

public class CloudServiceMapping
{
    public string LocalService { get; set; } = "";
    public string CloudService { get; set; } = "";
    public Dictionary<string, object> Configuration { get; set; } = new();
}
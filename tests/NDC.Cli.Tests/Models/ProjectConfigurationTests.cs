namespace NDC.Cli.Tests.Models;

[TestFixture]
public class ProjectConfigurationTests
{
    [Test]
    public void ProjectConfiguration_DefaultValues_AreSetCorrectly()
    {
        // Act
        var config = new ProjectConfiguration();

        // Assert
        Assert.That(config.Framework, Is.EqualTo("net9.0"));
        Assert.That(config.Port, Is.EqualTo(8080));
        Assert.That(config.MinInstances, Is.EqualTo(1));
        Assert.That(config.MaxInstances, Is.EqualTo(5));
        Assert.That(config.Services, Is.Not.Null);
    }

    [Test]
    public void ProjectConfiguration_WithAllProperties_SetsValuesCorrectly()
    {
        // Arrange
        var services = new ServiceConfiguration
        {
            IncludeCache = true,
            IncludeStorage = true,
            IncludeMail = false,
            IncludeMessageQueue = false,
            IncludeJobs = true,
            IncludeWorker = false
        };

        // Act
        var config = new ProjectConfiguration
        {
            Name = "TestProject",
            Template = "webapp-aws",
            OutputDirectory = "/test/path",
            Framework = "net9.0",
            Port = 8080,
            MinInstances = 1,
            MaxInstances = 5,
            Database = "PostgreSQL",
            Services = services
        };

        // Assert
        Assert.That(config.Name, Is.EqualTo("TestProject"));
        Assert.That(config.Template, Is.EqualTo("webapp-aws"));
        Assert.That(config.OutputDirectory, Is.EqualTo("/test/path"));
        Assert.That(config.Framework, Is.EqualTo("net9.0"));
        Assert.That(config.Port, Is.EqualTo(8080));
        Assert.That(config.MinInstances, Is.EqualTo(1));
        Assert.That(config.MaxInstances, Is.EqualTo(5));
        Assert.That(config.Database, Is.EqualTo("PostgreSQL"));
        Assert.That(config.Services, Is.EqualTo(services));
    }
}

[TestFixture]
public class ServiceConfigurationTests
{
    [Test]
    public void ServiceConfiguration_DefaultValues_AreFalse()
    {
        // Act
        var config = new ServiceConfiguration();

        // Assert
        Assert.That(config.IncludeCache, Is.False);
        Assert.That(config.IncludeStorage, Is.False);
        Assert.That(config.IncludeMail, Is.False);
        Assert.That(config.IncludeMessageQueue, Is.False);
        Assert.That(config.IncludeJobs, Is.False);
        Assert.That(config.IncludeWorker, Is.False);
        Assert.That(config.HasAnyService, Is.False);
    }

    [Test]
    public void ServiceConfiguration_WithAllServicesEnabled_SetsCorrectly()
    {
        // Act
        var config = new ServiceConfiguration
        {
            IncludeCache = true,
            IncludeStorage = true,
            IncludeMail = true,
            IncludeMessageQueue = true,
            IncludeJobs = true,
            IncludeWorker = true
        };

        // Assert
        Assert.That(config.IncludeCache, Is.True);
        Assert.That(config.IncludeStorage, Is.True);
        Assert.That(config.IncludeMail, Is.True);
        Assert.That(config.IncludeMessageQueue, Is.True);
        Assert.That(config.IncludeJobs, Is.True);
        Assert.That(config.IncludeWorker, Is.True);
        Assert.That(config.HasAnyService, Is.True);
    }
}

[TestFixture]
public class TemplateInfoTests
{
    [Test]
    public void TemplateInfo_DefaultValues_AreSetCorrectly()
    {
        // Act
        var template = new TemplateInfo();

        // Assert
        Assert.That(template.IsInstalled, Is.False);
        Assert.That(template.Name, Is.EqualTo(""));
        Assert.That(template.ShortName, Is.EqualTo(""));
        Assert.That(template.Description, Is.EqualTo(""));
        Assert.That(template.CloudProvider, Is.EqualTo(""));
        Assert.That(template.IsAspire, Is.False);
    }

    [Test]
    public void TemplateInfo_WithAllProperties_SetsCorrectly()
    {
        // Act
        var template = new TemplateInfo
        {
            Name = "NDC Web App for AWS",
            ShortName = "webapp-aws",
            Description = "Multi-cloud web application template for AWS",
            PackageName = "NDC.Templates.WebApp",
            Version = "1.0.0",
            IsInstalled = true,
            CloudProvider = "AWS",
            IsAspire = true
        };

        // Assert
        Assert.That(template.Name, Is.EqualTo("NDC Web App for AWS"));
        Assert.That(template.ShortName, Is.EqualTo("webapp-aws"));
        Assert.That(template.Description, Is.EqualTo("Multi-cloud web application template for AWS"));
        Assert.That(template.PackageName, Is.EqualTo("NDC.Templates.WebApp"));
        Assert.That(template.Version, Is.EqualTo("1.0.0"));
        Assert.That(template.IsInstalled, Is.True);
        Assert.That(template.CloudProvider, Is.EqualTo("AWS"));
        Assert.That(template.IsAspire, Is.True);
    }
}

[TestFixture]
public class ProjectCreationResultTests
{
    [Test]
    public void ProjectCreationResult_SuccessResult_HasCorrectProperties()
    {
        // Act
        var result = new ProjectCreationResult { Success = true };

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.GeneratedFiles, Is.Not.Null);
        Assert.That(result.GeneratedFiles, Is.Empty);
    }

    [Test]
    public void ProjectCreationResult_FailureResult_HasCorrectProperties()
    {
        // Arrange
        var errorMessage = "Template not found";

        // Act
        var result = new ProjectCreationResult 
        { 
            Success = false, 
            ErrorMessage = errorMessage 
        };

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
    }
}
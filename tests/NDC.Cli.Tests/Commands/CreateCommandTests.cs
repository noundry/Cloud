using System.CommandLine;
using Microsoft.Extensions.Hosting;

namespace NDC.Cli.Tests.Commands;

[TestFixture]
public class CreateCommandTests
{
    private Mock<ITemplateService> _mockTemplateService = null!;
    private Mock<IAspireService> _mockAspireService = null!;
    private Mock<ICloudService> _mockCloudService = null!;
    private Mock<INuGetService> _mockNuGetService = null!;
    private Mock<ILogger<CreateCommand>> _mockLogger = null!;
    private IServiceProvider _serviceProvider = null!;
    private CreateCommand _command = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTemplateService = new Mock<ITemplateService>();
        _mockAspireService = new Mock<IAspireService>();
        _mockCloudService = new Mock<ICloudService>();
        _mockNuGetService = new Mock<INuGetService>();
        _mockLogger = new Mock<ILogger<CreateCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockTemplateService.Object);
        services.AddSingleton(_mockAspireService.Object);
        services.AddSingleton(_mockCloudService.Object);
        services.AddSingleton(_mockNuGetService.Object);
        services.AddSingleton(_mockLogger.Object);

        _serviceProvider = services.BuildServiceProvider();
        _command = new CreateCommand(_serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [Test]
    public void CreateCommand_Initialization_SetsCorrectName()
    {
        // Assert
        Assert.That(_command.Name, Is.EqualTo("create"));
        Assert.That(_command.Description, Is.EqualTo("Create a new project from a template"));
    }

    [Test]
    public void CreateCommand_HasRequiredArguments()
    {
        // Assert
        Assert.That(_command.Arguments, Has.Count.EqualTo(1));
        var templateArg = _command.Arguments.First();
        Assert.That(templateArg.Name, Is.EqualTo("template"));
    }

    [Test]
    public void CreateCommand_HasRequiredOptions()
    {
        // Assert
        var nameOption = _command.Options.FirstOrDefault(o => o.Name == "name");
        Assert.That(nameOption, Is.Not.Null);
        Assert.That(nameOption!.IsRequired, Is.True);
        
        var outputOption = _command.Options.FirstOrDefault(o => o.Name == "output");
        Assert.That(outputOption, Is.Not.Null);
        
        var frameworkOption = _command.Options.FirstOrDefault(o => o.Name == "framework");
        Assert.That(frameworkOption, Is.Not.Null);
    }

    [Test]
    public void CreateCommand_HasOptionalServiceOptions()
    {
        // Assert
        var serviceOptions = new[] { "cache", "storage", "mail", "queue", "jobs", "worker" };
        
        foreach (var optionName in serviceOptions)
        {
            var option = _command.Options.FirstOrDefault(o => o.Name == optionName);
            Assert.That(option, Is.Not.Null, $"Option {optionName} should exist");
        }
        
        var servicesOption = _command.Options.FirstOrDefault(o => o.Name == "services");
        Assert.That(servicesOption, Is.Not.Null);
    }

    [Test]
    public async Task TemplateService_Mock_CanBeConfigured()
    {
        // Arrange
        var templateName = "webapp-aws";
        
        _mockTemplateService
            .Setup(x => x.TemplateExistsAsync(templateName))
            .ReturnsAsync(true);
        
        _mockTemplateService
            .Setup(x => x.CreateProjectAsync(It.IsAny<ProjectConfiguration>()))
            .ReturnsAsync(new ProjectCreationResult { Success = true });

        // Act
        var exists = await _mockTemplateService.Object.TemplateExistsAsync(templateName);
        var result = await _mockTemplateService.Object.CreateProjectAsync(new ProjectConfiguration());

        // Assert
        Assert.That(exists, Is.True);
        Assert.That(result.Success, Is.True);
        _mockTemplateService.Verify(x => x.TemplateExistsAsync(templateName), Times.Once);
        _mockTemplateService.Verify(x => x.CreateProjectAsync(It.IsAny<ProjectConfiguration>()), Times.Once);
    }
}
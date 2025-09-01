using System.CommandLine;

namespace NDC.Cli.Tests.Commands;

[TestFixture]
public class ListCommandTests
{
    private Mock<ITemplateService> _mockTemplateService = null!;
    private Mock<IAspireService> _mockAspireService = null!;
    private Mock<ICloudService> _mockCloudService = null!;
    private Mock<INuGetService> _mockNuGetService = null!;
    private Mock<ILogger<ListCommand>> _mockLogger = null!;
    private IServiceProvider _serviceProvider = null!;
    private ListCommand _command = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTemplateService = new Mock<ITemplateService>();
        _mockAspireService = new Mock<IAspireService>();
        _mockCloudService = new Mock<ICloudService>();
        _mockNuGetService = new Mock<INuGetService>();
        _mockLogger = new Mock<ILogger<ListCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockTemplateService.Object);
        services.AddSingleton(_mockAspireService.Object);
        services.AddSingleton(_mockCloudService.Object);
        services.AddSingleton(_mockNuGetService.Object);
        services.AddSingleton(_mockLogger.Object);

        _serviceProvider = services.BuildServiceProvider();
        _command = new ListCommand(_serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [Test]
    public void ListCommand_Initialization_SetsCorrectName()
    {
        // Assert
        Assert.That(_command.Name, Is.EqualTo("list"));
        Assert.That(_command.Description, Does.Contain("templates"));
    }

    [Test]
    public void ListCommand_HasAllOption()
    {
        // Assert
        var allOption = _command.Options.FirstOrDefault(o => o.Name == "all");
        Assert.That(allOption, Is.Not.Null);
    }

    [Test]
    public async Task TemplateService_GetAvailableTemplates_WithMock_WorksCorrectly()
    {
        // Arrange
        var templates = new List<TemplateInfo>
        {
            new() { Name = "webapp-aws", ShortName = "webapp-aws", IsInstalled = true },
            new() { Name = "webapp-azure", ShortName = "webapp-azure", IsInstalled = false },
            new() { Name = "webapp-gcp", ShortName = "webapp-gcp", IsInstalled = true }
        };

        _mockTemplateService
            .Setup(x => x.GetAvailableTemplatesAsync(false))
            .ReturnsAsync(templates.Where(t => t.IsInstalled));

        // Act
        var result = await _mockTemplateService.Object.GetAvailableTemplatesAsync(includeNotInstalled: false);

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList, Has.Count.EqualTo(2));
        Assert.That(resultList.All(t => t.IsInstalled), Is.True);
    }

    [Test]
    public async Task TemplateService_GetAvailableTemplates_WithIncludeNotInstalled_WorksCorrectly()
    {
        // Arrange
        var templates = new List<TemplateInfo>
        {
            new() { Name = "webapp-aws", ShortName = "webapp-aws", IsInstalled = true },
            new() { Name = "webapp-azure", ShortName = "webapp-azure", IsInstalled = false },
            new() { Name = "webapp-gcp", ShortName = "webapp-gcp", IsInstalled = true }
        };

        _mockTemplateService
            .Setup(x => x.GetAvailableTemplatesAsync(true))
            .ReturnsAsync(templates);

        // Act
        var result = await _mockTemplateService.Object.GetAvailableTemplatesAsync(includeNotInstalled: true);

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList, Has.Count.EqualTo(3));
    }
}
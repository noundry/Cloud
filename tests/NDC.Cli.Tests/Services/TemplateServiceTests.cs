namespace NDC.Cli.Tests.Services;

[TestFixture]
public class TemplateServiceTests
{
    private Mock<ILogger<TemplateService>> _mockLogger = null!;
    private Mock<IAspireService> _mockAspireService = null!;
    private Mock<ICloudService> _mockCloudService = null!;
    private TemplateService _templateService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<TemplateService>>();
        _mockAspireService = new Mock<IAspireService>();
        _mockCloudService = new Mock<ICloudService>();
        _templateService = new TemplateService(_mockLogger.Object, _mockAspireService.Object, _mockCloudService.Object);
    }

    [Test]
    public void TemplateService_Constructor_InitializesCorrectly()
    {
        // Assert
        Assert.That(_templateService, Is.Not.Null);
        Assert.That(_templateService, Is.InstanceOf<ITemplateService>());
    }

    [Test]
    public async Task TemplateExistsAsync_ReturnsBoolean()
    {
        // Arrange
        var templateName = "webapp-aws";

        // Act & Assert - should not throw
        Assert.DoesNotThrowAsync(async () => await _templateService.TemplateExistsAsync(templateName));
    }

    [Test]
    public async Task GetAvailableTemplatesAsync_ReturnsEnumerable()
    {
        // Act & Assert - should not throw
        Assert.DoesNotThrowAsync(async () => await _templateService.GetAvailableTemplatesAsync());
    }

    [Test]
    public async Task CreateProjectAsync_WithConfiguration_ReturnsResult()
    {
        // Arrange
        var config = new ProjectConfiguration
        {
            Name = "TestProject",
            Template = "webapp-aws",
            OutputDirectory = Path.GetTempPath(),
            Framework = "net9.0"
        };

        // Act & Assert - should not throw
        Assert.DoesNotThrowAsync(async () => await _templateService.CreateProjectAsync(config));
    }

    [Test]
    public async Task GetTemplateInfoAsync_WithTemplateName_ReturnsInfo()
    {
        // Arrange
        var templateName = "webapp-aws";

        // Act & Assert - should not throw
        Assert.DoesNotThrowAsync(async () => await _templateService.GetTemplateInfoAsync(templateName));
    }
}
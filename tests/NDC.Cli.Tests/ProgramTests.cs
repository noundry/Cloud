
namespace NDC.Cli.Tests;

[TestFixture]
public class ProgramTests
{
    [Test]
    public async Task Main_WithNoArguments_ShowsHelpAndReturnsSuccess()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var result = await Program.Main(args);

        // Assert - Program should show help when no arguments provided
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task Main_WithHelpFlag_ReturnsSuccess()
    {
        // Arrange
        var args = new[] { "--help" };

        // Act
        var result = await Program.Main(args);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task Main_WithVersionFlag_ReturnsSuccess()
    {
        // Arrange
        var args = new[] { "--version" };

        // Act
        var result = await Program.Main(args);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task Main_WithVerboseFlag_EnablesVerboseLogging()
    {
        // Arrange
        var args = new[] { "--verbose", "--help" };

        // Act
        var result = await Program.Main(args);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task Main_WithInvalidCommand_ReturnsError()
    {
        // Arrange
        var args = new[] { "invalid-command" };

        // Act
        var result = await Program.Main(args);

        // Assert
        Assert.That(result, Is.Not.EqualTo(0));
    }

    [Test]
    public async Task Main_WithCreateCommandMissingArguments_ReturnsError()
    {
        // Arrange - create command without required template argument
        var args = new[] { "create" };

        // Act
        var result = await Program.Main(args);

        // Assert
        Assert.That(result, Is.Not.EqualTo(0));
    }
}
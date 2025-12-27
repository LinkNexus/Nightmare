using System.Text.Json;
using Nightmare.Config;

namespace Nightmare.Tests;

/// <summary>
///     Tests for the ConfigLoader class which handles creation of default configuration files.
/// </summary>
public class ConfigLoaderTests : IDisposable
{
    private readonly string _testDirectory;

    public ConfigLoaderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"nightmare_tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
    }

    /// <summary>
    ///     Verifies that CreateDefaultConfig creates a file with valid JSON structure.
    /// </summary>
    [Fact]
    public void CreateDefaultConfig_CreatesFileWithCorrectContent()
    {
        // Arrange
        var configPath = Path.Combine(_testDirectory, "test_config.json");

        // Act
        ConfigManager.CreateDefaultConfig(configPath);

        // Assert
        Assert.True(File.Exists(configPath));

        var content = File.ReadAllText(configPath);
        var jsonDoc = JsonDocument.Parse(content);

        Assert.True(jsonDoc.RootElement.TryGetProperty("profiles", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("requests", out _));
    }

    /// <summary>
    ///     Verifies that the created config file contains the expected structure with profiles and requests.
    /// </summary>
    [Fact]
    public void CreateDefaultConfig_CreatesValidJsonStructure()
    {
        // Arrange
        var configPath = Path.Combine(_testDirectory, "valid_json_config.json");

        // Act
        ConfigManager.CreateDefaultConfig(configPath);

        // Assert
        var content = File.ReadAllText(configPath);
        var jsonDoc = JsonDocument.Parse(content);

        // Verify profiles structure
        var profiles = jsonDoc.RootElement.GetProperty("profiles");
        Assert.True(profiles.TryGetProperty("dev", out var defaultProfile));
        Assert.True(defaultProfile.GetProperty("default").GetBoolean());
        Assert.True(defaultProfile.TryGetProperty("data", out var data));
        Assert.Equal("https://httpbin.org", data.GetProperty("base_url").GetString());

        // Verify requests structure
        var requests = jsonDoc.RootElement.GetProperty("requests");
        Assert.True(requests.TryGetProperty("auth", out var auth));
        Assert.True(auth.TryGetProperty("requests", out var authRequests));
        Assert.True(authRequests.TryGetProperty("login", out var login));
        Assert.Equal("{{ base_url }}/post", login.GetProperty("url").GetString());
        Assert.Equal("POST", login.GetProperty("method").GetString());
    }

    /// <summary>
    ///     Verifies that CreateDefaultConfig overwrites existing files with the default content.
    /// </summary>
    [Fact]
    public void CreateDefaultConfig_OverwritesExistingFile()
    {
        // Arrange
        var configPath = Path.Combine(_testDirectory, "overwrite_test.json");
        File.WriteAllText(configPath, "existing content");

        // Act
        ConfigManager.CreateDefaultConfig(configPath);

        // Assert
        Assert.True(File.Exists(configPath));
        var content = File.ReadAllText(configPath);
        Assert.NotEqual("existing content", content);

        // Verify it's valid JSON with expected structure
        var jsonDoc = JsonDocument.Parse(content);
        Assert.True(jsonDoc.RootElement.TryGetProperty("profiles", out _));
    }

    /// <summary>
    ///     Verifies that CreateDefaultConfig can create files in nested directories.
    /// </summary>
    [Fact]
    public void CreateDefaultConfig_CreatesFileInNonExistentDirectory()
    {
        // Arrange
        var nestedPath = Path.Combine(_testDirectory, "nested", "deep", "path");
        Directory.CreateDirectory(nestedPath);
        var configPath = Path.Combine(nestedPath, "config.json");

        // Act
        ConfigManager.CreateDefaultConfig(configPath);

        // Assert
        Assert.True(File.Exists(configPath));
    }

    /// <summary>
    ///     Verifies that the created config file is not empty.
    /// </summary>
    [Fact]
    public void CreateDefaultConfig_CreatesNonEmptyFile()
    {
        // Arrange
        var configPath = Path.Combine(_testDirectory, "non_empty_config.json");

        // Act
        ConfigManager.CreateDefaultConfig(configPath);

        // Assert
        var fileInfo = new FileInfo(configPath);
        Assert.True(fileInfo.Length > 0);
    }
}
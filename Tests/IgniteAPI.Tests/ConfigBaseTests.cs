using IgniteAPI.Attributes;
using IgniteAPI.Utils;
using System;
using System.IO;
using YamlDotNet.Serialization;
using Xunit;

namespace IgniteAPI.Tests;

/// <summary>
/// Tests for <see cref="ConfigBase{T}"/> YAML save/load round-trip and
/// <see cref="EnviromentVarLoader"/> environment variable injection.
/// </summary>
public class ConfigBaseTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _tempDir;

    public ConfigBaseTests(ITestOutputHelper output)
    {
        _output = output;
        _tempDir = Path.Combine(Path.GetTempPath(), "IgniteApiTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    #region Test config class

    public class TestConfig : ConfigBase<TestConfig>
    {
        [YamlMember(Description = "Server name")]
        public string ServerName { get; set; } = "DefaultServer";

        public int Port { get; set; } = 27016;

        [EnvVar("TEST_CFG_SERVER_NAME")]
        public string EnvOverrideName { get; set; } = "FromDefault";
    }

    #endregion

    /// <summary>
    /// Verifies that saving and loading a config file round-trips all property values.
    /// </summary>
    [Fact]
    public void SaveAndLoad_RoundtripsValues()
    {
        var filePath = Path.Combine(_tempDir, "config.yml");

        var original = new TestConfig
        {
            filePath = filePath,
            ServerName = "MyServer",
            Port = 12345,
            EnvOverrideName = "Original"
        };
        original.Save();

        _output.WriteLine($"Saved to: {filePath}");
        _output.WriteLine(File.ReadAllText(filePath));

        var loaded = TestConfig.LoadYaml(filePath);

        _output.WriteLine($"Loaded ServerName: {loaded.ServerName}, Port: {loaded.Port}");

        Assert.Equal("MyServer", loaded.ServerName);
        Assert.Equal(12345, loaded.Port);
        Assert.Equal(filePath, loaded.filePath);
    }

    /// <summary>
    /// Verifies that LoadYaml creates a default config file when none exists.
    /// </summary>
    [Fact]
    public void LoadYaml_CreatesDefault_WhenFileNotFound()
    {
        var filePath = Path.Combine(_tempDir, "new_config.yml");

        Assert.False(File.Exists(filePath));

        var config = TestConfig.LoadYaml(filePath);

        _output.WriteLine($"Default ServerName: {config.ServerName}, Port: {config.Port}");

        Assert.True(File.Exists(filePath));
        Assert.Equal("DefaultServer", config.ServerName);
        Assert.Equal(27016, config.Port);
    }

    /// <summary>
    /// Verifies that <see cref="EnviromentVarLoader"/> overrides properties
    /// annotated with <see cref="EnvVarAttribute"/> from environment variables.
    /// </summary>
    [Fact]
    public void EnvVarLoader_OverridesAnnotatedProperty()
    {
        var config = new TestConfig();
        Assert.Equal("FromDefault", config.EnvOverrideName);

        Environment.SetEnvironmentVariable("TEST_CFG_SERVER_NAME", "FromEnv", EnvironmentVariableTarget.Process);
        try
        {
            EnviromentVarLoader.Load(config);

            _output.WriteLine($"EnvOverrideName after load: {config.EnvOverrideName}");

            Assert.Equal("FromEnv", config.EnvOverrideName);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_CFG_SERVER_NAME", null, EnvironmentVariableTarget.Process);
        }
    }

    /// <summary>
    /// Verifies that properties without <see cref="EnvVarAttribute"/> are not
    /// affected by environment variable loading.
    /// </summary>
    [Fact]
    public void EnvVarLoader_DoesNotAffectUnannotatedProperties()
    {
        var config = new TestConfig { ServerName = "Keep" };

        EnviromentVarLoader.Load(config);

        _output.WriteLine($"ServerName after EnvVarLoader: {config.ServerName}");

        Assert.Equal("Keep", config.ServerName);
    }
}

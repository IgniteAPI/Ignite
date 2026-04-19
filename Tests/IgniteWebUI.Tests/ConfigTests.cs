using IgniteWebUI.Configs;
using Xunit;
using Xunit.Abstractions;

namespace IgniteWebUI.Tests;

/// <summary>
/// Tests for <see cref="IgniteWebUICfg"/> and <see cref="LoggingConfig"/> default values,
/// verifying that the panel configuration matches expected defaults out of the box.
/// </summary>
public class ConfigTests
{
    private readonly ITestOutputHelper _output;

    public ConfigTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Verifies that IgniteWebUICfg has expected default property values.
    /// </summary>
    [Fact]
    public void IgniteWebUICfg_DefaultValues()
    {
        var cfg = new IgniteWebUICfg();

        _output.WriteLine($"PanelName: {cfg.PanelName}, Port: {cfg.Port}");

        Assert.Equal("Torch2 Web UI", cfg.PanelName);
        Assert.Equal(7076, cfg.Port);
        Assert.NotNull(cfg.Logging);
        Assert.NotNull(cfg.DarkPalette);
        Assert.NotNull(cfg.LightPalette);
    }

    /// <summary>
    /// Verifies that LoggingConfig has the expected default values.
    /// </summary>
    [Fact]
    public void LoggingConfig_DefaultValues()
    {
        var cfg = new LoggingConfig();

        _output.WriteLine($"LogDir: {cfg.LogDirectory}, MaxAge: {cfg.MaxLogAgeDays}d, " +
                          $"Level: {cfg.LogLevel}, MaxEntries: {cfg.InstanceLogViewerMaxEntries}, " +
                          $"ChatMax: {cfg.InstanceChatViewerMaxEntries}");

        Assert.Equal("Logs", cfg.LogDirectory);
        Assert.Equal(30, cfg.MaxLogAgeDays);
        Assert.Equal("Information", cfg.LogLevel);
        Assert.True(cfg.EnableInstanceLogging);
        Assert.Equal(2000, cfg.InstanceLogViewerMaxEntries);
        Assert.Equal(1000, cfg.InstanceChatViewerMaxEntries);
    }
}

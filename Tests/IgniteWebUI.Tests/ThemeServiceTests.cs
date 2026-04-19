using IgniteWebUI.Configs;
using IgniteWebUI.Services;
using Xunit;
using Xunit.Abstractions;

namespace IgniteWebUI.Tests;

/// <summary>
/// Tests for <see cref="ThemeService"/> — MudBlazor theme creation from config,
/// dark/light mode toggle, and theme change notification.
/// </summary>
public class ThemeServiceTests
{
    private readonly ITestOutputHelper _output;

    public ThemeServiceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Verifies that the ThemeService creates a non-null theme on construction.
    /// </summary>
    [Fact]
    public void Constructor_CreatesTheme()
    {
        var config = new IgniteWebUICfg();
        var sut = new ThemeService(config);

        _output.WriteLine($"CurrentTheme is null: {sut.CurrentTheme == null}");

        Assert.NotNull(sut.CurrentTheme);
        Assert.NotNull(sut.CurrentTheme.PaletteDark);
        Assert.NotNull(sut.CurrentTheme.PaletteLight);
    }

    /// <summary>
    /// Verifies that IsDarkMode defaults to true.
    /// </summary>
    [Fact]
    public void IsDarkMode_DefaultsToTrue()
    {
        var sut = new ThemeService(new IgniteWebUICfg());

        _output.WriteLine($"IsDarkMode: {sut.IsDarkMode}");

        Assert.True(sut.IsDarkMode);
    }

    /// <summary>
    /// Verifies that UpdateTheme replaces the current theme and fires OnThemeChanged.
    /// </summary>
    [Fact]
    public async Task UpdateTheme_FiresOnThemeChanged()
    {
        var sut = new ThemeService(new IgniteWebUICfg());
        var originalTheme = sut.CurrentTheme;

        bool eventFired = false;
        sut.OnThemeChanged += () => { eventFired = true; return Task.CompletedTask; };

        sut.UpdateTheme();

        _output.WriteLine($"Event fired: {eventFired}, Theme replaced: {!ReferenceEquals(originalTheme, sut.CurrentTheme)}");

        Assert.True(eventFired);
        Assert.NotSame(originalTheme, sut.CurrentTheme);
    }

    /// <summary>
    /// Verifies that the theme layout properties include the expected border radius.
    /// </summary>
    [Fact]
    public void Theme_HasExpectedLayoutProperties()
    {
        var sut = new ThemeService(new IgniteWebUICfg());

        _output.WriteLine($"BorderRadius: {sut.CurrentTheme!.LayoutProperties.DefaultBorderRadius}");

        Assert.Equal("6px", sut.CurrentTheme.LayoutProperties.DefaultBorderRadius);
    }
}

using IgniteAPI.DTOs.Instances;
using IgniteAPI.Models;
using Xunit;
using Xunit.Abstractions;

namespace IgniteAPI.Tests;

/// <summary>
/// Tests for <see cref="ConfiguredInstance"/> and <see cref="TorchInstanceBase"/>.
/// </summary>
public class InstanceDtoTests
{
    private readonly ITestOutputHelper _output;

    public InstanceDtoTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Verifies that GetShortId returns the last 6 characters of the InstanceID in upper case.
    /// </summary>
    [Fact]
    public void GetShortId_ReturnsLast6Chars_UpperCase()
    {
        var instance = new ConfiguredInstance { InstanceID = "a1b2c3d4-e5f6-7890-abcd-ef1234567890" };
        var shortId = instance.GetShortId();

        _output.WriteLine($"InstanceID: {instance.InstanceID} → ShortId: {shortId}");

        Assert.Equal("567890", shortId);
        Assert.Equal(6, shortId.Length);
    }

    /// <summary>
    /// Verifies that GetShortId returns the full ID when it's 6 or fewer characters.
    /// </summary>
    [Fact]
    public void GetShortId_ShortId_ReturnsAsIs()
    {
        var instance = new ConfiguredInstance { InstanceID = "abc" };
        var shortId = instance.GetShortId();

        _output.WriteLine($"InstanceID: \"{instance.InstanceID}\" → ShortId: \"{shortId}\"");

        Assert.Equal("abc", shortId);
    }

    /// <summary>
    /// Verifies that GetShortId returns empty for null or empty InstanceID.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetShortId_NullOrEmpty_ReturnsEmpty(string? id)
    {
        var instance = new ConfiguredInstance { InstanceID = id! };
        var shortId = instance.GetShortId();

        _output.WriteLine($"InstanceID: \"{id ?? "(null)"}\" → ShortId: \"{shortId}\"");

        Assert.Equal(string.Empty, shortId);
    }

    /// <summary>
    /// Verifies that UpdateFromConfiguredInstance copies all properties from the source.
    /// </summary>
    [Fact]
    public void UpdateFromConfiguredInstance_CopiesAllProperties()
    {
        var source = new TorchInstanceBase
        {
            InstanceID = "id-1",
            Name = "Server1",
            MachineName = "HOST",
            IPAddress = "10.0.0.1",
            GamePort = 27016,
            ProfileName = "Default",
            TargetWorld = "Earth",
            TorchVersion = "v2.0.0",
            ServerStatus = ServerStatusEnum.Running,
            SimSpeed = 1.0f,
            PlayersOnline = 5,
            TotalGrids = 100
        };

        var target = new TorchInstanceBase();
        target.UpdateFromConfiguredInstance(source);

        _output.WriteLine($"Copied Name: {target.Name}, Status: {target.ServerStatus}, Players: {target.PlayersOnline}");

        Assert.Equal("id-1", target.InstanceID);
        Assert.Equal("Server1", target.Name);
        Assert.Equal("HOST", target.MachineName);
        Assert.Equal(27016, target.GamePort);
        Assert.Equal(ServerStatusEnum.Running, target.ServerStatus);
        Assert.Equal(1.0f, target.SimSpeed);
        Assert.Equal((ushort)5, target.PlayersOnline);
        Assert.Equal(100u, target.TotalGrids);
    }

    /// <summary>
    /// Verifies that GetFormattedGameUptime formats correctly with zero-padded segments.
    /// </summary>
    [Fact]
    public void GetFormattedGameUptime_FormatsCorrectly()
    {
        var instance = new TorchInstanceBase
        {
            GameUpTime = new TimeSpan(1, 2, 30, 5)
        };

        var formatted = instance.GetFormattedGameUptime();

        _output.WriteLine($"GameUpTime: {instance.GameUpTime} → Formatted: {formatted}");

        Assert.Equal("01:02:30:05", formatted);
    }

    /// <summary>
    /// Verifies that negative uptime is clamped to zero in the formatted output.
    /// </summary>
    [Fact]
    public void GetFormattedGameUptime_NegativeUptime_ClampsToZero()
    {
        var instance = new TorchInstanceBase
        {
            GameUpTime = TimeSpan.FromSeconds(-30)
        };

        var formatted = instance.GetFormattedGameUptime();

        _output.WriteLine($"Negative GameUpTime → Formatted: {formatted}");

        Assert.Equal("00:00:00:00", formatted);
    }
}

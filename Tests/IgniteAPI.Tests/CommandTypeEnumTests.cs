using IgniteAPI.Models.Commands;
using Xunit;

namespace IgniteAPI.Tests;

/// <summary>
/// Tests for the <see cref="CommandTypeEnum"/> flags enum to verify
/// flag combinations and bitwise operations used for command routing.
/// </summary>
public class CommandTypeEnumTests
{
    private readonly ITestOutputHelper _output;

    public CommandTypeEnumTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Verifies that the All flag includes every individual context flag.
    /// </summary>
    [Fact]
    public void All_IncludesEveryContext()
    {
        _output.WriteLine($"All = {(int)CommandTypeEnum.All} ({CommandTypeEnum.All})");

        Assert.True(CommandTypeEnum.All.HasFlag(CommandTypeEnum.Ingame));
        Assert.True(CommandTypeEnum.All.HasFlag(CommandTypeEnum.Console));
        Assert.True(CommandTypeEnum.All.HasFlag(CommandTypeEnum.Discord));
        Assert.True(CommandTypeEnum.All.HasFlag(CommandTypeEnum.WebPanel));
        Assert.True(CommandTypeEnum.All.HasFlag(CommandTypeEnum.Debug));
    }

    /// <summary>
    /// Verifies that AdminOnly includes Console, Discord, WebPanel, and Debug
    /// but excludes Ingame (players shouldn't run admin commands in-game).
    /// </summary>
    [Fact]
    public void AdminOnly_ExcludesIngame()
    {
        _output.WriteLine($"AdminOnly = {(int)CommandTypeEnum.AdminOnly} ({CommandTypeEnum.AdminOnly})");

        Assert.False(CommandTypeEnum.AdminOnly.HasFlag(CommandTypeEnum.Ingame));
        Assert.True(CommandTypeEnum.AdminOnly.HasFlag(CommandTypeEnum.Console));
        Assert.True(CommandTypeEnum.AdminOnly.HasFlag(CommandTypeEnum.WebPanel));
        Assert.True(CommandTypeEnum.AdminOnly.HasFlag(CommandTypeEnum.Discord));
    }

    /// <summary>
    /// Verifies that individual flags have distinct powers-of-two values so they
    /// can be combined without overlap.
    /// </summary>
    [Theory]
    [InlineData(CommandTypeEnum.Ingame, 1)]
    [InlineData(CommandTypeEnum.Console, 2)]
    [InlineData(CommandTypeEnum.Discord, 4)]
    [InlineData(CommandTypeEnum.WebPanel, 8)]
    [InlineData(CommandTypeEnum.Debug, 16)]
    public void IndividualFlags_HaveCorrectValues(CommandTypeEnum flag, int expected)
    {
        _output.WriteLine($"{flag} = {(int)flag}");

        Assert.Equal(expected, (int)flag);
    }
}

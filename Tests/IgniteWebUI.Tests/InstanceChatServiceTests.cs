using IgniteAPI.DTOs.Chat;
using IgniteWebUI.Configs;
using IgniteWebUI.Services.InstanceServices;
using Xunit;
using Xunit.Abstractions;

namespace IgniteWebUI.Tests;

/// <summary>
/// Tests for <see cref="InstanceChatService"/> — the panel-side rolling chat store
/// that keeps per-instance chat history and notifies Blazor components.
/// </summary>
public class InstanceChatServiceTests
{
    private readonly ITestOutputHelper _output;
    private readonly InstanceChatService _sut;

    public InstanceChatServiceTests(ITestOutputHelper output)
    {
        _output = output;
        var config = new IgniteWebUICfg();
        config.Logging.InstanceChatViewerMaxEntries = 3;
        config.Logging.EnableInstanceLogging = false;
        _sut = new InstanceChatService(config);
    }

    private static ChatMessage MakeChat(string msg, string player = "Player1") => new()
    {
        DisplayName = player,
        Message = msg,
        SteamId = 12345,
        Channel = "Global",
        Timestamp = DateTime.UtcNow
    };

    /// <summary>
    /// Verifies that an appended chat message appears in the instance's history.
    /// </summary>
    [Fact]
    public void Append_MessageAppearsInHistory()
    {
        _sut.Append("inst-1", MakeChat("hello"));

        var history = _sut.GetHistory("inst-1");

        _output.WriteLine($"History count: {history.Length}");

        Assert.Single(history);
        Assert.Equal("hello", history[0].Message);
    }

    /// <summary>
    /// Verifies that the rolling buffer evicts oldest messages when max is exceeded.
    /// </summary>
    [Fact]
    public void Append_RollingBuffer_EvictsOldest()
    {
        for (int i = 0; i < 6; i++)
            _sut.Append("inst-1", MakeChat($"msg-{i}"));

        var history = _sut.GetHistory("inst-1");

        _output.WriteLine($"History count: {history.Length}, first: {history[0].Message}");

        Assert.Equal(3, history.Length);
        Assert.Equal("msg-3", history[0].Message);
    }

    /// <summary>
    /// Verifies that the OnChat event fires when a message is appended.
    /// </summary>
    [Fact]
    public void Append_RaisesOnChat()
    {
        string? receivedId = null;
        ChatMessage? receivedMsg = null;
        _sut.OnChat += (id, msg) => { receivedId = id; receivedMsg = msg; };

        _sut.Append("inst-2", MakeChat("event"));

        _output.WriteLine($"Received instanceId: {receivedId}, message: {receivedMsg?.Message}");

        Assert.Equal("inst-2", receivedId);
        Assert.Equal("event", receivedMsg!.Message);
    }

    /// <summary>
    /// Verifies that GetHistory returns empty for an unknown instance.
    /// </summary>
    [Fact]
    public void GetHistory_UnknownInstance_ReturnsEmpty()
    {
        var history = _sut.GetHistory("unknown");

        _output.WriteLine($"History count for unknown: {history.Length}");

        Assert.Empty(history);
    }

    /// <summary>
    /// Verifies that Append sets InstanceName from the provided parameter.
    /// </summary>
    [Fact]
    public void Append_SetsInstanceName_FromParameter()
    {
        var msg = MakeChat("test");
        _sut.Append("inst-1", msg, "MyServer");

        var history = _sut.GetHistory("inst-1");

        _output.WriteLine($"InstanceName: {history[0].InstanceName}");

        Assert.Equal("MyServer", history[0].InstanceName);
    }

    /// <summary>
    /// Verifies that GetAllHistory returns messages from all instances.
    /// </summary>
    [Fact]
    public void GetAllHistory_ReturnsFromAllInstances()
    {
        _sut.Append("inst-a", MakeChat("msg-a"));
        _sut.Append("inst-b", MakeChat("msg-b"));

        var all = _sut.GetAllHistory();

        _output.WriteLine($"All history count: {all.Length}");

        Assert.Equal(2, all.Length);
    }
}

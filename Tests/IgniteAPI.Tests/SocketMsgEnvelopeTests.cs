using IgniteAPI.DTOs.WebSockets;
using IgniteAPI.Constants;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace IgniteAPI.Tests;

/// <summary>
/// Tests for <see cref="SocketMsgEnvelope"/> serialization/deserialization,
/// verifying the JSON contract matches what the WebSocket transport expects.
/// </summary>
public class SocketMsgEnvelopeTests
{
    private readonly ITestOutputHelper _output;

    public SocketMsgEnvelopeTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Verifies that a SocketMsgEnvelope round-trips through JSON serialization
    /// using the shared <see cref="TorchConstants.JsonOptions"/>.
    /// </summary>
    [Fact]
    public void Roundtrip_WithTorchJsonOptions()
    {
        var original = new SocketMsgEnvelope("server.start")
        {
            RequestId = "req-123",
            UserID = "user-456"
        };

        // Set args via a parsed JsonElement
        var argsJson = JsonSerializer.Serialize(new { world = "Earth", save = true });
        original.Args = JsonDocument.Parse(argsJson).RootElement;

        var json = JsonSerializer.Serialize(original, TorchConstants.JsonOptions);
        _output.WriteLine($"Serialized: {json}");

        var deserialized = JsonSerializer.Deserialize<SocketMsgEnvelope>(json, TorchConstants.JsonOptions)!;

        _output.WriteLine($"Deserialized command: {deserialized.Command}, requestId: {deserialized.RequestId}");

        Assert.Equal("server.start", deserialized.Command);
        Assert.Equal("req-123", deserialized.RequestId);
        Assert.Equal("user-456", deserialized.UserID);
        Assert.Equal("Earth", deserialized.Args.GetProperty("world").GetString());
        Assert.True(deserialized.Args.GetProperty("save").GetBoolean());
    }

    /// <summary>
    /// Verifies that TorchConstants.JsonOptions uses camelCase naming, so
    /// "Command" serializes as "command" on the wire.
    /// </summary>
    [Fact]
    public void JsonOptions_UsesCamelCase()
    {
        var envelope = new SocketMsgEnvelope("test.cmd");
        envelope.Args = JsonDocument.Parse("{}").RootElement;
        var json = JsonSerializer.Serialize(envelope, TorchConstants.JsonOptions);

        _output.WriteLine($"JSON: {json}");

        Assert.Contains("\"command\"", json);
        Assert.DoesNotContain("\"Command\"", json);
    }

    /// <summary>
    /// Verifies that JsonOptions is case-insensitive on deserialization, so
    /// "Command" or "command" both work.
    /// </summary>
    [Fact]
    public void JsonOptions_IsCaseInsensitiveOnDeserialize()
    {
        var json = "{\"Command\":\"test.ping\",\"requestId\":null,\"args\":{}}";

        var envelope = JsonSerializer.Deserialize<SocketMsgEnvelope>(json, TorchConstants.JsonOptions)!;

        _output.WriteLine($"Deserialized command: {envelope.Command}");

        Assert.Equal("test.ping", envelope.Command);
    }
}

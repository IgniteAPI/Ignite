using IgniteAPI.DTOs.Logs;
using IgniteAPI.Constants;
using System;
using System.Text.Json;
using Xunit;

namespace IgniteAPI.Tests;

/// <summary>
/// Tests for <see cref="LogLine"/> DTO serialization and property behavior.
/// </summary>
public class LogLineTests
{
    private readonly ITestOutputHelper _output;

    public LogLineTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Verifies that LogLine round-trips through JSON using TorchConstants.JsonOptions
    /// (camelCase on the wire, case-insensitive on read).
    /// </summary>
    [Fact]
    public void Roundtrip_Json_WithTorchOptions()
    {
        var original = new LogLine
        {
            InstanceName = "Server1",
            LoggerName = "NLog.Main",
            Level = "Warn",
            Message = "Low memory",
            Timestamp = new DateTime(2025, 6, 15, 12, 30, 0, DateTimeKind.Utc)
        };

        var json = JsonSerializer.Serialize(original, TorchConstants.JsonOptions);
        _output.WriteLine($"Serialized: {json}");

        var result = JsonSerializer.Deserialize<LogLine>(json, TorchConstants.JsonOptions)!;

        _output.WriteLine($"Deserialized: Level={result.Level}, Message={result.Message}");

        Assert.Equal("Server1", result.InstanceName);
        Assert.Equal("NLog.Main", result.LoggerName);
        Assert.Equal("Warn", result.Level);
        Assert.Equal("Low memory", result.Message);
        Assert.Equal(original.Timestamp, result.Timestamp);
    }

    /// <summary>
    /// Verifies that default LogLine properties are null/default and don't throw.
    /// </summary>
    [Fact]
    public void DefaultProperties_AreNull()
    {
        var line = new LogLine();

        _output.WriteLine($"Default InstanceName: {line.InstanceName ?? "(null)"}, Timestamp: {line.Timestamp}");

        Assert.Null(line.InstanceName);
        Assert.Null(line.LoggerName);
        Assert.Null(line.Level);
        Assert.Null(line.Message);
        Assert.Equal(default(DateTime), line.Timestamp);
    }
}

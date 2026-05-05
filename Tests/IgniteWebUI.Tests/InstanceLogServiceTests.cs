using IgniteAPI.DTOs.Logs;
using IgniteWebUI.Configs;
using IgniteWebUI.Models;
using IgniteWebUI.Services.InstanceServices;
using IgniteWebUI.Services.SQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IgniteWebUI.Tests;

/// <summary>
/// Tests for <see cref="InstanceLogService"/> — the panel-side rolling log store
/// that receives log entries from connected instances and notifies Blazor components.
/// </summary>
public class InstanceLogServiceTests
{
    private readonly ITestOutputHelper _output;
    private readonly InstanceLogService _sut;

    public InstanceLogServiceTests(ITestOutputHelper output)
    {
        _output = output;

        var config = new IgniteWebUICfg();
        config.Logging.InstanceLogViewerMaxEntries = 5;
        config.Logging.EnableInstanceLogging = false;

        // Use a shared in-memory connection so the schema persists
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        services.AddMemoryCache();
        var sp = services.BuildServiceProvider();

        // Create the schema
        using (var scope = sp.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        var cache = sp.GetRequiredService<IMemoryCache>();
        var instanceManager = new InstanceManager(scopeFactory, cache, new IgniteWebUICfg());

        _sut = new InstanceLogService(config, instanceManager);
    }

    private static LogLine MakeLine(string msg, string level = "Info") => new()
    {
        InstanceName = "Server1",
        LoggerName = "Test",
        Level = level,
        Message = msg,
        Timestamp = DateTime.UtcNow
    };

    /// <summary>
    /// Verifies that an appended log entry appears in the instance's history.
    /// </summary>
    [Fact]
    public void Append_EntryAppearsInHistory()
    {
        _sut.Append("inst-1", MakeLine("hello"));

        var history = _sut.GetHistory("inst-1");

        _output.WriteLine($"History count: {history.Length}");

        Assert.Single(history);
        Assert.Equal("hello", history[0].Message);
    }

    /// <summary>
    /// Verifies that the rolling buffer respects MaxPerInstance and evicts oldest entries.
    /// </summary>
    [Fact]
    public void Append_RollingBuffer_EvictsOldest()
    {
        for (int i = 0; i < 8; i++)
            _sut.Append("inst-1", MakeLine($"msg-{i}"));

        var history = _sut.GetHistory("inst-1");

        _output.WriteLine($"History count: {history.Length}, first: {history[0].Message}");

        Assert.Equal(5, history.Length);
        Assert.Equal("msg-3", history[0].Message);
    }

    /// <summary>
    /// Verifies that the OnLog event fires with the correct instance ID and entry.
    /// </summary>
    [Fact]
    public void Append_RaisesOnLog()
    {
        string? receivedId = null;
        LogLine? receivedEntry = null;
        _sut.OnLog += (id, entry) => { receivedId = id; receivedEntry = entry; };

        _sut.Append("inst-2", MakeLine("event"));

        _output.WriteLine($"Received instanceId: {receivedId}, message: {receivedEntry?.Message}");

        Assert.Equal("inst-2", receivedId);
        Assert.Equal("event", receivedEntry!.Message);
    }

    /// <summary>
    /// Verifies that GetHistory returns empty for an unknown instance ID.
    /// </summary>
    [Fact]
    public void GetHistory_UnknownInstance_ReturnsEmpty()
    {
        var history = _sut.GetHistory("nonexistent");

        _output.WriteLine($"History count for unknown: {history.Length}");

        Assert.Empty(history);
    }

    /// <summary>
    /// Verifies that AppendHistory bulk-loads entries and respects the max capacity.
    /// </summary>
    [Fact]
    public void AppendHistory_BulkLoad_RespectsMax()
    {
        var entries = Enumerable.Range(0, 10)
            .Select(i => MakeLine($"bulk-{i}"))
            .ToArray();

        _sut.AppendHistory("inst-3", entries);

        var history = _sut.GetHistory("inst-3");

        _output.WriteLine($"History count after bulk: {history.Length}, first: {history[0].Message}");

        Assert.Equal(5, history.Length);
        Assert.Equal("bulk-5", history[0].Message);
    }

    /// <summary>
    /// Verifies that GetAllHistory returns logs from all instances sorted by timestamp.
    /// </summary>
    [Fact]
    public void GetAllHistory_ReturnsAllInstancesSorted()
    {
        _sut.Append("inst-a", new LogLine
        {
            Message = "first", Level = "Info",
            Timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        _sut.Append("inst-b", new LogLine
        {
            Message = "second", Level = "Info",
            Timestamp = new DateTime(2025, 1, 1, 0, 0, 1, DateTimeKind.Utc)
        });

        var all = _sut.GetAllHistory();

        _output.WriteLine($"All history count: {all.Length}");

        Assert.Equal(2, all.Length);
        Assert.Equal("first", all[0].Message);
        Assert.Equal("second", all[1].Message);
    }

    /// <summary>
    /// Verifies that Append sets InstanceName from the provided instanceName parameter.
    /// </summary>
    [Fact]
    public void Append_SetsInstanceName_FromParameter()
    {
        var entry = MakeLine("test");
        _sut.Append("inst-1", entry, "MyServer");

        var history = _sut.GetHistory("inst-1");

        _output.WriteLine($"InstanceName: {history[0].InstanceName}");

        Assert.Equal("MyServer", history[0].InstanceName);
    }
}

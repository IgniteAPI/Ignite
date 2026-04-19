using IgniteAPI.DTOs.Instances;
using IgniteAPI.Models;
using IgniteAPI.Models.Configs;
using IgniteWebUI.Models;
using IgniteWebUI.Services.InstanceServices;
using IgniteWebUI.Services.SQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IgniteWebUI.Tests;

/// <summary>
/// Tests for <see cref="InstanceManager"/> — the panel-side in-memory registry
/// of connected Torch instances, including registration, status updates, and lookups.
/// </summary>
public class InstanceManagerTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly InstanceManager _sut;
    private readonly ServiceProvider _sp;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

    public InstanceManagerTests(ITestOutputHelper output)
    {
        _output = output;

        // Use a shared in-memory connection so the schema persists across scopes
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(_connection));
        services.AddMemoryCache();
        _sp = services.BuildServiceProvider();

        // Create the schema
        using (var scope = _sp.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        var scopeFactory = _sp.GetRequiredService<IServiceScopeFactory>();
        var cache = _sp.GetRequiredService<IMemoryCache>();

        _sut = new InstanceManager(scopeFactory, cache);
        _sut.EnableServerDiscovery = true;
    }

    public void Dispose()
    {
        _sp.Dispose();
        _connection.Dispose();
    }

    private static TorchInstanceBase MakeInstance(string id, string name = "TestServer") => new()
    {
        InstanceID = id,
        Name = name,
        MachineName = "HOST",
        IPAddress = "10.0.0.1",
        GamePort = 27016,
        ServerStatus = ServerStatusEnum.Running
    };

    /// <summary>
    /// Verifies that RegisterInstance adds a new instance to ActiveInstances
    /// when server discovery is enabled.
    /// </summary>
    [Fact]
    public void RegisterInstance_AddsToActiveInstances()
    {
        var instance = MakeInstance("id-1");
        var result = _sut.RegisterInstance(instance);

        _output.WriteLine($"RegisterInstance returned: {result}, ActiveInstances count: {_sut.ActiveInstances.Count}");

        Assert.True(result);
        Assert.True(_sut.ActiveInstances.ContainsKey("id-1"));
    }

    /// <summary>
    /// Verifies that RegisterInstance returns false for null or empty instance IDs.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterInstance_RejectsInvalidIds(string? id)
    {
        var instance = new TorchInstanceBase { InstanceID = id! };
        var result = _sut.RegisterInstance(instance);

        _output.WriteLine($"RegisterInstance(\"{id ?? "(null)"}\") returned: {result}");

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that RegisterInstance returns false when server discovery is disabled
    /// and the instance is not in the database.
    /// </summary>
    [Fact]
    public void RegisterInstance_ReturnsFalse_WhenDiscoveryDisabled()
    {
        _sut.EnableServerDiscovery = false;

        var result = _sut.RegisterInstance(MakeInstance("unknown-id"));

        _output.WriteLine($"RegisterInstance with discovery off: {result}");

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that UpdateStatus registers and updates an instance's properties.
    /// </summary>
    [Fact]
    public void UpdateStatus_UpdatesExistingInstance()
    {
        _sut.RegisterInstance(MakeInstance("id-2", "Original"));

        var updated = MakeInstance("id-2", "Updated");
        updated.SimSpeed = 0.95f;
        updated.PlayersOnline = 3;
        _sut.UpdateStatus(updated);

        var inst = _sut.ActiveInstances["id-2"];

        _output.WriteLine($"Name: {inst.Name}, SimSpeed: {inst.SimSpeed}, Players: {inst.PlayersOnline}");

        Assert.Equal("Updated", inst.Name);
        Assert.Equal(0.95f, inst.SimSpeed);
        Assert.Equal((ushort)3, inst.PlayersOnline);
    }

    /// <summary>
    /// Verifies that UpdateStatus fires the OnChange event with the instance ID.
    /// </summary>
    [Fact]
    public void UpdateStatus_FiresOnChange()
    {
        string? changedId = null;
        _sut.OnChange += id => changedId = id;

        _sut.UpdateStatus(MakeInstance("id-3"));

        _output.WriteLine($"OnChange fired with: {changedId}");

        Assert.Equal("id-3", changedId);
    }

    /// <summary>
    /// Verifies that GetInstanceByID retrieves an instance by its full ID.
    /// </summary>
    [Fact]
    public void GetInstanceByID_FindsByFullId()
    {
        _sut.RegisterInstance(MakeInstance("abcdef-123456"));

        var result = _sut.GetInstanceByID("abcdef-123456");

        _output.WriteLine($"Found instance: {result?.Name}");

        Assert.NotNull(result);
        Assert.Equal("abcdef-123456", result.InstanceID);
    }

    /// <summary>
    /// Verifies that GetInstanceByID resolves a 6-character short ID.
    /// </summary>
    [Fact]
    public void GetInstanceByID_FindsByShortId()
    {
        _sut.RegisterInstance(MakeInstance("a1b2c3-d4e5f6-789012"));

        var result = _sut.GetInstanceByID("789012");

        _output.WriteLine($"Short ID lookup result: {result?.InstanceID}");

        Assert.NotNull(result);
    }

    /// <summary>
    /// Verifies that GetInstanceByID returns null for a non-existent ID.
    /// </summary>
    [Fact]
    public void GetInstanceByID_ReturnsNull_WhenNotFound()
    {
        var result = _sut.GetInstanceByID("nonexistent");

        _output.WriteLine($"GetInstanceByID(\"nonexistent\"): {(result == null ? "null" : result.Name)}");

        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that UpdateProfiles stores profiles on the instance.
    /// </summary>
    [Fact]
    public void UpdateProfiles_StoresProfilesOnInstance()
    {
        _sut.RegisterInstance(MakeInstance("id-5"));

        var profiles = new List<ProfileCfg>
        {
            new() { InstanceName = "Default" },
            new() { InstanceName = "Custom" }
        };

        var result = _sut.UpdateProfiles("id-5", profiles);

        _output.WriteLine($"UpdateProfiles returned: {result}, profile count: {_sut.ActiveInstances["id-5"].Profiles.Count}");

        Assert.True(result);
        Assert.Equal(2, _sut.ActiveInstances["id-5"].Profiles.Count);
    }

    /// <summary>
    /// Verifies that UpdateProfiles returns false for unknown instance IDs.
    /// </summary>
    [Fact]
    public void UpdateProfiles_ReturnsFalse_ForUnknownInstance()
    {
        var result = _sut.UpdateProfiles("unknown", new List<ProfileCfg>());

        _output.WriteLine($"UpdateProfiles for unknown: {result}");

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that GetInstanceName returns the instance name or falls back to the ID.
    /// </summary>
    [Fact]
    public void GetInstanceName_ReturnsName_OrFallbackToId()
    {
        _sut.RegisterInstance(MakeInstance("id-6", "MyServer"));

        var name = _sut.GetInstanceName("id-6");
        var fallback = _sut.GetInstanceName("unknown-id");

        _output.WriteLine($"Known: {name}, Unknown fallback: {fallback}");

        Assert.Equal("MyServer", name);
        Assert.Equal("unknown-id", fallback);
    }

    /// <summary>
    /// Verifies that GetPendingInstances only returns non-configured instances.
    /// </summary>
    [Fact]
    public void GetPendingInstances_ReturnsOnlyUnconfigured()
    {
        _sut.RegisterInstance(MakeInstance("id-7"));
        _sut.RegisterInstance(MakeInstance("id-8"));

        // Manually mark one as configured
        _sut.ActiveInstances["id-7"].Configured = true;

        var pending = _sut.GetPendingInstances();

        _output.WriteLine($"Pending count: {pending.Count}");

        Assert.Single(pending);
        Assert.Equal("id-8", pending[0].InstanceID);
    }
}

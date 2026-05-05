using IgniteWebUI.Models.Database;
using IgniteWebUI.Models.DTOs;
using IgniteWebUI.Services.ModServices;
using IgniteWebUI.Services.SQL;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IgniteWebUI.Tests;

/// <summary>
/// Tests for <see cref="ModsService"/> — CRUD operations on mod lists and mods
/// using an in-memory SQLite database.
/// </summary>
public class ModsServiceTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _dbContext;
    private readonly ModsService _sut;

    public ModsServiceTests(ITestOutputHelper output)
    {
        _output = output;

        // Use a shared in-memory SQLite database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _sut = new ModsService(_dbContext, new FakeSteamService());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    /// <summary>Stub that avoids real HTTP calls to Steam.</summary>
    private class FakeSteamService : ISteamService
    {
        public Task<Dictionary<string, SteamPublishedFileDetails>> GetPublishedFileDetailsAsync(params string[] publishedFileIds)
            => Task.FromResult(new Dictionary<string, SteamPublishedFileDetails>());
    }

    /// <summary>
    /// Verifies that CreateModListAsync creates a list and returns a DTO with the correct name.
    /// </summary>
    [Fact]
    public async Task CreateModList_ReturnsDto()
    {
        var result = await _sut.CreateModListAsync(new CreateModListRequest
        {
            Name = "My Mods",
            Description = "A test mod list"
        });

        _output.WriteLine($"Created ModList: Id={result.Id}, Name={result.Name}");

        Assert.True(result.Id > 0);
        Assert.Equal("My Mods", result.Name);
        Assert.Equal("A test mod list", result.Description);
    }

    /// <summary>
    /// Verifies that GetAllModListsAsync returns all created mod lists.
    /// </summary>
    [Fact]
    public async Task GetAllModLists_ReturnsAll()
    {
        await _sut.CreateModListAsync(new CreateModListRequest { Name = "List1" });
        await _sut.CreateModListAsync(new CreateModListRequest { Name = "List2" });

        var all = await _sut.GetAllModListsAsync();

        _output.WriteLine($"Total mod lists: {all.Count}");

        Assert.Equal(2, all.Count);
    }

    /// <summary>
    /// Verifies that GetModListByIdAsync retrieves a specific list by ID.
    /// </summary>
    [Fact]
    public async Task GetModListById_ReturnsCorrectList()
    {
        var created = await _sut.CreateModListAsync(new CreateModListRequest { Name = "Target" });

        var result = await _sut.GetModListByIdAsync(created.Id);

        _output.WriteLine($"Retrieved: Id={result.Id}, Name={result.Name}");

        Assert.Equal("Target", result.Name);
    }

    /// <summary>
    /// Verifies that GetModListByIdAsync throws for a non-existent ID.
    /// </summary>
    [Fact]
    public async Task GetModListById_ThrowsForNonExistent()
    {
        _output.WriteLine("Expecting KeyNotFoundException for id 999");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetModListByIdAsync(999));
    }

    /// <summary>
    /// Verifies that GetModListByNameAsync retrieves a list by its name.
    /// </summary>
    [Fact]
    public async Task GetModListByName_ReturnsCorrectList()
    {
        await _sut.CreateModListAsync(new CreateModListRequest { Name = "ByName" });

        var result = await _sut.GetModListByNameAsync("ByName");

        _output.WriteLine($"Found by name: {result.Name}");

        Assert.Equal("ByName", result.Name);
    }

    /// <summary>
    /// Verifies that DeleteModListAsync removes the list and returns true.
    /// </summary>
    [Fact]
    public async Task DeleteModList_RemovesList()
    {
        var created = await _sut.CreateModListAsync(new CreateModListRequest { Name = "ToDelete" });

        var deleted = await _sut.DeleteModListAsync(created.Id);

        _output.WriteLine($"Delete returned: {deleted}");

        Assert.True(deleted);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetModListByIdAsync(created.Id));
    }

    /// <summary>
    /// Verifies that DeleteModListAsync returns false for a non-existent ID.
    /// </summary>
    [Fact]
    public async Task DeleteModList_ReturnsFalse_WhenNotFound()
    {
        var result = await _sut.DeleteModListAsync(999);

        _output.WriteLine($"Delete non-existent: {result}");

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that UpdateModListAsync updates the name and description.
    /// </summary>
    [Fact]
    public async Task UpdateModList_UpdatesProperties()
    {
        var created = await _sut.CreateModListAsync(new CreateModListRequest { Name = "Old", Description = "Old Desc" });

        var updated = await _sut.UpdateModListAsync(created.Id, new CreateModListRequest { Name = "New", Description = "New Desc" });

        _output.WriteLine($"Update returned: {updated}");

        Assert.True(updated);

        var result = await _sut.GetModListByIdAsync(created.Id);
        Assert.Equal("New", result.Name);
        Assert.Equal("New Desc", result.Description);
    }

    /// <summary>
    /// Verifies that AddModAsync adds a mod to a list and returns a DTO.
    /// </summary>
    [Fact]
    public async Task AddMod_AddsToList()
    {
        var list = await _sut.CreateModListAsync(new CreateModListRequest { Name = "ModList" });

        var mod = await _sut.AddModAsync(new AddModRequest
        {
            ModListId = list.Id,
            ModId = "12345",
            ModUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=12345",
            Source = "SteamWorkshop"
        });

        _output.WriteLine($"Added mod: Id={mod.Id}, ModId={mod.ModId}, Name={mod.Name}");

        Assert.Equal("12345", mod.ModId);
        Assert.Contains("12345", mod.Name);
    }

    /// <summary>
    /// Verifies that AddModAsync throws when adding a duplicate mod to the same list.
    /// </summary>
    [Fact]
    public async Task AddMod_ThrowsOnDuplicate()
    {
        var list = await _sut.CreateModListAsync(new CreateModListRequest { Name = "DupList" });

        await _sut.AddModAsync(new AddModRequest { ModListId = list.Id, ModId = "111", ModUrl = "http://test.com" });

        _output.WriteLine("Expecting InvalidOperationException for duplicate mod");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.AddModAsync(new AddModRequest { ModListId = list.Id, ModId = "111", ModUrl = "http://test.com" }));
    }

    /// <summary>
    /// Verifies that RemoveModAsync removes a mod and returns true.
    /// </summary>
    [Fact]
    public async Task RemoveMod_RemovesFromList()
    {
        var list = await _sut.CreateModListAsync(new CreateModListRequest { Name = "RemoveTest" });
        var mod = await _sut.AddModAsync(new AddModRequest { ModListId = list.Id, ModId = "222", ModUrl = "http://test.com" });

        var removed = await _sut.RemoveModAsync(mod.Id);

        _output.WriteLine($"Remove returned: {removed}");

        Assert.True(removed);

        var updatedList = await _sut.GetModListByIdAsync(list.Id);
        Assert.Empty(updatedList.Mods);
    }

    /// <summary>
    /// Verifies that AddModAsync throws for a non-existent mod list ID.
    /// </summary>
    [Fact]
    public async Task AddMod_ThrowsForNonExistentList()
    {
        _output.WriteLine("Expecting KeyNotFoundException for non-existent list");

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.AddModAsync(new AddModRequest { ModListId = 999, ModId = "333", ModUrl = "http://test.com" }));
    }
}

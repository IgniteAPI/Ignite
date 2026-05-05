using System.Text.Json;

namespace IgniteWebUI.Models.Dashboard
{
public class DashboardWidget
{
    /// <summary>Uniquely identifies this widget instance.</summary>
    public Guid InstanceId { get; set; } = Guid.NewGuid();

    /// <summary>Matches a <see cref="WidgetDescriptor.Id"/>. Not unique per instance.</summary>
    public required string WidgetId { get; set; }

    /// <summary>Zero-based column index (0–11) in the 12-column grid.</summary>
    public int Col { get; set; }

    /// <summary>Zero-based row index in the grid.</summary>
    public int Row { get; set; }

    /// <summary>Number of columns this widget spans (1–12).</summary>
    public int ColSpan { get; set; } = 6;

    /// <summary>Number of rows this widget spans.</summary>
    public int RowSpan { get; set; } = 1;

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Incremented each time config changes. Used to trigger widget re-render in Blazor.
    /// Not serialized to JSON - only used at runtime.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int ConfigVersion { get; private set; } = 0;

    /// <summary>
    /// Per-instance widget configuration. Keys and value shapes are defined by each widget.
    /// Stored as raw JSON so any value type is supported without a fixed schema.
    /// </summary>
    public Dictionary<string, JsonElement> Config { get; set; } = [];

    /// <summary>Reads a config value, falling back to <paramref name="defaultValue"/> if the key is missing or the wrong type.</summary>
    public T GetConfig<T>(string key, T defaultValue = default!)
    {
        if (Config == null || !Config.TryGetValue(key, out var element))
        {
            return defaultValue;
        }

        try 
        { 
            return JsonSerializer.Deserialize<T>(element.GetRawText())!; 
        }
        catch 
        { 
            return defaultValue; 
        }
    }

    /// <summary>Writes a config value, replacing any existing entry for <paramref name="key"/>.</summary>
    public void SetConfig<T>(string key, T value)
    {
        var element = JsonSerializer.SerializeToElement(value);
        Config[key] = element;
        ConfigVersion++; // Increment to trigger re-render
    }
}
}

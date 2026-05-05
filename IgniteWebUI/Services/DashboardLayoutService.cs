using System.Text.Json;
using System.Text.Json.Serialization;
using IgniteWebUI.Models.Dashboard;
using NLog;

namespace IgniteWebUI.Services
{
    /// <summary>
    /// Manages widget layouts for different pages/contexts, persisted server-side as JSON files.
    /// Each page/context can have its own independent layout (e.g., "dashboard", "instances", "mods").
    /// Widget availability is driven by <see cref="WidgetRegistry"/>.
    /// </summary>
    public class DashboardLayoutService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly string _dataDirectory;
        private readonly WidgetRegistry _registry;
        private readonly Dictionary<string, List<DashboardWidget>> _layouts = new();

        public List<DashboardWidget> Widgets { get; private set; } = [];

        /// <summary>The current layout context ID (e.g., "dashboard", "instances", "mods"). Defaults to "dashboard".</summary>
        public string CurrentLayoutId { get; private set; } = "dashboard";

        public DashboardLayoutService(WidgetRegistry registry)
        {
            _registry = registry;
            _dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(_dataDirectory);

            // Load the default dashboard layout
            LoadLayout("dashboard");
        }

        /// <summary>
        /// Switches to a different layout context and loads its widgets.
        /// If the layout doesn't exist yet, it will be created with default widgets on first save.
        /// </summary>
        public void SwitchLayout(string layoutId)
        {
            if (string.IsNullOrWhiteSpace(layoutId))
                layoutId = "dashboard";

            CurrentLayoutId = layoutId;
            LoadLayout(layoutId);
        }

        private string GetLayoutFilePath(string layoutId) =>
            Path.Combine(_dataDirectory, $"layout_{layoutId}.json");

        private List<DashboardWidget> GetDefaults()
        {
            var result = new List<DashboardWidget>();
            int col = 0, row = 0;
            foreach (var d in _registry.All)
            {
                // Wrap to next row if this widget won't fit on current row
                if (col + d.ColSpan > 12) { col = 0; row++; }
                var widget = new DashboardWidget
                {
                    WidgetId = d.Id,
                    Col = col, Row = row,
                    ColSpan = d.ColSpan, RowSpan = d.RowSpan,
                    Visible = true
                };
                foreach (var (key, value) in d.DefaultConfig)
                    widget.SetConfig(key, value);
                result.Add(widget);
                col += d.ColSpan;
                if (col >= 12) { col = 0; row++; }
            }
            return result;
        }

        private void LoadLayout(string layoutId)
        {
            if (_layouts.TryGetValue(layoutId, out var cached))
            {
                Widgets = cached;
                return;
            }

            try
            {
                var filePath = GetLayoutFilePath(layoutId);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var loaded = JsonSerializer.Deserialize<List<DashboardWidget>>(json, _jsonOptions);
                    if (loaded != null && loaded.Count > 0)
                    {
                        // Drop entries for widgets that are no longer registered
                        loaded = loaded.Where(w => _registry.Get(w.WidgetId) != null).ToList();

                        // Assign new InstanceId to any widget missing it (for backward compatibility)
                        foreach (var widget in loaded)
                        {
                            if (widget.InstanceId == Guid.Empty)
                                widget.InstanceId = Guid.NewGuid();
                        }

                        // Append newly registered widgets not yet in the saved layout (hidden by default)
                        foreach (var descriptor in _registry.All)
                        {
                            if (!loaded.Any(w => w.WidgetId == descriptor.Id))
                                loaded.Add(new DashboardWidget
                                {
                                    InstanceId = Guid.NewGuid(),
                                    WidgetId = descriptor.Id,
                                    Col = 0, Row = 0,
                                    ColSpan = descriptor.ColSpan, RowSpan = descriptor.RowSpan,
                                    Visible = false
                                });
                        }

                        Widgets = loaded;
                        _layouts[layoutId] = loaded;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to load layout {LayoutId}, using defaults.", layoutId);
            }

            Widgets = GetDefaults();
            _layouts[layoutId] = Widgets;
        }

        /// <summary>
        /// Saves the current layout to disk.
        /// </summary>
        public void Save()
        {
            try
            {
                var filePath = GetLayoutFilePath(CurrentLayoutId);
                var json = JsonSerializer.Serialize(Widgets, _jsonOptions);
                File.WriteAllText(filePath, json);
                _layouts[CurrentLayoutId] = Widgets;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to save layout {LayoutId}.", CurrentLayoutId);
            }
        }

        /// <summary>
        /// Reloads the current layout to pick up newly registered widgets.
        /// </summary>
        public void Reload()
        {
            _layouts.Remove(CurrentLayoutId);
            LoadLayout(CurrentLayoutId);
        }

        public void MoveWidget(Guid instanceId, int col, int row)
        {
            var widget = Widgets.FirstOrDefault(w => w.InstanceId == instanceId);
            if (widget == null) return;
            widget.Col = col;
            widget.Row = row;
        }

        /// <summary>
        /// Updates the column span (width) of a widget.
        /// </summary>
        public void UpdateWidgetColSpan(Guid instanceId, int colSpan)
        {
            var widget = Widgets.FirstOrDefault(w => w.InstanceId == instanceId);
            if (widget == null) return;
            widget.ColSpan = Math.Max(1, Math.Min(12, colSpan)); // Ensure between 1-12 columns
        }

        /// <summary>
        /// Updates the row span (height) of a widget.
        /// </summary>
        public void UpdateWidgetRowSpan(Guid instanceId, int rowSpan)
        {
            var widget = Widgets.FirstOrDefault(w => w.InstanceId == instanceId);
            if (widget == null) return;
            widget.RowSpan = Math.Max(1, rowSpan); // Ensure at least 1 row
        }

        /// <summary>
        /// Swaps two widgets' positions.
        /// </summary>
        public void SwapWidgets(Guid instanceIdA, Guid instanceIdB)
        {
            var a = Widgets.FirstOrDefault(w => w.InstanceId == instanceIdA);
            var b = Widgets.FirstOrDefault(w => w.InstanceId == instanceIdB);
            if (a == null || b == null) return;
            (a.Col, a.Row, a.ColSpan, a.RowSpan, b.Col, b.Row, b.ColSpan, b.RowSpan) =
                (b.Col, b.Row, b.ColSpan, b.RowSpan, a.Col, a.Row, a.ColSpan, a.RowSpan);
        }

        /// <summary>
        /// Adds a widget to the current layout (first available free slot, visible) if it isn't already present.
        /// </summary>
        public void AddWidget(string widgetId)
        {
            var descriptor = _registry.Get(widgetId);
            int cs = descriptor?.ColSpan ?? 6;
            int rs = descriptor?.RowSpan ?? 1;

            // Find first free slot
            var (col, row) = FindFreeSlot(cs);
            var widget = new DashboardWidget
            {
                InstanceId = Guid.NewGuid(),
                WidgetId = widgetId,
                Col = col,
                Row = row,
                ColSpan = cs,
                RowSpan = rs,
                Visible = true
            };

            if (descriptor != null)
                foreach (var (key, value) in descriptor.DefaultConfig)
                    widget.SetConfig(key, value);

            Widgets.Add(widget);
        }

        /// <summary>
        /// Removes a widget from the current layout entirely.
        /// </summary>
        public void RemoveWidget(Guid instanceId)
        {
            Widgets = Widgets.Where(w => w.InstanceId != instanceId).ToList();
        }

        /// <summary>Returns the first (col, row) position where a widget of <paramref name="colSpan"/> columns fits without overlapping.</summary>
        public (int col, int row) FindFreeSlot(int colSpan)
        {
            var occupied = new HashSet<(int, int)>();
            foreach (var w in Widgets.Where(w => w.Visible))
                for (int r = w.Row; r < w.Row + w.RowSpan; r++)
                    for (int c = w.Col; c < w.Col + w.ColSpan; c++)
                        occupied.Add((c, r));

            for (int row = 0; row < 100; row++)
                for (int col = 0; col <= 12 - colSpan; col++)
                {
                    bool fits = true;
                    for (int dc = 0; dc < colSpan && fits; dc++)
                        if (occupied.Contains((col + dc, row))) fits = false;
                    if (fits) return (col, row);
                }
            return (0, 100); // fallback
        }
    }
}

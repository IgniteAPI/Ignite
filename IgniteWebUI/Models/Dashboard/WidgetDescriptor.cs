using Microsoft.AspNetCore.Components;

namespace IgniteWebUI.Models.Dashboard
{
    /// <summary>
    /// Describes a dashboard widget that can be placed on the home page.
    /// Register instances via <see cref="IgniteWebUI.Services.WidgetRegistry"/>.
    /// </summary>
    public class WidgetDescriptor
    {
        /// <summary>Unique string identifier, e.g. "core.welcome" or "myplugin.playerstats".</summary>
        public required string Id { get; init; }

        /// <summary>Human-readable name shown in the widget panel.</summary>
        public required string DisplayName { get; init; }

        /// <summary>MudBlazor icon string shown alongside the name.</summary>
        public string Icon { get; init; } = MudBlazor.Icons.Material.Filled.Widgets;

        /// <summary>How many of the 12 grid columns this widget occupies by default.</summary>
        public int ColSpan { get; init; } = 6;

        /// <summary>How many grid rows this widget occupies by default.</summary>
        public int RowSpan { get; init; } = 1;

        /// <summary>Factory that produces the rendered widget content.</summary>
        public required Func<RenderFragment> Content { get; init; }

        /// <summary>
        /// Optional factory that produces the configuration UI for this widget.
        /// The config component receives the DashboardWidget instance as a cascading parameter.
        /// Return null if the widget doesn't support configuration.
        /// </summary>
        public Func<DashboardWidget, RenderFragment>? ConfigComponent { get; set; }

        /// <summary>
        /// Default config values applied when the widget is first added to a layout.
        /// Keys should match what the widget reads via <see cref="DashboardWidget.GetConfig{T}"/>.
        /// </summary>
        public Dictionary<string, object> DefaultConfig { get; init; } = [];
    }
}

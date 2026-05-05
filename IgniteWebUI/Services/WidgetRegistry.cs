using System.Collections.Concurrent;
using IgniteWebUI.Models.Dashboard;

namespace IgniteWebUI.Services
{
    /// <summary>
    /// Central registry of all available dashboard widgets.
    /// Core widgets are registered at startup; plugins register additional widgets at runtime.
    /// </summary>
    public class WidgetRegistry
    {
        private readonly ConcurrentDictionary<string, WidgetDescriptor> _widgets = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>All currently registered widget descriptors in registration order.</summary>
        public IReadOnlyList<WidgetDescriptor> All => _widgets.Values.ToList();

        /// <summary>
        /// Registers a widget. If a widget with the same Id is already registered, it is replaced.
        /// Safe to call at any time, including from plugin initializers after startup.
        /// </summary>
        public void Register(WidgetDescriptor descriptor)
        {
            _widgets[descriptor.Id] = descriptor;
        }

        /// <summary>Returns the descriptor for the given id, or null if not found.</summary>
        public WidgetDescriptor? Get(string id) =>
            _widgets.TryGetValue(id, out var d) ? d : null;
    }
}

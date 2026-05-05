using System;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using IgniteWebUI.Models.Dashboard;
using IgniteWebUI.Attributes;

namespace IgniteWebUI.Services
{
    /// <summary>
    /// Extension methods for auto-discovering and registering widgets from attributes.
    /// </summary>
    public static class WidgetRegistryExtensions
    {
        /// <summary>
        /// Auto-discovers all Blazor components decorated with [WidgetDescriptor] and registers them.
        /// </summary>
        public static void AutoRegisterWidgets(this WidgetRegistry registry, Assembly assembly)
        {
            var widgetTypes = assembly.GetTypes()
                .Where(t => typeof(ComponentBase).IsAssignableFrom(t)
                    && !t.IsAbstract
                    && t.GetCustomAttribute<WidgetDescriptorAttribute>() != null);

            foreach (var type in widgetTypes)
            {
                var attr = type.GetCustomAttribute<WidgetDescriptorAttribute>()!;

                var descriptor = new WidgetDescriptor
                {
                    Id = attr.Id,
                    DisplayName = attr.DisplayName,
                    Icon = attr.Icon ?? MudBlazor.Icons.Material.Filled.Widgets,
                    ColSpan = attr.ColSpan,
                    RowSpan = attr.RowSpan,
                    Content = () => b => { b.OpenComponent(0, type); b.CloseComponent(); }
                };

                // If widget has config, try to find and register the config component
                if (attr.HasConfig)
                {
                    // Look for a config component with convention: {ComponentName}Config
                    var configComponentName = type.Name + "Config";
                    var configType = assembly.GetType(type.Namespace + "." + configComponentName);

                    if (configType != null && typeof(ComponentBase).IsAssignableFrom(configType))
                    {
                        descriptor.ConfigComponent = widget => b =>
                        {
                            b.OpenComponent(0, configType);
                            b.AddAttribute(1, "Widget", widget);
                            b.CloseComponent();
                        };
                    }
                }

                registry.Register(descriptor);
            }
        }
    }
}

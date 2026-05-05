using System;

namespace IgniteWebUI.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WidgetDescriptorAttribute : Attribute
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string? Icon { get; set; }
        public int ColSpan { get; set; } = 6;
        public int RowSpan { get; set; } = 1;
        public bool HasConfig { get; set; }

        public WidgetDescriptorAttribute(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}

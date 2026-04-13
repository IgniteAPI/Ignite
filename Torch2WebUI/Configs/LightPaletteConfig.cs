using Torch2API.Attributes;
using YamlDotNet.Serialization;

namespace Torch2WebUI.Configs
{
    /// <summary>
    /// Configuration for the light theme color palette.
    /// </summary>
    public class LightPaletteConfig
    {
        [YamlMember(Description = "Black color")]
        public string Black { get; set; } = "#110e2d";

        [YamlMember(Description = "Primary color")]
        public string Primary { get; set; } = "#e1824bff";

        [YamlMember(Description = "Surface color")]
        public string Surface { get; set; } = "#f5f5f5";

        [YamlMember(Description = "Background color")]
        public string Background { get; set; } = "#fafafa";

        [YamlMember(Description = "Background gray color")]
        public string BackgroundGray { get; set; } = "#eeeeee";

        [YamlMember(Description = "App bar text color")]
        public string AppbarText { get; set; } = "#424242";

        [YamlMember(Description = "App bar background color")]
        public string AppbarBackground { get; set; } = "rgba(255,255,255,0.8)";

        [YamlMember(Description = "Drawer background color")]
        public string DrawerBackground { get; set; } = "#ffffff";

        [YamlMember(Description = "Default action color")]
        public string ActionDefault { get; set; } = "#757575";

        [YamlMember(Description = "Disabled action color")]
        public string ActionDisabled { get; set; } = "#bdbdbd4d";

        [YamlMember(Description = "Disabled action background color")]
        public string ActionDisabledBackground { get; set; } = "#f5f5f54d";

        [YamlMember(Description = "Primary text color")]
        public string TextPrimary { get; set; } = "#212121";

        [YamlMember(Description = "Secondary text color")]
        public string TextSecondary { get; set; } = "#757575";

        [YamlMember(Description = "Disabled text color")]
        public string TextDisabled { get; set; } = "#bdbdbd";

        [YamlMember(Description = "Drawer icon color")]
        public string DrawerIcon { get; set; } = "#757575";

        [YamlMember(Description = "Drawer text color")]
        public string DrawerText { get; set; } = "#757575";

        [YamlMember(Description = "Light gray color")]
        public string GrayLight { get; set; } = "#e8e8e8";

        [YamlMember(Description = "Lighter gray color")]
        public string GrayLighter { get; set; } = "#f9f9f9";

        [YamlMember(Description = "Info color")]
        public string Info { get; set; } = "#1976d2";

        [YamlMember(Description = "Success color")]
        public string Success { get; set; } = "#388e3c";

        [YamlMember(Description = "Warning color")]
        public string Warning { get; set; } = "#f57c00";

        [YamlMember(Description = "Error color")]
        public string Error { get; set; } = "#d32f2f";

        [YamlMember(Description = "Default lines color")]
        public string LinesDefault { get; set; } = "#e0e0e0";

        [YamlMember(Description = "Table lines color")]
        public string TableLines { get; set; } = "#e0e0e0";

        [YamlMember(Description = "Divider color")]
        public string Divider { get; set; } = "#e0e0e0";

        [YamlMember(Description = "Overlay light color")]
        public string OverlayLight { get; set; } = "#f5f5f580";
    }
}

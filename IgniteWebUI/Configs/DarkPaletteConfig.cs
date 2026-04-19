using IgniteAPI.Attributes;
using YamlDotNet.Serialization;

namespace IgniteWebUI.Configs
{
    /// <summary>
    /// Configuration for the dark theme color palette.
    /// </summary>
    public class DarkPaletteConfig
    {
        [YamlMember(Description = "Primary color")]
        public string Primary { get; set; } = "#e1824bff";

        [YamlMember(Description = "Surface color")]
        public string Surface { get; set; } = "#1e1e2d";

        [YamlMember(Description = "Background color")]
        public string Background { get; set; } = "#1e1e28ff";

        [YamlMember(Description = "Background gray color")]
        public string BackgroundGray { get; set; } = "#151521";

        [YamlMember(Description = "App bar text color")]
        public string AppbarText { get; set; } = "#92929f";

        [YamlMember(Description = "App bar background color")]
        public string AppbarBackground { get; set; } = "rgba(26,26,39,0.8)";

        [YamlMember(Description = "Drawer background color")]
        public string DrawerBackground { get; set; } = "#1a1a27";

        [YamlMember(Description = "Default action color")]
        public string ActionDefault { get; set; } = "#74718e";

        [YamlMember(Description = "Disabled action color")]
        public string ActionDisabled { get; set; } = "#9999994d";

        [YamlMember(Description = "Disabled action background color")]
        public string ActionDisabledBackground { get; set; } = "#605f6d4d";

        [YamlMember(Description = "Primary text color")]
        public string TextPrimary { get; set; } = "#b2b0bf";

        [YamlMember(Description = "Secondary text color")]
        public string TextSecondary { get; set; } = "#92929f";

        [YamlMember(Description = "Disabled text color")]
        public string TextDisabled { get; set; } = "#ffffff33";

        [YamlMember(Description = "Drawer icon color")]
        public string DrawerIcon { get; set; } = "#92929f";

        [YamlMember(Description = "Drawer text color")]
        public string DrawerText { get; set; } = "#92929f";

        [YamlMember(Description = "Light gray color")]
        public string GrayLight { get; set; } = "#2a2833";

        [YamlMember(Description = "Lighter gray color")]
        public string GrayLighter { get; set; } = "#1e1e2d";

        [YamlMember(Description = "Info color")]
        public string Info { get; set; } = "#4a86ff";

        [YamlMember(Description = "Success color")]
        public string Success { get; set; } = "#3dcb6c";

        [YamlMember(Description = "Warning color")]
        public string Warning { get; set; } = "#ffb545";

        [YamlMember(Description = "Error color")]
        public string Error { get; set; } = "#ff3f5f";

        [YamlMember(Description = "Default lines color")]
        public string LinesDefault { get; set; } = "#33323e";

        [YamlMember(Description = "Table lines color")]
        public string TableLines { get; set; } = "#33323e";

        [YamlMember(Description = "Divider color")]
        public string Divider { get; set; } = "#292838";

        [YamlMember(Description = "Overlay light color")]
        public string OverlayLight { get; set; } = "#1e1e2d80";
    }
}

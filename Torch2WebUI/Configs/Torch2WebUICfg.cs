using Torch2API.Attributes;
using Torch2API.Utils;
using YamlDotNet.Serialization;

namespace Torch2WebUI.Configs
{
    public class Torch2WebUICfg : ConfigBase<Torch2WebUICfg>
    {
        [EnvVar("TORCH2_PANEL_NAME")]
        [YamlMember(Description = "Name of the Web UI Panel")]
        public string PanelName { get; set; } = "Torch2 Web UI";

        [EnvVar("TORCH2_WEB_PORT")]
        [YamlMember(Description = "Web UI Port")]
        public int Port { get; set; } = 7076;

        [YamlMember(Description = "Logging Configuration")]
        public LoggingConfig Logging { get; set; } = new LoggingConfig();

        [YamlMember(Description = "Dark Theme Palette Configuration")]
        public DarkPaletteConfig DarkPalette { get; set; } = new DarkPaletteConfig();

        [YamlMember(Description = "Light Theme Palette Configuration")]
        public LightPaletteConfig LightPalette { get; set; } = new LightPaletteConfig();
    }
}

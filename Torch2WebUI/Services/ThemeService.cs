using MudBlazor;
using MudBlazor.Utilities;
using Torch2WebUI.Configs;

namespace Torch2WebUI.Services
{
    public class ThemeService
    {
        private readonly Torch2WebUICfg _config;

        public MudTheme? CurrentTheme { get; private set; }

        public bool IsDarkMode { get; set; } = true;

        public event Func<Task>? OnThemeChanged;

        public ThemeService(Torch2WebUICfg config)
        {
            _config = config;
            CreateTheme();
        }

        public void CreateTheme()
        {
            var darkPalette = CreatePaletteDarkFromConfig(_config.DarkPalette);
            var lightPalette = CreatePaletteLightFromConfig(_config.LightPalette);

            CurrentTheme = new MudTheme()
            {
                PaletteLight = lightPalette,
                PaletteDark = darkPalette,
                LayoutProperties = new LayoutProperties()
                {
                    DefaultBorderRadius = "6px"
                }
            };
        }

        public void UpdateTheme()
        {
            var darkPalette = CreatePaletteDarkFromConfig(_config.DarkPalette);
            var lightPalette = CreatePaletteLightFromConfig(_config.LightPalette);

            var newTheme = new MudTheme()
            {
                PaletteLight = lightPalette,
                PaletteDark = darkPalette,
                LayoutProperties = new LayoutProperties()
                {
                    DefaultBorderRadius = "6px"
                }
            };
            CurrentTheme = newTheme;

            if (OnThemeChanged != null)
            {
                foreach (Func<Task> handler in OnThemeChanged.GetInvocationList())
                {
                    _ = handler();
                }
            }
        }

        private PaletteDark CreatePaletteDarkFromConfig(DarkPaletteConfig config)
        {
            return new PaletteDark()
            {
                Primary = config.Primary,
                Surface = config.Surface,
                Background = config.Background,
                BackgroundGray = config.BackgroundGray,
                AppbarText = config.AppbarText,
                AppbarBackground = config.AppbarBackground,
                DrawerBackground = config.DrawerBackground,
                ActionDefault = config.ActionDefault,
                ActionDisabled = config.ActionDisabled,
                ActionDisabledBackground = config.ActionDisabledBackground,
                TextPrimary = config.TextPrimary,
                TextSecondary = config.TextSecondary,
                TextDisabled = config.TextDisabled,
                DrawerIcon = config.DrawerIcon,
                DrawerText = config.DrawerText,
                GrayLight = config.GrayLight,
                GrayLighter = config.GrayLighter,
                Info = config.Info,
                Success = config.Success,
                Warning = config.Warning,
                Error = config.Error,
                LinesDefault = config.LinesDefault,
                TableLines = config.TableLines,
                Divider = config.Divider,
                OverlayLight = config.OverlayLight
            };
        }

        private PaletteLight CreatePaletteLightFromConfig(LightPaletteConfig config)
        {
            return new PaletteLight()
            {
                Black = config.Black,
                Primary = config.Primary,
                Surface = config.Surface,
                Background = config.Background,
                BackgroundGray = config.BackgroundGray,
                AppbarText = config.AppbarText,
                AppbarBackground = config.AppbarBackground,
                DrawerBackground = config.DrawerBackground,
                ActionDefault = config.ActionDefault,
                ActionDisabled = config.ActionDisabled,
                ActionDisabledBackground = config.ActionDisabledBackground,
                TextPrimary = config.TextPrimary,
                TextSecondary = config.TextSecondary,
                TextDisabled = config.TextDisabled,
                DrawerIcon = config.DrawerIcon,
                DrawerText = config.DrawerText,
                GrayLight = config.GrayLight,
                GrayLighter = config.GrayLighter,
                Info = config.Info,
                Success = config.Success,
                Warning = config.Warning,
                Error = config.Error,
                LinesDefault = config.LinesDefault,
                TableLines = config.TableLines,
                Divider = config.Divider,
                OverlayLight = config.OverlayLight
            };
        }
    }
}

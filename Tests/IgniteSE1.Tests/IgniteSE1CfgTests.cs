using IgniteSE1.Configs;
using Xunit;

namespace IgniteSE1.Tests
{
    public class IgniteSE1CfgTests
    {
        private readonly ITestOutputHelper _output;

        public IgniteSE1CfgTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Verifies that a default-constructed IgniteSE1Cfg has the expected values
        /// for IgniteCMDName, AutoStartServer, TargetInstance, and ProtoServerPort.
        /// </summary>
        [Fact]
        public void DefaultValues_AreCorrect()
        {
            var cfg = new IgniteSE1Cfg();

            _output.WriteLine($"IgniteCMDName: {cfg.IgniteCMDName}");
            _output.WriteLine($"AutoStartServer: {cfg.AutoStartServer}");
            _output.WriteLine($"TargetInstance: {cfg.TargetInstance}");
            _output.WriteLine($"ProtoServerPort: {cfg.ProtoServerPort}");

            Assert.Equal("Ignite SE1", cfg.IgniteCMDName);
            Assert.True(cfg.AutoStartServer);
            Assert.Equal("MyNewIgniteInstance", cfg.TargetInstance);
            Assert.Equal(4800, cfg.ProtoServerPort);
        }

        /// <summary>
        /// Verifies that the nested DirectoriesConfig has correct default paths for
        /// SteamCMD, Game, Mods, Profiles, and Worlds directories.
        /// </summary>
        [Fact]
        public void DirectoriesConfig_HasDefaults()
        {
            var cfg = new IgniteSE1Cfg();

            _output.WriteLine($"SteamCMDFolder: {cfg.Directories.SteamCMDFolder}");
            _output.WriteLine($"Game: {cfg.Directories.Game}");
            _output.WriteLine($"ModStorage: {cfg.Directories.ModStorage}");
            _output.WriteLine($"ProfileDir: {cfg.Directories.ProfileDir}");
            _output.WriteLine($"WorldsDir: {cfg.Directories.WorldsDir}");

            Assert.NotNull(cfg.Directories);
            Assert.Equal("SteamCMD", cfg.Directories.SteamCMDFolder);
            Assert.Equal("Game", cfg.Directories.Game);
            Assert.Equal("Mods", cfg.Directories.ModStorage);
            Assert.Equal("Profiles", cfg.Directories.ProfileDir);
            Assert.Equal("Worlds", cfg.Directories.WorldsDir);
        }

        /// <summary>
        /// Verifies that the default WebServerAddress uses HTTPS.
        /// </summary>
        [Fact]
        public void WebServerAddress_DefaultIsHttps()
        {
            var cfg = new IgniteSE1Cfg();

            _output.WriteLine($"WebServerAddress: {cfg.WebServerAddress}");

            Assert.StartsWith("https://", cfg.WebServerAddress);
        }
    }
}

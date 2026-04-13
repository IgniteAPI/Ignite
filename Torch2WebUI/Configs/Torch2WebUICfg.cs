using Torch2API.Attributes;
using Torch2API.Utils;
using YamlDotNet.Serialization;

namespace Torch2WebUI.Configs
{
    public class Torch2WebUICfg : ConfigBase<Torch2WebUICfg>
    {
        #region Yaml Groups

        public class LoggingConfig
        {
            [EnvVar("TORCH2_LOG_ENABLE_FILE")]
            [YamlMember(Description = "Enable file logging")]
            public bool EnableFileLogging { get; set; } = true;

            [EnvVar("TORCH2_LOG_DIRECTORY")]
            [YamlMember(Description = "Log file directory path")]
            public string LogDirectory { get; set; } = "Logs";

            [EnvVar("TORCH2_LOG_MAX_AGE_DAYS")]
            [YamlMember(Description = "Maximum age of log files in days before deletion")]
            public int MaxLogAgeDays { get; set; } = 30;

            [EnvVar("TORCH2_LOG_LEVEL")]
            [YamlMember(Description = "Log level (Trace, Debug, Information, Warning, Error, Critical)")]
            public string LogLevel { get; set; } = "Information";

            [EnvVar("TORCH2_LOG_INSTANCES_CONSOLE")]
            [YamlMember(Description = "Log instances logs to console")]
            public bool EnableInstanceLogging { get; set; } = true;

            [EnvVar("TORCH2_LOG_INSTANCES_MAX_ENTRIES")]
            [YamlMember(Description = "Maximum number of log entries to keep for each instance in memory")]
            public int InstanceLogViewerMaxEntries { get; set; } = 2000;


            [EnvVar("TORCH2_LOG_CHAT_MAX_ENTRIES")]
            [YamlMember(Description = "Maximum number of chat entries to keep for each instance in memory")]
            public int InstanceChatViewerMaxEntries { get; set; } = 1000;

        }

        #endregion


        [EnvVar("TORCH2_PANEL_NAME")]
        [YamlMember(Description = "Name of the Web UI Panel")]
        public string PanelName { get; set; } = "Torch2 Web UI";

        [EnvVar("TORCH2_WEB_PORT")]
        [YamlMember(Description = "Web UI Port")]
        public int Port { get; set; } = 7076;

        [YamlMember(Description = "Logging Configuration")]
        public LoggingConfig Logging { get; set; } = new LoggingConfig();
    }
}

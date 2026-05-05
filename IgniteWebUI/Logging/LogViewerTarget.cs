using IgniteAPI.DTOs.Logs;
using IgniteWebUI.Services.InstanceServices;
using NLog;
using NLog.Targets;

namespace IgniteWebUI.Logging
{
    [Target("LogViewer")]
    public class LogViewerTarget : TargetWithLayout
    {
        public const string WebUIInstanceId = "webui";
        public const string WebUIInstanceName = "Web UI";

        private static InstanceLogService? _logService;

        public static void Register(InstanceLogService logService)
        {
            _logService = logService;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (_logService is null)
                return;

            var entry = new LogLine
            {
                InstanceName = WebUIInstanceName,
                LoggerName   = logEvent.LoggerName,
                Level        = MapLevel(logEvent.Level),
                Message      = logEvent.FormattedMessage,
                Timestamp    = logEvent.TimeStamp.ToUniversalTime(),
            };

            if (logEvent.Exception is not null)
                entry.Message += Environment.NewLine + logEvent.Exception.ToString();

            _logService.Append(WebUIInstanceId, entry, WebUIInstanceName);
        }

        private static string MapLevel(NLog.LogLevel level)
        {
            if (level == NLog.LogLevel.Trace) return "Trace";
            if (level == NLog.LogLevel.Debug) return "Debug";
            if (level == NLog.LogLevel.Info)  return "Info";
            if (level == NLog.LogLevel.Warn)  return "Warn";
            if (level == NLog.LogLevel.Error) return "Error";
            return "Fatal";
        }
    }
}

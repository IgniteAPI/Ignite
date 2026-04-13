using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using NLog;
using Torch2API.Constants;
using Torch2API.DTOs.Logs;
using Torch2API.DTOs.WebSockets;
using Torch2WebUI.Configs;

namespace Torch2WebUI.Services.InstanceServices
{
    /// <summary>
    /// Panel-side log store. Keeps a rolling history per instance and notifies
    /// Blazor components via <see cref="OnLog"/> when new entries arrive.
    /// </summary>
    public class InstanceLogService : ISocketMessageHandler
    {

        private readonly ConcurrentDictionary<string, Queue<LogLine>> _histories = new();
        private readonly object _lock = new();
        private readonly Torch2WebUICfg _webConfig;
        private readonly InstanceManager _instanceManager;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public int MaxPerInstance => _webConfig.Logging.InstanceLogViewerMaxEntries;

        public IReadOnlyList<string> HandledCommands { get; } = [TorchConstants.WsLog, TorchConstants.WsLogHistory];

        /// <summary>Raised on the thread that appended the entry: (instanceId, entry).</summary>
        public event Action<string, LogLine>? OnLog;


        public InstanceLogService(Torch2WebUICfg webConfig, InstanceManager instanceManager)
        {
            _webConfig = webConfig;
            _instanceManager = instanceManager;
        }

        public void Append(string instanceId, LogLine entry, string? instanceName = null)
        {
            lock (_lock)
            {
                var q = _histories.GetOrAdd(instanceId, _ => new Queue<LogLine>(MaxPerInstance));
                q.Enqueue(entry);
                if (q.Count > MaxPerInstance)
                    q.Dequeue();
            }

            // Write to NLog file if enabled in config
            if (_webConfig.Logging.EnableInstanceLogging && instanceName is not null)
            {
                var logLevel = NLog.LogLevel.FromString(entry.Level ?? "Information");
                _logger.Log(logLevel, $"[{instanceName}] {entry.Message}");
            }

            OnLog?.Invoke(instanceId, entry);
        }

        public void AppendHistory(string instanceId, IEnumerable<LogLine> entries)
        {
            lock (_lock)
            {
                var q = _histories.GetOrAdd(instanceId, _ => new Queue<LogLine>(MaxPerInstance));
                foreach (var entry in entries)
                {
                    q.Enqueue(entry);
                    if (q.Count > MaxPerInstance)
                        q.Dequeue();
                }
            }
        }

        public LogLine[] GetHistory(string instanceId)
        {
            lock (_lock)
            {
                return _histories.TryGetValue(instanceId, out var q)
                    ? q.ToArray()
                    : Array.Empty<LogLine>();
            }
        }

        public void Handle(string instanceId, SocketMsgEnvelope envelope)
        {
            switch (envelope.Command)
            {
                case TorchConstants.WsLog:
                    var entry = envelope.Args.Deserialize<LogLine>(TorchConstants.JsonOptions);
                    if (entry is not null)
                        Append(instanceId, entry, _instanceManager.GetInstanceName(instanceId));
                    break;

                case TorchConstants.WsLogHistory:
                    var history = envelope.Args.Deserialize<LogLine[]>(TorchConstants.JsonOptions);
                    if (history is not null)
                        AppendHistory(instanceId, history);
                    break;
            }
        }
    }
}

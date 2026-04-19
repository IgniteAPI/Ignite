using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NLog;
using IgniteAPI.DTOs.Chat;
using IgniteWebUI.Configs;

namespace IgniteWebUI.Services.InstanceServices
{
    /// <summary>
    /// Panel-side chat store. Keeps a rolling history per instance and notifies
    /// Blazor components via <see cref="OnChat"/> when new messages arrive.
    /// </summary>
    public class InstanceChatService
    {

        private readonly ConcurrentDictionary<string, Queue<ChatMessage>> _histories = new();
        private readonly object _lock = new();
        private readonly IgniteWebUICfg _webConfig;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public int MaxPerInstance => _webConfig.Logging.InstanceChatViewerMaxEntries;

        /// <summary>Raised when a new chat message is appended: (instanceId, message).</summary>
        public event Action<string, ChatMessage>? OnChat;
        


        public InstanceChatService(IgniteWebUICfg webConfig) { _webConfig = webConfig; }

        public void Append(string instanceId, ChatMessage message, string? instanceName = null)
        {
            if (instanceName is not null)
                message.InstanceName = instanceName;

            lock (_lock)
            {
                var q = _histories.GetOrAdd(instanceId, _ => new Queue<ChatMessage>(MaxPerInstance));
                q.Enqueue(message);
                if (q.Count > MaxPerInstance)
                    q.Dequeue();
            }

            if (_webConfig.Logging.EnableInstanceLogging && instanceName is not null)
            {
                _logger.Info($"[{instanceName}] {message.DisplayName}: {message.Message}");
            }

            OnChat?.Invoke(instanceId, message);
        }

        public void AppendHistory(string instanceId, IEnumerable<ChatMessage> messages)
        {
            lock (_lock)
            {
                var q = _histories.GetOrAdd(instanceId, _ => new Queue<ChatMessage>(MaxPerInstance));
                foreach (var msg in messages)
                {
                    q.Enqueue(msg);
                    if (q.Count > MaxPerInstance)
                        q.Dequeue();
                }
            }
        }

        public ChatMessage[] GetHistory(string instanceId)
        {
            lock (_lock)
            {
                return _histories.TryGetValue(instanceId, out var q)
                    ? q.ToArray()
                    : Array.Empty<ChatMessage>();
            }
        }

        public ChatMessage[] GetAllHistory()
        {
            lock (_lock)
            {
                var allMessages = new List<ChatMessage>();
                foreach (var queue in _histories.Values)
                {
                    allMessages.AddRange(queue);
                }
                // Sort by timestamp to maintain chronological order
                return allMessages.OrderBy(m => m.Timestamp).ToArray();
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Torch2API.DTOs.Chat;
using Torch2WebUI.Configs;

namespace Torch2WebUI.Services.InstanceServices
{
    /// <summary>
    /// Panel-side chat store. Keeps a rolling history per instance and notifies
    /// Blazor components via <see cref="OnChat"/> when new messages arrive.
    /// </summary>
    public class InstanceChatService
    {

        private readonly ConcurrentDictionary<string, Queue<ChatMessage>> _histories = new();
        private readonly object _lock = new();
        private readonly Torch2WebUICfg _webConfig;
        public int MaxPerInstance => _webConfig.Logging.InstanceChatViewerMaxEntries;

        /// <summary>Raised when a new chat message is appended: (instanceId, message).</summary>
        public event Action<string, ChatMessage>? OnChat;
        


        public InstanceChatService(Torch2WebUICfg webConfig) { _webConfig = webConfig; }

        public void Append(string instanceId, ChatMessage message)
        {
            lock (_lock)
            {
                var q = _histories.GetOrAdd(instanceId, _ => new Queue<ChatMessage>(MaxPerInstance));
                q.Enqueue(message);
                if (q.Count > MaxPerInstance)
                    q.Dequeue();
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
    }
}

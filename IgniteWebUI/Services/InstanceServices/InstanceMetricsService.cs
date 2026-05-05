using System;
using System.Collections.Concurrent;
using System.Linq;
using IgniteAPI.DTOs.Instances;

namespace IgniteWebUI.Services.InstanceServices
{
    /// <summary>
    /// Tracks instance metrics over time for analytics and graphing.
    /// </summary>
    public class InstanceMetricsService
    {
        private const int MaxDataPoints = 300; // Keep 5 minutes of data at 1-second intervals, or 5 hours at 60-second intervals
        private readonly ConcurrentDictionary<string, Queue<InstanceMetricSnapshot>> _metricsHistory = new();
        private readonly object _lockObject = new object();

        public class InstanceMetricSnapshot
        {
            public DateTime Timestamp { get; set; }
            public float SimSpeed { get; set; }
            public ushort PlayersOnline { get; set; }
            public uint TotalGrids { get; set; }
        }

        /// <summary>
        /// Records a snapshot of instance metrics at the current time.
        /// </summary>
        public void RecordMetrics(string instanceId, TorchInstanceBase instance)
        {
            if (instance == null) return;

            var snapshot = new InstanceMetricSnapshot
            {
                Timestamp = DateTime.UtcNow,
                SimSpeed = instance.SimSpeed,
                PlayersOnline = instance.PlayersOnline,
                TotalGrids = instance.TotalGrids
            };

            var queue = _metricsHistory.GetOrAdd(instanceId, _ => new Queue<InstanceMetricSnapshot>());

            lock (_lockObject)
            {
                queue.Enqueue(snapshot);
                while (queue.Count > MaxDataPoints)
                {
                    queue.Dequeue();
                }
            }
        }

        /// <summary>
        /// Retrieves the metric history for a specific instance.
        /// </summary>
        public List<InstanceMetricSnapshot> GetMetricsHistory(string instanceId, int maxPoints = MaxDataPoints)
        {
            if (!_metricsHistory.TryGetValue(instanceId, out var queue))
                return new List<InstanceMetricSnapshot>();

            lock (_lockObject)
            {
                return queue.TakeLast(maxPoints).ToList();
            }
        }

        /// <summary>
        /// Clears all metrics history for a specific instance.
        /// </summary>
        public void ClearMetricsHistory(string instanceId)
        {
            _metricsHistory.TryRemove(instanceId, out _);
        }

        /// <summary>
        /// Clears all metrics history.
        /// </summary>
        public void ClearAllMetrics()
        {
            _metricsHistory.Clear();
        }

        /// <summary>
        /// Gets aggregate statistics for a time period.
        /// </summary>
        public (float AvgSimSpeed, ushort MaxPlayers, uint MaxGrids) GetAggregateStats(
            string instanceId, 
            TimeSpan? timeWindow = null)
        {
            var history = GetMetricsHistory(instanceId);
            if (history.Count == 0)
                return (0, 0, 0);

            var cutoffTime = timeWindow.HasValue ? DateTime.UtcNow - timeWindow.Value : DateTime.MinValue;
            var filtered = history.Where(h => h.Timestamp >= cutoffTime).ToList();

            if (filtered.Count == 0)
                return (0, 0, 0);

            return (
                (float)filtered.Average(h => h.SimSpeed),
                filtered.Max(h => h.PlayersOnline),
                filtered.Max(h => h.TotalGrids)
            );
        }
    }
}

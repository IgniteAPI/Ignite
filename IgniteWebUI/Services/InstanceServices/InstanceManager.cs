using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Diagnostics.Eventing.Reader;
using IgniteAPI.DTOs.Instances;
using IgniteAPI.Models;
using IgniteAPI.Models.Configs;
using IgniteAPI.Models.Schema;
using IgniteWebUI.Components.Pages;
using IgniteWebUI.Configs;
using IgniteWebUI.Models;
using IgniteWebUI.Services.SQL;
using Timer = System.Timers.Timer;
using IgniteAPI.Models.SE1;

namespace IgniteWebUI.Services.InstanceServices
{
    public class InstanceManager
    {
        public ConcurrentDictionary<string, TorchInstance> ActiveInstances { get; private set; } = new();

        private readonly TimeSpan _timeout;
        private readonly Timer CleanupTimer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private InstanceMetricsService? _metricsService;

        public event Action<string>? OnChange;
        private void NotifyStateChanged(string instanceid) => OnChange?.Invoke(instanceid);

        //Do not need to notify the page when its a bind
        public bool EnableServerDiscovery { get; set; } = false;

        public InstanceManager(IServiceScopeFactory scopeFactory, IMemoryCache cache, IgniteWebUICfg config)
        {
            _cache = cache;
            _scopeFactory = scopeFactory;
            _timeout = TimeSpan.FromSeconds(config.Network.InstanceTimeoutSeconds);
            CleanupTimer = new Timer(_timeout.Add(TimeSpan.FromSeconds(2)));
            CleanupTimer.AutoReset = false;

            CleanupTimer.Elapsed += CleanupTimer_Elapsed;
            CleanupTimer.Start();

            // Load configured instances from database (fire and forget)
            Task.Run(() => LoadConfiguredInstancesAsync());
        }

        public void SetMetricsService(InstanceMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        private async Task LoadConfiguredInstancesAsync()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var configuredInstances = await dbContext.ConfiguredInstances.ToListAsync<ConfiguredInstance>();

                    // Load configured instances into ActiveInstances so they display even when offline
                    foreach (var configuredInstance in configuredInstances)
                    {
                        TorchInstance inst = new TorchInstance();
                        inst.InstanceID = configuredInstance.InstanceID;
                        inst.Name = configuredInstance.Name;
                        inst.MachineName = configuredInstance.MachineName;
                        inst.IPAddress = configuredInstance.IPAddress;
                        inst.GamePort = configuredInstance.GamePort;
                        inst.ProfileName = configuredInstance.ProfileName;
                        inst.TargetWorld = configuredInstance.TargetWorld;
                        inst.TorchVersion = configuredInstance.TorchVersion;
                        inst.Configured = true;
                        inst.IsOnline = false;
                        inst.ServerStatus = ServerStatusEnum.Offline;

                        ActiveInstances.TryAdd(configuredInstance.InstanceID, inst);
                    }

                    // Store in cache for quick access
                    _cache.Set("ConfiguredInstanceIDs", configuredInstances.Select(x => x.InstanceID).ToList());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configured instances: {ex.Message}");
            }
        }

        private void CleanupTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CleanupInstances();
            CleanupTimer.Start();
        }

        private void CleanupInstances()
        {
        }

        public void UpdateStatus(TorchInstanceBase instance)
        {
            if (!RegisterInstance(instance))
                return;

            // Update last seen or other status info here
            if (ActiveInstances.ContainsKey(instance.InstanceID))
            {
                ActiveInstances[instance.InstanceID].UpdateFromConfiguredInstance(instance);
                // Record metrics for analytics
                _metricsService?.RecordMetrics(instance.InstanceID, instance);
            }

            NotifyStateChanged(instance.InstanceID);
            return;
        }

        public bool RegisterInstance(TorchInstanceBase instance)
        {
            if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceID))
                return false;

            instance.LastUpdate = DateTime.UtcNow;
            if (ActiveInstances.ContainsKey(instance.InstanceID))
                return true;

            using (var scope = _scopeFactory.CreateScope())
            {
                var _database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (_database.ConfiguredInstances.Any(i => i.InstanceID == instance.InstanceID))
                {

                    TorchInstance inst = new TorchInstance();
                    inst.UpdateFromConfiguredInstance(instance);
                    inst.Configured = true;

                    ActiveInstances.TryAdd(instance.InstanceID, inst);
                    NotifyStateChanged(instance.InstanceID);
                    return true;
                }
            }

            if (EnableServerDiscovery)
            {
                TorchInstance inst = new TorchInstance();
                inst.UpdateFromConfiguredInstance(instance);

                ActiveInstances.TryAdd(instance.InstanceID, inst);
                NotifyStateChanged(instance.InstanceID);
                return true;
            }

            return false;
        }

        public bool UpdateProfiles(string? instanceid, List<ProfileCfg> profileCfgs)
        {
            if (string.IsNullOrWhiteSpace(instanceid))
                return false;

            if (ActiveInstances.TryGetValue(instanceid, out var instance))
            {
                instance.Profiles = profileCfgs;
                NotifyStateChanged(instance.InstanceID);
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool UpdateWorlds(string? instanceid, List<WorldInfo> worlds, bool isCustomWorlds = false)
        {
            if (string.IsNullOrWhiteSpace(instanceid))
                return false;

            if (ActiveInstances.TryGetValue(instanceid, out var instance))
            {
                if (isCustomWorlds)
                {
                    instance.CustomWorlds = worlds;
                }
                else
                {
                    instance.WorldInfos = worlds;
                }

                NotifyStateChanged(instance.InstanceID);
            }
            else
            {
                return false;
            }

            return true;
        }

        // New: accept the full config DTO and store it on the instance model
        public bool UpdateDedicatedConfig(string? instanceid, ConfigDedicatedSE1 config)
        {
            if (string.IsNullOrWhiteSpace(instanceid) || config == null)
                return false;

            if (ActiveInstances.TryGetValue(instanceid, out var instance))
            {
                instance.DedicatedConfig = config;
                NotifyStateChanged(instance.InstanceID);
                return true;
            }

            return false;
        }

        public TorchInstance? GetInstanceByID(string instanceID)
        {
            if (string.IsNullOrWhiteSpace(instanceID))
                return null;

            // 2?? Short ID match (last 6 chars)
            if (instanceID.Length == 6)
            {
                var match = ActiveInstances.Values
                    .Where(i => !string.IsNullOrEmpty(i.InstanceID))
                    .Where(i => i.InstanceID.Length >= 6)
                    .Where(i => i.InstanceID
                        .Substring(i.InstanceID.Length - 6, 6)
                        .Equals(instanceID, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Safety: only return if exactly one match
                if (match.Count == 1)
                    return match[0];
            }
            else
            {
                // 1?? Exact match (fast path)
                if (ActiveInstances.TryGetValue(instanceID, out var active))
                    return active;
            }

            return null;
        }

        public Task AdoptInstance(string instanceID)
        {
            if (ActiveInstances.TryGetValue(instanceID, out var instance))
            {
                instance.Configured = true;

                // Save to database asynchronously
                return Task.Run(async () =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var configuredInstance = new ConfiguredInstance
                        {
                            InstanceID = instance.InstanceID,
                            Name = instance.Name,
                            MachineName = instance.MachineName,
                            IPAddress = instance.IPAddress,
                            GamePort = instance.GamePort,
                            ProfileName = instance.ProfileName ?? string.Empty,
                            TargetWorld = instance.TargetWorld ?? string.Empty,
                            TorchVersion = instance.TorchVersion,
                            LastUpdate = DateTime.UtcNow
                        };

                        // Check if already exists
                        var existing = await dbContext.ConfiguredInstances.FindAsync(instanceID);
                        if (existing != null)
                        {
                            existing.Name = configuredInstance.Name;
                            existing.MachineName = configuredInstance.MachineName;
                            existing.IPAddress = configuredInstance.IPAddress;
                            existing.GamePort = configuredInstance.GamePort;
                            existing.ProfileName = configuredInstance.ProfileName;
                            existing.TargetWorld = configuredInstance.TargetWorld;
                            existing.TorchVersion = configuredInstance.TorchVersion;
                            existing.LastUpdate = DateTime.UtcNow;
                        }
                        else
                        {
                            dbContext.ConfiguredInstances.Add(configuredInstance);
                        }

                        await dbContext.SaveChangesAsync();
                    }

                    NotifyStateChanged(instanceID);
                });
            }

            return Task.CompletedTask;
        }

        public List<TorchInstance> GetPendingInstances()
        {
            return ActiveInstances.Values.Where(x => !x.Configured).ToList();
        }

        public string GetInstanceName(string instanceId)
        {
            //Should we be using getinstancebyid here and return instance.Name?
            if (ActiveInstances.TryGetValue(instanceId, out var instance))
                return instance.Name ?? instanceId;
            return instanceId;
        }
    }
}
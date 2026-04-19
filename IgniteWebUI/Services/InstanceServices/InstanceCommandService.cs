using IgniteAPI.DTOs.Instances;
using IgniteAPI.Models;

namespace IgniteWebUI.Services.InstanceServices
{
    /// <summary>
    /// Service responsible for sending commands from the web UI to the Torch instances via the <see cref="InstanceSocketManager"/>. 
    /// </summary>
    public class InstanceCommandService
    {
        private readonly InstanceSocketManager _socketManager;

        public InstanceCommandService(InstanceSocketManager socketManager)
        {
            _socketManager = socketManager;
        }

        public Task SendCommand(TorchInstanceBase instanceBase, string command)
            => _socketManager.SendCommandAsync(instanceBase.InstanceID, command, new { });

        public Task SendCommand(TorchInstanceBase instanceBase, string command, object args)
            => _socketManager.SendCommandAsync(instanceBase.InstanceID, command, args);
    }
}

using Torch2API.DTOs.WebSockets;

namespace Torch2WebUI.Services.InstanceServices
{
    /// <summary>
    /// Handles one or more incoming WebSocket commands dispatched by <see cref="InstanceSocketManager"/>.
    /// Implement this interface and register it in DI as <see cref="ISocketMessageHandler"/>
    /// to subscribe to specific commands without coupling to the socket manager.
    /// </summary>
    public interface ISocketMessageHandler
    {
        /// <summary>The WebSocket command strings this handler responds to (e.g. <c>TorchConstants.WsLog</c>).</summary>
        IReadOnlyList<string> HandledCommands { get; }

        void Handle(string instanceId, SocketMsgEnvelope envelope);
    }
}

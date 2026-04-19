using Microsoft.AspNetCore.Mvc;
using IgniteAPI.Constants;
using IgniteAPI.DTOs.Chat;
using IgniteWebUI.Services.InstanceServices;

namespace IgniteWebUI.APIControllers.Chat
{
    [ApiController]
    public class ChatController : ControllerBase
    {
        [HttpPost(WebAPIConstants.PostChat)]
        public IActionResult PostMessage(
            [FromBody] ChatMessage message,
            [FromServices] InstanceChatService chatService,
            [FromServices] InstanceManager instanceManager)
        {
            if (!Request.Headers.TryGetValue(TorchConstants.InstanceIdHeader, out var instanceId))
                return BadRequest("Missing Instance-Id header");

            var instance = instanceManager.GetInstanceName(instanceId);
            chatService.Append(instanceId.ToString(), message, instance);
            return Ok();
        }
    }
}

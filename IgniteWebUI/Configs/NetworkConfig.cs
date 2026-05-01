using IgniteAPI.Attributes;
using YamlDotNet.Serialization;

namespace IgniteWebUI.Configs
{
    public class NetworkConfig
    {
        [EnvVar("TORCH2_HTTPS_REDIRECT")]
        [YamlMember(Description = "Enable HTTPS redirection (disable for reverse proxy / Docker setups)")]
        public bool UseHttpsRedirection { get; set; } = true;

        [EnvVar("TORCH2_HTTP_TIMEOUT_SECONDS")]
        [YamlMember(Description = "HTTP client timeout in seconds for panel requests")]
        public int HttpTimeoutSeconds { get; set; } = 5;

        [EnvVar("TORCH2_WS_RECONNECT_DELAY_SECONDS")]
        [YamlMember(Description = "WebSocket reconnection delay in seconds")]
        public int WsReconnectDelaySeconds { get; set; } = 5;

        [EnvVar("TORCH2_WS_CONNECTION_TIMEOUT_SECONDS")]
        [YamlMember(Description = "WebSocket connection timeout in seconds")]
        public int WsConnectionTimeoutSeconds { get; set; } = 10;

        [EnvVar("TORCH2_INSTANCE_TIMEOUT_SECONDS")]
        [YamlMember(Description = "Seconds before an instance is considered offline after no heartbeat")]
        public int InstanceTimeoutSeconds { get; set; } = 5;
    }
}

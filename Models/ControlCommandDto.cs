using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RAIDAR_FRONT.Models
{
    /// <summary>
    /// C++ 서버로 전송할 원격 제어 명령 DTO
    /// </summary>
    public class ControlCommandDto
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty; // e.g. EMERGENCY_ALARM, RESET, SET_RANGE

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("parameters")]
        public Dictionary<string, object> Parameters { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}

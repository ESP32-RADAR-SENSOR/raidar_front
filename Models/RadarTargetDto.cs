using System;
using System.Text.Json.Serialization;

namespace RAIDAR_FRONT.Models
{
    /// <summary>
    /// C++ 서버 및 ESP32 레이더 센서로부터 수신되는 타겟 데이터 DTO
    /// </summary>
    public class RadarTargetDto
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("distance")]
        public double Distance { get; set; } // 미터(m) 단위

        [JsonPropertyName("angle")]
        public double Angle { get; set; } // 도(deg) 단위 (0~360)

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } // 섭씨(°C)

        [JsonPropertyName("dangerLevel")]
        public string DangerLevel { get; set; } = "Normal"; // Normal, Warning, Danger

        [JsonPropertyName("sequence")]
        public long Sequence { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}

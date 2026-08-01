using System;
using System.Text.Json.Serialization;

namespace RAIDAR_FRONT.Models
{
    /// <summary>
    /// 시스템 로그 항목 데이터 모델
    /// </summary>
    public class LogEntry
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [JsonPropertyName("level")]
        public string Level { get; set; } = "INFO"; // INFO, WARN, ERROR

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = "SYSTEM";

        public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");
    }
}

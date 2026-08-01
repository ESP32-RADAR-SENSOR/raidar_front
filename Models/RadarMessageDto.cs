using System;
using System.Text.Json.Serialization;

namespace RAIDAR_FRONT.Models
{

    /// 수신 메시지 공통 베이스 DTO ("type" 필드로 구분)

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(DistanceMessageDto), typeDiscriminator: "distance")]
    [JsonDerivedType(typeof(ScanMessageDto), typeDiscriminator: "scan")]
    public abstract class RadarMessageDto
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("sequence")]
        public long Sequence { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// 거리 단일 측정 메시지 ("type": "distance")

    public class DistanceMessageDto : RadarMessageDto
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }

    /// 레이더 스캔 메시지 ("type": "scan")

    public class ScanMessageDto : RadarMessageDto
    {
        [JsonPropertyName("angle")]
        public double Angle { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }
}

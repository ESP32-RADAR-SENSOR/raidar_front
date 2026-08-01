using RAIDAR_FRONT.Models;

namespace RAIDAR_FRONT.ViewModels
{
    /// 수신된 RadarMessageDto(DistanceMessage/ScanMessage)를 화면에 바인딩할 타겟 ViewModel

    public class RadarTargetViewModel : ViewModelBase
    {
        private string _deviceId = string.Empty;
        private long _sequence;
        private double _distance;
        private double _angle;
        private string _dangerLevel = "Normal";
        private DateTime _lastUpdated = DateTime.Now;
        private bool _isSelected;

        // 1. 장비 ID
        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        // 2. 패킷 시퀀스 번호
        public long Sequence
        {
            get => _sequence;
            set => SetProperty(ref _sequence, value);
        }

        // 3. 측정 거리
        public double Distance
        {
            get => _distance;
            set => SetProperty(ref _distance, value);
        }

        // 4. 스캔 방위각
        public double Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }

        // 5. 위험 등급 ("Normal", "Warning", "Danger")
        public string DangerLevel
        {
            get => _dangerLevel;
            set => SetProperty(ref _dangerLevel, value);
        }

        // 6. 마지막 수신 시각
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        // 7. 레이더 상에서 선택 상태
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }


        /// 수신된 RadarMessageDto (DistanceMessageDto 또는 ScanMessageDto) 데이터를 받아 실시간 갱신

        public void UpdateFromMessage(RadarMessageDto message)
        {
            if (message == null) return;

            // 공통 프로퍼티 갱신
            DeviceId = message.DeviceId;
            Sequence = message.Sequence;
            LastUpdated = message.Timestamp;

            // 타입별 프로퍼티 갱신 (C# Pattern Matching)
            switch (message)
            {
                case DistanceMessageDto distMsg:
                    Distance = distMsg.Distance;
                    break;

                case ScanMessageDto scanMsg:
                    Distance = scanMsg.Distance;
                    Angle = scanMsg.Angle;
                    break;
            }

            DangerLevel = Distance < 100.0 ? "Danger" : (Distance < 200.0 ? "Warning" : "Normal");
        }
    }
}

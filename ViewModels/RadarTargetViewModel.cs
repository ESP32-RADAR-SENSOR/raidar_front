using System;
using RAIDAR_FRONT.Models;

namespace RAIDAR_FRONT.ViewModels
{
    /// <summary>
    /// 레이더 화면에 개별 표시될 감지 타겟 ViewModel
    /// </summary>
    public class RadarTargetViewModel : ViewModelBase
    {
        private string _deviceId = string.Empty;
        private double _distance;
        private double _angle;
        private double _temperature;
        private string _dangerLevel = "Normal";
        private long _sequence;
        private DateTime _lastUpdated = DateTime.Now;
        private bool _isSelected;

        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        public double Distance
        {
            get => _distance;
            set => SetProperty(ref _distance, value);
        }

        public double Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }

        public double Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        public string DangerLevel
        {
            get => _dangerLevel;
            set => SetProperty(ref _dangerLevel, value);
        }

        public long Sequence
        {
            get => _sequence;
            set => SetProperty(ref _sequence, value);
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public void UpdateFromDto(RadarTargetDto dto)
        {
            DeviceId = dto.DeviceId;
            Distance = dto.Distance;
            Angle = dto.Angle;
            Temperature = dto.Temperature;
            DangerLevel = dto.DangerLevel;
            Sequence = dto.Sequence;
            LastUpdated = dto.Timestamp;
        }
    }
}

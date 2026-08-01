using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace RAIDAR_FRONT.ViewModels
{
    /// 레이더 상황도 뷰모델 

    public class RadarViewModel : ViewModelBase
    {
        private double _maxRange = 5.0; // 최대 거리 (단위: m)
        private double _sweepAngle = 0;
        private RadarTargetViewModel? _selectedTarget;
        private DispatcherTimer? _timer;

        public ObservableCollection<RadarTargetViewModel> Targets { get; } = new();

        public double MaxRange
        {
            get => _maxRange;
            set => SetProperty(ref _maxRange, value);
        }

        public double SweepAngle
        {
            get => _sweepAngle;
            set => SetProperty(ref _sweepAngle, value);
        }

        public RadarTargetViewModel? SelectedTarget
        {
            get => _selectedTarget;
            set
            {
                if(_selectedTarget != value)
                {
                    if(_selectedTarget != null)
                    {
                        _selectedTarget.IsSelected = false;
                    }
                    _selectedTarget = value;
                    if(_selectedTarget != null)
                    {
                        _selectedTarget.IsSelected = true;
                    }
                }
            }
        }

        public event Action<RadarTargetViewModel?>? TargetSelectedChanged;

        public RadarViewModel()
        {
            StartSweepAnimation();
        }

        private void StartSweepAnimation()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30) // 30ms 간격으로 스윕 각도 업데이트
            };

            _timer.Tick += (s, e) =>
            {
                _sweepAngle += 5; // 스윕 각도 증가
                if (_sweepAngle >= 360)
                {
                    _sweepAngle %= 360; // 360도 이상이면 초기화
                }
            };
            _timer.Start();
        }
    }
}

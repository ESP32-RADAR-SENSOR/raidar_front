using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using RAIDAR_FRONT.Models;

namespace RAIDAR_FRONT.ViewModels
{
    /// <summary>
    /// 레이더 상황도 (Region 1) ViewModel
    /// </summary>
    public class RadarViewModel : ViewModelBase
    {
        private double _maxRange = 5.0; // 최대 측정 거리 (미터)
        private double _sweepAngle = 0.0;
        private RadarTargetViewModel? _selectedTarget;
        private DispatcherTimer? _sweepTimer;

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
                if (_selectedTarget != value)
                {
                    if (_selectedTarget != null) _selectedTarget.IsSelected = false;
                    _selectedTarget = value;
                    if (_selectedTarget != null) _selectedTarget.IsSelected = true;
                    OnPropertyChanged();
                    TargetSelectionChanged?.Invoke(_selectedTarget);
                }
            }
        }

        public event Action<RadarTargetViewModel?>? TargetSelectionChanged;

        public RadarViewModel()
        {
            StartSweepAnimation();
        }

        public void UpdateTarget(RadarTargetDto dto)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var existing = Targets.FirstOrDefault(t => t.DeviceId == dto.DeviceId);
                if (existing == null)
                {
                    var newTarget = new RadarTargetViewModel();
                    newTarget.UpdateFromDto(dto);
                    Targets.Add(newTarget);

                    if (SelectedTarget == null)
                    {
                        SelectedTarget = newTarget;
                    }
                }
                else
                {
                    existing.UpdateFromDto(dto);
                }
            });
        }

        private void StartSweepAnimation()
        {
            _sweepTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            _sweepTimer.Tick += (s, e) =>
            {
                SweepAngle = (SweepAngle + 2.5) % 360;
            };
            _sweepTimer.Start();
        }
    }
}

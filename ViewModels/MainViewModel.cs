using System.Linq;
using System.Windows;
using RAIDAR_FRONT.Models;
using RAIDAR_FRONT.Services;

namespace RAIDAR_FRONT.ViewModels
{
    /// <summary>
    /// 메인 대시보드 ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private bool _isConnected;

        public INetworkService NetworkService { get; }
        public RadarViewModel RadarVM { get; } = new();
        public DeviceControlViewModel DeviceControlVM { get; }
        public LogViewerViewModel LogViewerVM { get; }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public MainViewModel()
        {
            NetworkService = new TcpClientService();
            DeviceControlVM = new DeviceControlViewModel(NetworkService);
            LogViewerVM = new LogViewerViewModel(NetworkService);

            IsConnected = NetworkService.IsConnected;
            NetworkService.ConnectionStatusChanged += isConnected =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    IsConnected = isConnected;
                });
            };

            // TCP로 수신된 레이더 데이터를 RadarViewModel의 Targets에 반영
            NetworkService.TargetDataReceived += targetData =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    var target = RadarVM.Targets.FirstOrDefault(t => t.DeviceId == targetData.DeviceId);
                    if (target == null)
                    {
                        target = new RadarTargetViewModel { DeviceId = targetData.DeviceId };
                        RadarVM.Targets.Add(target);
                    }

                    string prevDanger = target.DangerLevel;
                    target.UpdateFromMessage(targetData);

                    // DangerLevel 등급 변동 시 WARN / ERROR 로그 자동 생성
                    if (prevDanger != target.DangerLevel)
                    {
                        if (target.DangerLevel == "Danger")
                        {
                            LogViewerVM.AddLog(new LogEntry
                            {
                                Timestamp = DateTime.Now,
                                Level = "ERROR",
                                Message = $"[DANGER] Target {target.DeviceId} in critical distance! ({target.Distance:F2}m, {target.Angle:F1}°)",
                                Source = target.DeviceId
                            });
                        }
                        else if (target.DangerLevel == "Warning")
                        {
                            LogViewerVM.AddLog(new LogEntry
                            {
                                Timestamp = DateTime.Now,
                                Level = "WARN",
                                Message = $"[WARNING] Target {target.DeviceId} entering warning zone! ({target.Distance:F2}m, {target.Angle:F1}°)",
                                Source = target.DeviceId
                            });
                        }
                    }
                });
            };

            // 레이더 뷰어 타겟 선택 이벤트 연동 -> 장비 제어 패널 업데이트
            RadarVM.TargetSelectedChanged += target =>
            {
                DeviceControlVM.SelectedTarget = target;
                if (target != null)
                {
                    LogViewerVM.AddLog(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = "INFO",
                        Message = $"Selected Target: {target.DeviceId} (Dist: {target.Distance:F2}m, Angle: {target.Angle:F1}°)",
                        Source = "UI_CONTROL"
                    });
                }
            };

            LogViewerVM.AddLog(new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = "INFO",
                Message = "Application started.",
                Source = "MainViewModel"
            });
        }
    }
}


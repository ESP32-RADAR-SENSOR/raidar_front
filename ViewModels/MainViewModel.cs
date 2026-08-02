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
                    target.UpdateFromMessage(targetData);
                });
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


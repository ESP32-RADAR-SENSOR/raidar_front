using RAIDAR_FRONT.Models;
using RAIDAR_FRONT.Services;
using System.Windows.Input;

namespace RAIDAR_FRONT.ViewModels
{

    /// 장비 상세 및 원격 제어 뷰모델 골격 (직접 작성할 영역)

    public class DeviceControlViewModel : ViewModelBase
    {
        private readonly INetworkService _networkService;
        private RadarTargetViewModel? _selectedTarget;

        private string _ipAddress = "127.0.0.1";
        private int _port = 8080;
        private bool _isConnected = false;

        public RadarTargetViewModel? SelectedTarget
        {
            get => _selectedTarget;
            set => SetProperty(ref _selectedTarget, value);
        }

        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public ICommand EmergencyAlarmCommand { get; }
        public ICommand ResetDeviceCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }


        public DeviceControlViewModel(INetworkService networkService)
        {
            _networkService = networkService;

            _isConnected = _networkService.IsConnected;

            _networkService.ConnectionStatusChanged += _isConnected =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    IsConnected = _isConnected;
                });
            };

            EmergencyAlarmCommand = new RelayCommand(async() => await SendEmergencyAlarmAsync());
            ResetDeviceCommand = new RelayCommand(async() => await SendResetAsync());
            ConnectCommand = new RelayCommand(async() => await ConnectAsync());
            DisconnectCommand = new RelayCommand(async() => await DisconnectAsync());

        }

        private async Task SendEmergencyAlarmAsync()
        {
            if (SelectedTarget == null) return;
            var cmd = new ControlCommandDto
            {
                Command = "EMERGENCY_ALARM",
                DeviceId = SelectedTarget.DeviceId,
                Timestamp = DateTime.Now
            };
            await _networkService.SendCommandAsync(cmd);
        }

        private async Task SendResetAsync()
        {
            if (SelectedTarget == null) return;

            var cmd = new ControlCommandDto
            {
                Command = "RESET",
                DeviceId = SelectedTarget.DeviceId,
                Timestamp = DateTime.Now
            };
            await _networkService.SendCommandAsync(cmd);
        }

        private async Task ConnectAsync()
        {
            await _networkService.ConnectAsync(IpAddress, Port);
        }

        private async Task DisconnectAsync()
        {
            await _networkService.DisconnectAsync();
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using RAIDAR_FRONT.Models;
using RAIDAR_FRONT.Services;

namespace RAIDAR_FRONT.ViewModels
{
    /// <summary>
    /// 장비 상세정보 및 원격 제어 명령 패널 (Region 2) ViewModel
    /// </summary>
    public class DeviceControlViewModel : ViewModelBase
    {
        private readonly INetworkService _networkService;
        private RadarTargetViewModel? _selectedTarget;

        private string _ipAddress = "127.0.0.1";
        private int _port = 8080;
        private bool _isConnected;
        private bool _isMockMode = true;

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

        public bool IsMockMode
        {
            get => _isMockMode;
            set
            {
                if (SetProperty(ref _isMockMode, value))
                {
                    _networkService.SetMockMode(value);
                }
            }
        }

        public ICommand EmergencyAlarmCommand { get; }
        public ICommand ResetDeviceCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        public DeviceControlViewModel(INetworkService networkService)
        {
            _networkService = networkService ?? throw new ArgumentNullException(nameof(networkService));
            _isConnected = _networkService.IsConnected;
            _isMockMode = _networkService.IsMockMode;

            _networkService.ConnectionStatusChanged += isConnected =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    IsConnected = isConnected;
                });
            };

            EmergencyAlarmCommand = new RelayCommand(async () => await SendEmergencyAlarmAsync());
            ResetDeviceCommand = new RelayCommand(async () => await SendResetAsync());
            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
            DisconnectCommand = new RelayCommand(async () => await DisconnectAsync());
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

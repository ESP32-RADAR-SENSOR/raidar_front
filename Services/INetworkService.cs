using System;
using System.Threading.Tasks;
using RAIDAR_FRONT.Models;

namespace RAIDAR_FRONT.Services
{
    /// <summary>
    /// C++ 서버 및 ESP32 레이더 통신을 위한 네트워크 서비스 인터페이스
    /// </summary>
    public interface INetworkService
    {
        event Action<RadarTargetDto>? TargetDataReceived;
        event Action<LogEntry>? LogReceived;
        event Action<bool>? ConnectionStatusChanged;

        bool IsConnected { get; }
        bool IsMockMode { get; }
        string ServerIp { get; }
        int ServerPort { get; }

        Task<bool> ConnectAsync(string ip, int port);
        Task DisconnectAsync();
        Task SendCommandAsync(ControlCommandDto command);
        void SetMockMode(bool enable);
    }
}

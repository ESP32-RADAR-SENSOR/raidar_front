using System;
using System.Threading.Tasks;
using RAIDAR_FRONT.Models;

namespace RAIDAR_FRONT.Services
{
    /// C++ 서버와의 TCP/IP 소켓 통신 모듈
    /// 
    public class TcpClientService : INetworkService
    {
        public event Action<RadarTargetDto>? TargetDataReceived;
        public event Action<LogEntry>? LogReceived;
        public event Action<bool>? ConnectionStatusChanged;

        public bool IsConnected { get; private set; }

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            // TODO: TCP 소켓 연결 로직 구현
            await Task.CompletedTask;
            return false;
        }

        public async Task DisconnectAsync()
        {
            // TODO: 소켓 연결 해제 로직 구현
            await Task.CompletedTask;
        }

        public async Task SendCommandAsync(ControlCommandDto command)
        {
            // TODO: C++ 서버로 JSON 명령 송신 로직 구현
            await Task.CompletedTask;
        }
    }
}

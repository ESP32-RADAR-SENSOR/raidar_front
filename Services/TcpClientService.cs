using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RAIDAR_FRONT.Models;

namespace RAIDAR_FRONT.Services
{
    /// <summary>
    /// C++ 서버와의 TCP/IP 소켓 통신 및 JSON 메세지 송수신 구현 모듈
    /// </summary>
    public class TcpClientService : INetworkService
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private Timer? _mockTimer;

        private bool _isConnected;
        private bool _isMockMode = true; // 기본적으로 가상 모드 활성화로 즉시 확인 가능
        private string _serverIp = "127.0.0.1";
        private int _serverPort = 8080;
        private long _sequenceCounter = 0;
        private readonly Random _random = new();

        public event Action<RadarTargetDto>? TargetDataReceived;
        public event Action<LogEntry>? LogReceived;
        public event Action<bool>? ConnectionStatusChanged;

        public bool IsConnected => _isConnected;
        public bool IsMockMode => _isMockMode;
        public string ServerIp => _serverIp;
        public int ServerPort => _serverPort;

        public TcpClientService()
        {
            // 초기 시뮬레이션 타이머 설정
            if (_isMockMode)
            {
                StartMockGenerator();
            }
        }

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            _serverIp = ip;
            _serverPort = port;

            try
            {
                StopMockGenerator();

                _client = new TcpClient();
                await _client.ConnectAsync(ip, port);
                _stream = _client.GetStream();

                _cts = new CancellationTokenSource();
                _isConnected = true;
                _isMockMode = false;

                ConnectionStatusChanged?.Invoke(true);
                EmitLog("INFO", $"TCP 서버 ({ip}:{port})에 성공적으로 연결되었습니다.", "NetworkService");

                // 비동기 수신 루프 시작
                _ = ReceiveLoopAsync(_cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
                EmitLog("ERROR", $"TCP 서버 연결 실패 ({ip}:{port}): {ex.Message}", "NetworkService");

                // 연결 실패 시 Mock 모드로 자동 전환하여 시각화 동작 보장
                SetMockMode(true);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _cts?.Cancel();
                _stream?.Close();
                _client?.Close();
            }
            catch
            {
                // Ignore cleanup errors
            }
            finally
            {
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
                EmitLog("INFO", "TCP 서버 연결이 해제되었습니다.", "NetworkService");
            }

            await Task.CompletedTask;
        }

        public async Task SendCommandAsync(ControlCommandDto command)
        {
            if (command == null) return;

            string json = JsonSerializer.Serialize(command);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");

            if (_isConnected && _stream != null)
            {
                try
                {
                    await _stream.WriteAsync(data, 0, data.Length);
                    await _stream.FlushAsync();
                    EmitLog("INFO", $"원격 제어 명령 전송 완료: {command.Command} (대상: {command.DeviceId})", "NetworkService");
                }
                catch (Exception ex)
                {
                    EmitLog("ERROR", $"명령 전송 실패: {ex.Message}", "NetworkService");
                }
            }
            else if (_isMockMode)
            {
                EmitLog("INFO", $"[가상 모드] 원격 명령 시뮬레이션 처리: {command.Command} -> {command.DeviceId}", "MockService");
            }
        }

        public void SetMockMode(bool enable)
        {
            _isMockMode = enable;
            if (enable)
            {
                StartMockGenerator();
                EmitLog("INFO", "가상 데모 모드가 활성화되었습니다.", "MockService");
            }
            else
            {
                StopMockGenerator();
                EmitLog("INFO", "가상 데모 모드가 비활성화되었습니다.", "MockService");
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            if (_stream == null) return;

            using StreamReader reader = new StreamReader(_stream, Encoding.UTF8);

            try
            {
                while (!token.IsCancellationRequested && _client?.Connected == true)
                {
                    string? line = await reader.ReadLineAsync(token);
                    if (line == null) break; // Server disconnected

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var target = JsonSerializer.Deserialize<RadarTargetDto>(line);
                        if (target != null)
                        {
                            TargetDataReceived?.Invoke(target);
                        }
                    }
                    catch (JsonException ex)
                    {
                        EmitLog("WARN", $"수신된 JSON 파싱 오류: {ex.Message} | Raw: {line}", "NetworkService");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal exit
            }
            catch (Exception ex)
            {
                EmitLog("ERROR", $"수신 루프 오류: {ex.Message}", "NetworkService");
            }
            finally
            {
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
            }
        }

        private void StartMockGenerator()
        {
            StopMockGenerator();
            _mockTimer = new Timer(GenerateMockData, null, 0, 500); // 500ms 간격으로 업데이트
        }

        private void StopMockGenerator()
        {
            _mockTimer?.Dispose();
            _mockTimer = null;
        }

        private double _t1Angle = 45.0;
        private double _t2Angle = 180.0;
        private double _t3Angle = 290.0;

        private void GenerateMockData(object? state)
        {
            if (!_isMockMode) return;

            _sequenceCounter++;

            // 타겟 1 (경고 상태 - 저속 이동)
            _t1Angle = (_t1Angle + 1.5) % 360;
            double t1Dist = 3.2 + Math.Sin(_sequenceCounter * 0.1) * 0.4;
            var target1 = new RadarTargetDto
            {
                DeviceId = "ESP32-RADAR-01",
                Distance = Math.Round(t1Dist, 2),
                Angle = Math.Round(_t1Angle, 1),
                Temperature = Math.Round(38.5 + _random.NextDouble() * 1.5, 1),
                DangerLevel = "Warning",
                Sequence = _sequenceCounter,
                Timestamp = DateTime.Now
            };

            // 타겟 2 (정상 상태 - 고정 타겟)
            _t2Angle = (_t2Angle + 0.5) % 360;
            double t2Dist = 1.8 + Math.Cos(_sequenceCounter * 0.05) * 0.2;
            var target2 = new RadarTargetDto
            {
                DeviceId = "ESP32-RADAR-02",
                Distance = Math.Round(t2Dist, 2),
                Angle = Math.Round(_t2Angle, 1),
                Temperature = Math.Round(32.0 + _random.NextDouble(), 1),
                DangerLevel = "Normal",
                Sequence = _sequenceCounter,
                Timestamp = DateTime.Now
            };

            // 타겟 3 (위험 상태 - 빠르게 접근 중인 감지 물체)
            _t3Angle = (_t3Angle + 2.5) % 360;
            double t3Dist = 0.8 + Math.Abs(Math.Sin(_sequenceCounter * 0.08)) * 1.2;
            var target3 = new RadarTargetDto
            {
                DeviceId = "ESP32-RADAR-03",
                Distance = Math.Round(t3Dist, 2),
                Angle = Math.Round(_t3Angle, 1),
                Temperature = Math.Round(45.2 + _random.NextDouble() * 3.0, 1),
                DangerLevel = t3Dist < 1.2 ? "Danger" : "Warning",
                Sequence = _sequenceCounter,
                Timestamp = DateTime.Now
            };

            TargetDataReceived?.Invoke(target1);
            TargetDataReceived?.Invoke(target2);
            TargetDataReceived?.Invoke(target3);

            // 주기적 이벤트 로그 생성
            if (_sequenceCounter % 10 == 0)
            {
                string level = target3.DangerLevel == "Danger" ? "ERROR" : "INFO";
                string msg = target3.DangerLevel == "Danger"
                    ? $"[경고] {target3.DeviceId} 근접 위험물체 감지! (거리: {target3.Distance}m, 각도: {target3.Angle}°)"
                    : $"[정상] 레이더 스캔 주기 완료 (시퀀스 #{_sequenceCounter})";
                EmitLog(level, msg, "ESP32-Sensor");
            }
        }

        private void EmitLog(string level, string message, string source)
        {
            LogReceived?.Invoke(new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Source = source
            });
        }
    }
}

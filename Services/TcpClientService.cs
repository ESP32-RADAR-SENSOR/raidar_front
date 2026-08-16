using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using RAIDAR_FRONT.Models;

namespace RAIDAR_FRONT.Services
{
    /// C++ 서버와의 TCP/IP 소켓 통신 모듈

    public class TcpClientService : INetworkService
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private bool _isConnected;

        public event Action<RadarMessageDto>? TargetDataReceived;
        public event Action<LogEntry>? LogReceived;
        public event Action<bool>? ConnectionStatusChanged;

        public bool IsConnected => _isConnected;

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            try { 
                _client = new TcpClient();
                await _client.ConnectAsync(ip, port);
                _stream = _client.GetStream();

                _cts = new CancellationTokenSource();
                _isConnected = true;

                ConnectionStatusChanged?.Invoke(true);
                EmitLog("INFO", $"Connected to {ip}:{port}", "TCP_CLIENT");


                _ = ReceiveLoopAsync(_cts.Token);
                return true;

            } catch(Exception ex)
            {
                _isConnected = false;
                EmitLog("ERROR", $"Failed to connect to {ip}:{port}, Error: {ex.Message}", "TCP_CLIENT");
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
            finally
            {
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
                EmitLog("INFO", "Disconnected from server", "TCP_CLIENT");
            }

            await Task.CompletedTask;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            if (_stream == null) return;

            using StreamReader reader = new StreamReader(_stream, Encoding.UTF8);
            try
            {
                while (!token.IsCancellationRequested && _client?.Connected == true)
                {
                    string? line = await reader.ReadLineAsync(token);
                    if (line == null) break;

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        DistanceMessageDto? targetData = JsonSerializer.Deserialize<DistanceMessageDto>(line, _jsonOptions);
                        if (targetData != null)
                        {
                            TargetDataReceived?.Invoke(targetData);
                            EmitLog("INFO", $"Received data: {line}", $"{targetData.DeviceId}");
                        }

                    }
                    catch (JsonException jsonEx)
                    {
                        EmitLog("ERROR", $"JSON deserialization error: {jsonEx.Message}, Data: {line}", "TCP_CLIENT");
                    }
                }
            } catch(OperationCanceledException)
            {
                EmitLog("INFO", "Receive loop canceled", "TCP_CLIENT");
            }
            catch (Exception ex)
            {
                EmitLog("ERROR", $"Error in receive loop: {ex.Message}", "TCP_CLIENT");
            } finally
            {
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
                EmitLog("INFO", "Disconnected from server", "TCP_CLIENT");
            }
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
                    EmitLog("INFO", $"Sent command: {json}", $"{command.DeviceId}");
                }
                catch (Exception ex)
                {
                    EmitLog("ERROR", $"Error sending command: {ex.Message}", $"{command.DeviceId}");
                    EmitLog("ERROR", ex.ToString(), $"{command.DeviceId}");
                }
            }
        }

        private void EmitLog(string level, string message, string source = "TCP_CLIENT")
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

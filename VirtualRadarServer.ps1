# ==============================================================================
# Script: VirtualRadarServer.ps1
# Description: RAIDAR_FRONT (WPF 앱) 연동용 가상 레이더 TCP 서버 시뮬레이터
# ==============================================================================

param(
    [int]$Port = 8080,               # WPF 앱(DeviceControlViewModel)의 기본 포트 8080
    [string]$DeviceId = "RADAR_01",
    [int]$IntervalMs = 50            # 데이터 전송 간격 (ms)
)

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " 📡 RAIDAR_FRONT 가상 레이더 TCP 서버 (Port: $Port)" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# IPAddress.Loopback (127.0.0.1) 로컬 TCP Listener 생성
$listener = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Loopback, $Port)

try {
    $listener.Start()
    Write-Host "[+] TCP 서버가 성공적으로 시작되었습니다. (127.0.0.1:$Port)" -ForegroundColor Green
    Write-Host "[+] WPF 앱에서 [연결] 버튼을 눌러주세요...`n" -ForegroundColor Yellow
} catch {
    Write-Error "❌ 포트 $Port 바인딩 실패: $_"
    exit
}

$sequence = 0
$angle = 0
$angleStep = 3
$maxAngle = 180

try {
    while ($true) {
        # WPF 클라이언트 연결 수락
        $client = $listener.AcceptTcpClient()
        $remote = $client.Client.RemoteEndPoint
        Write-Host "✅ [WPF 앱 연결 성공!] -> $remote" -ForegroundColor Green

        $stream = $client.GetStream()
        $writer = [System.IO.StreamWriter]::new($stream, [System.Text.Encoding]::UTF8)
        $reader = [System.IO.StreamReader]::new($stream, [System.Text.Encoding]::UTF8)
        $writer.AutoFlush = $true

        try {
            while ($client.Connected) {
                # 1. WPF에서 전송한 제어 명령어 수신 확인 (비블로킹)
                if ($stream.DataAvailable) {
                    $incomingData = $reader.ReadLine()
                    if ($incomingData) {
                        Write-Host "📩 [WPF 수신 Command] -> $incomingData" -ForegroundColor Magenta
                    }
                }

                # 2. 가상 레이더 스캔 데이터 생성
                $sequence++
                $angle += $angleStep
                if ($angle -ge $maxAngle -or $angle -le 0) {
                    $angleStep = -$angleStep
                }

                # WPF RadarViewModel MaxRange = 5.0m 에 맞게 0.5m ~ 4.5m 범위 가상 거리 생성
                $baseDist = 2.5 + 1.5 * [Math]::Sin([Math]::PI * $angle / 90)
                $noise = (Get-Random -Minimum -15 -Maximum 15) / 100.0
                $dist = [Math]::Round(($baseDist + $noise), 2)

                # WPF C# 규격: "type" 필드 ("scan" 또는 "distance") 필수 명시
                $isoTimestamp = (Get-Date).ToString("o")
                $jsonString = "{`"type`":`"scan`",`"deviceId`":`"$DeviceId`",`"sequence`":$sequence,`"timestamp`":`"$isoTimestamp`",`"angle`":$angle,`"distance`":$dist}"
                
                # 줄바꿈(\n) 필수 포함 전송 (ReadLineAsync 대응)
                $writer.WriteLine($jsonString)

                Write-Host "📡 전송 [seq:$sequence] Angle: ${angle}° | Distance: ${dist}m" -ForegroundColor DarkGray

                Start-Sleep -Milliseconds $IntervalMs
            }
        }
        catch {
            Write-Host "⚠️ WPF 앱 연결 끊김: $_" -ForegroundColor Yellow
        }
        finally {
            $writer.Dispose()
            $reader.Dispose()
            $stream.Dispose()
            $client.Dispose()
            Write-Host "`n[+] 다음 WPF 앱 연결 대기 중... (Port: $Port)" -ForegroundColor Yellow
        }
    }
}
finally {
    $listener.Stop()
    Write-Host "[!] 서버가 종료되었습니다." -ForegroundColor Red
}

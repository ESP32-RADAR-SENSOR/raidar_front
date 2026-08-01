using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RAIDAR_FRONT.Services
{
    /// <summary>
    /// WPF 앱에서 PowerShell 스크립트를 비동기로 실행하고 결과를 가져오는 서비스
    /// </summary>
    public class PowerShellRunnerService
    {
        public async Task<(bool Success, string Output, string Error)> RunScriptAsync(string scriptPath, string arguments = "")
        {
            if (!File.Exists(scriptPath)) {
                return (false, "", $"스크립트 파일을 찾을 수 없습니다: {scriptPath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            try
            {
                using var process = new Process { StartInfo = startInfo };
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                bool success = process.ExitCode == 0;
                return (success, output, error);
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
        }
    }
}

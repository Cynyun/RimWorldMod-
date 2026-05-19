using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using RimWorldModManager.Utils;

namespace RimWorldModManager.Services
{
    public class ProcessResult
    {
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
        public bool TimedOut { get; set; }
    }

    public class SteamCmdService
    {
        private readonly string _steamCmdPath;
        private readonly ILogger _logger;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

        public SteamCmdService(string steamCmdPath, ILogger logger)
        {
            _steamCmdPath = steamCmdPath;
            _logger = logger;
        }

        public async Task<ProcessResult> DownloadModAsync(int workshopId, string installDir)
        {
            var args = new[]
            {
                "+login", "anonymous",
                "+force_install_dir", installDir,
                "+workshop_download_item", "294100", workshopId.ToString(),
                "+quit"
            };
            return await ExecuteAsync(args);
        }

        private async Task<ProcessResult> ExecuteAsync(string[] args)
        {
            var result = new ProcessResult();

            if (!System.IO.File.Exists(_steamCmdPath))
            {
                result.StandardError = $"SteamCMD 路径不存在: {_steamCmdPath}";
                result.ExitCode = -1;
                _logger.Error(result.StandardError);
                return result;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = _steamCmdPath,
                Arguments = string.Join(" ", args),
                WorkingDirectory = System.IO.Path.GetDirectoryName(_steamCmdPath),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            using var cancellationTokenSource = new CancellationTokenSource(_defaultTimeout);

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    _logger.Info(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    _logger.Warning(e.Data);
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var processTask = Task.Run(() =>
                {
                    process.WaitForExit();
                    return process.ExitCode;
                }, cancellationTokenSource.Token);

                if (await Task.WhenAny(processTask, Task.Delay(_defaultTimeout)) == processTask)
                {
                    result.ExitCode = await processTask;
                    result.StandardOutput = outputBuilder.ToString();
                    result.StandardError = errorBuilder.ToString();
                }
                else
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                    }
                    result.TimedOut = true;
                    result.StandardError = "操作超时";
                    _logger.Error("SteamCMD 操作超时");
                }
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.StandardError = ex.ToString();
                _logger.Error("执行 SteamCMD 失败", ex);
            }

            return result;
        }
    }
}
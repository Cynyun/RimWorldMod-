using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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

    public class SteamCmdService : IDisposable
    {
        private Process _process;
        private readonly string _steamCmdPath;
        private readonly ILogger _logger;
        private readonly object _lockObj = new();
        private readonly StringBuilder _outputBuffer = new();
        private readonly ManualResetEventSlim _outputComplete = new(false);
        private bool _isLoggedIn;
        private bool _disposed;
        private System.Threading.Timer _heartbeatTimer;
        private string _lastOutput = string.Empty;
        private Action<string> _outputCallback;

        public event Action<string> OutputReceived;
        public event Action<bool> LoginStatusChanged;

        public bool IsLoggedIn => _isLoggedIn;
        public string LastOutput => _lastOutput;

        public SteamCmdService(string steamCmdPath, ILogger logger)
        {
            _steamCmdPath = steamCmdPath;
            _logger = logger;
        }

        public void SetOutputCallback(Action<string> callback)
        {
            _outputCallback = callback;
        }

        public async Task<bool> StartAsync()
        {
            if (!File.Exists(_steamCmdPath))
            {
                _logger.Error($"SteamCMD 路径不存在: {_steamCmdPath}");
                return false;
            }

            try
            {
                await Task.Run(() =>
                {
                    lock (_lockObj)
                    {
                        if (_process != null && !_process.HasExited)
                        {
                            return;
                        }

                        var startInfo = new ProcessStartInfo
                        {
                            FileName = _steamCmdPath,
                            Arguments = "+login anonymous +quit",
                            WorkingDirectory = Path.GetDirectoryName(_steamCmdPath),
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        };

                        _process = new Process { StartInfo = startInfo };
                        _process.OutputDataReceived += Process_OutputDataReceived;
                        _process.ErrorDataReceived += Process_ErrorDataReceived;
                        _process.Start();
                        _process.BeginOutputReadLine();
                        _process.BeginErrorReadLine();
                        _process.WaitForExit();
                    }
                });

                _isLoggedIn = CheckLoginSuccess();
                LoginStatusChanged?.Invoke(_isLoggedIn);

                if (_isLoggedIn)
                {
                    StartHeartbeat();
                    _logger.Info("SteamCMD 登录成功");
                }
                else
                {
                    _logger.Warning("SteamCMD 登录状态未知");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("启动 SteamCMD 失败", ex);
                return false;
            }
        }

        private void StartHeartbeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = new System.Threading.Timer(async _ => await CheckLoginStatusAsync(), null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private async Task CheckLoginStatusAsync()
        {
            if (_disposed) return;

            try
            {
                var testResult = await ExecuteCommandAsync(new[] { "+login", "anonymous", "+quit" }, false);
                
                var wasLoggedIn = _isLoggedIn;
                _isLoggedIn = CheckLoginSuccess();

                if (wasLoggedIn != _isLoggedIn)
                {
                    LoginStatusChanged?.Invoke(_isLoggedIn);
                    _logger.Info($"SteamCMD 登录状态变更: {_isLoggedIn}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"SteamCMD 状态检查失败: {ex.Message}");
            }
        }

        private bool CheckLoginSuccess()
        {
            var output = _outputBuffer.ToString();
            return !string.IsNullOrEmpty(output) && 
                   (output.Contains("Loading Steam API") || output.Contains("Anon"));
        }

        public async Task<ProcessResult> DownloadModAsync(uint workshopId)
        {
            var args = new[]
            {
                "+login", "anonymous",
                "+workshop_download_item", "294100", workshopId.ToString(),
                "+quit"
            };

            return await ExecuteCommandAsync(args, true);
        }

        private async Task<ProcessResult> ExecuteCommandAsync(string[] args, bool captureOutput)
        {
            var result = new ProcessResult();

            if (!File.Exists(_steamCmdPath))
            {
                result.StandardError = $"SteamCMD 路径不存在: {_steamCmdPath}";
                _logger.Error(result.StandardError);
                return result;
            }

            _outputBuffer.Clear();
            _outputComplete.Reset();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _steamCmdPath,
                    Arguments = string.Join(" ", args),
                    WorkingDirectory = Path.GetDirectoryName(_steamCmdPath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = startInfo };
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                        _lastOutput = e.Data;
                        OutputReceived?.Invoke(e.Data);
                        _outputCallback?.Invoke(e.Data);
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                        _lastOutput = e.Data;
                        OutputReceived?.Invoke(e.Data);
                        _outputCallback?.Invoke(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());

                result.ExitCode = process.ExitCode;
                result.StandardOutput = outputBuilder.ToString();
                result.StandardError = errorBuilder.ToString();

                if (captureOutput)
                {
                    _outputBuffer.Append(outputBuilder);
                    _outputBuffer.Append(errorBuilder);
                }
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.StandardError = ex.ToString();
                _logger.Error("执行 SteamCMD 命令失败", ex);
            }

            return result;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _outputBuffer.AppendLine(e.Data);
                _lastOutput = e.Data;
                OutputReceived?.Invoke(e.Data);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _outputBuffer.AppendLine(e.Data);
                _lastOutput = e.Data;
                OutputReceived?.Invoke(e.Data);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _heartbeatTimer?.Dispose();
            _process?.Dispose();
            _outputComplete.Dispose();
        }
    }
}
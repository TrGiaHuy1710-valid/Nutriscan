using System.Diagnostics;

namespace NutriScan.Services
{
    /// <summary>
    /// Background service that manages the Python OCR backend process
    /// Automatically starts the Flask app when the web app starts
    /// </summary>
    public class PythonOcrBackgroundService : BackgroundService
    {
        private readonly ILogger<PythonOcrBackgroundService> _logger;
        private Process? _pythonProcess;
        private readonly string _pythonPath;
        private readonly string _appModule = "app.main";
        private const string OCR_URL = "http://localhost:5000";
        private const int MAX_STARTUP_WAIT = 90000; // 90 seconds
        private const int HEALTH_CHECK_INTERVAL = 1000; // 1 second

        private readonly string _ocrServiceRoot;

        public PythonOcrBackgroundService(ILogger<PythonOcrBackgroundService> logger)
        {
            _logger = logger;
            
            _ocrServiceRoot = GetOcrServiceRoot();
            
            // Get Python executable path
            _pythonPath = GetPythonExecutable();

            _logger.LogInformation("OCR service root directory: {ProjectRoot}", _ocrServiceRoot);
            _logger.LogInformation("Python executable: {PythonPath}", _pythonPath);
            _logger.LogInformation("OCR app module: {AppModule}", _appModule);
        }

        private string GetOcrServiceRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "apps", "ocr-service");
                if (File.Exists(Path.Combine(candidate, "app", "main.py")))
                {
                    return candidate;
                }
                dir = dir.Parent;
            }
            return Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "";
        }

        /// <summary>
        /// Find Python executable in system PATH or venv
        /// </summary>
        private string GetPythonExecutable()
        {
            // Try local virtual environment first
            var venvPython = OperatingSystem.IsWindows()
                ? Path.Combine(_ocrServiceRoot, "venv", "Scripts", "python.exe")
                : Path.Combine(_ocrServiceRoot, "venv", "bin", "python");

            if (File.Exists(venvPython))
            {
                _logger.LogInformation("Found local virtual environment Python: {Path}", venvPython);
                return venvPython;
            }

            // Try common Python locations
            var pythonPaths = new[]
            {
                "python",      // Default in PATH
                "python3",     // Linux/Mac default
                "py",          // Windows launcher
            };

            foreach (var python in pythonPaths)
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = python,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process?.WaitForExit(5000) == true && process.ExitCode == 0)
                        {
                            _logger.LogInformation("Found Python: {Python}", python);
                            return python;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Python executable not found: {Python} - {Error}", python, ex.Message);
                }
            }

            throw new InvalidOperationException(
                "Python executable not found in PATH. Please install Python 3.8+ and add it to system PATH.");
        }

        /// <summary>
        /// Check if OCR service is healthy
        /// </summary>
        private async Task<bool> IsOcrServiceHealthyAsync()
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
                {
                    var response = await client.GetAsync($"{OCR_URL}/api/health");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Wait for OCR service to be ready
        /// </summary>
        private async Task WaitForOcrServiceAsync()
        {
            _logger.LogInformation("Waiting for OCR service to start...");
            
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < MAX_STARTUP_WAIT)
            {
                if (await IsOcrServiceHealthyAsync())
                {
                    _logger.LogInformation("OCR service is healthy and ready!");
                    return;
                }
                
                await Task.Delay(HEALTH_CHECK_INTERVAL);
            }

            throw new TimeoutException(
                $"OCR service did not start within {MAX_STARTUP_WAIT}ms. Check if Python and dependencies are installed.");
        }

        /// <summary>
        /// Execute background task: Start Python OCR backend
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (!File.Exists(Path.Combine(_ocrServiceRoot, "app", "main.py")))
                {
                    _logger.LogWarning("OCR app module not found under {Path}. OCR backend will not start.", _ocrServiceRoot);
                    return;
                }

                _logger.LogInformation("Starting Python OCR backend service...");

                var startInfo = new ProcessStartInfo
                {
                    FileName = _pythonPath,
                    Arguments = $"-m {_appModule}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _ocrServiceRoot
                };

                _pythonProcess = new Process { StartInfo = startInfo };

                // Log Python output
                _pythonProcess.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        _logger.LogInformation("[Python OCR] {Message}", args.Data);
                };

                _pythonProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        _logger.LogError("[Python OCR ERROR] {Message}", args.Data);
                };

                if (!_pythonProcess.Start())
                {
                    throw new InvalidOperationException("Failed to start Python process");
                }

                _logger.LogInformation("Python OCR process started (PID: {ProcessId})", _pythonProcess.Id);

                _pythonProcess.BeginOutputReadLine();
                _pythonProcess.BeginErrorReadLine();

                // Wait for service to be ready
                await WaitForOcrServiceAsync();

                _logger.LogInformation("✓ Python OCR backend is running at {Url}", OCR_URL);

                // Keep the process alive
                await _pythonProcess.WaitForExitAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OCR background service cancellation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OCR background service: {Message}", ex.Message);
                _logger.LogWarning("Continuing C# application execution despite OCR background service startup failure.");
            }
        }

        /// <summary>
        /// Stop the Python process gracefully
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _logger.LogInformation("Stopping Python OCR backend service...");

                    _pythonProcess.Kill(true); // Kill process tree
                    
                    var timeoutTask = Task.Delay(5000, cancellationToken);
                    var exitTask = _pythonProcess.WaitForExitAsync(cancellationToken);

                    await Task.WhenAny(exitTask, timeoutTask);

                    _logger.LogInformation("Python OCR backend service stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping OCR background service: {Message}", ex.Message);
            }
            finally
            {
                _pythonProcess?.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}

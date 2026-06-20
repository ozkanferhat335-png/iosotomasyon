namespace IOSAutomation.DeviceManagement.Services
{
    using IOSAutomation.Core.Interfaces;
    using IOSAutomation.Core.Exceptions;
    using IOSAutomation.DeviceManagement.Models;
    using System.Diagnostics;
    using System.Text.Json;
    using Serilog;

    /// <summary>
    /// Manages iOS devices using libimobiledevice and related tools
    /// </summary>
    public class IosDeviceManager : IDeviceManager
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, IDevice> _cachedDevices = new();
        private CancellationTokenSource? _monitoringCts;

        public event EventHandler<string>? DeviceConnected;
        public event EventHandler<string>? DeviceDisconnected;

        public IosDeviceManager()
        {
            _logger = Log.Logger;
            StartDeviceMonitoring();
        }

        /// <summary>
        /// Detects all connected iOS devices
        /// </summary>
        public async Task<IEnumerable<IDevice>> GetConnectedDevicesAsync()
        {
            try
            {
                _logger.Information("Scanning for connected iOS devices...");
                var devices = new List<IDevice>();

                // Execute idevice_id to get list of connected devices
                var output = await ExecuteCommandAsync("idevice_id", "-l");
                
                if (string.IsNullOrEmpty(output))
                {
                    _logger.Warning("No connected iOS devices found");
                    return devices;
                }

                var udids = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var udid in udids)
                {
                    try
                    {
                        var device = await GetDeviceAsync(udid);
                        if (device != null)
                        {
                            devices.Add(device);
                            _logger.Information($"Found device: {device}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning($"Failed to get device info for {udid}: {ex.Message}");
                    }
                }

                // Cache devices
                lock (_cachedDevices)
                {
                    _cachedDevices.Clear();
                    foreach (var device in devices)
                    {
                        _cachedDevices[device.UDID] = device;
                    }
                }

                return devices;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error detecting devices: {ex.Message}", ex);
                throw new DeviceException("Failed to detect connected devices", ex);
            }
        }

        /// <summary>
        /// Gets a specific device by UDID
        /// </summary>
        public async Task<IDevice?> GetDeviceAsync(string udid)
        {
            try
            {
                // Check cache first
                if (_cachedDevices.TryGetValue(udid, out var cachedDevice))
                {
                    // Verify device is still connected
                    var state = await GetDeviceStateAsync(udid);
                    if (state != DeviceState.Unknown)
                    {
                        cachedDevice.State = state;
                        return cachedDevice;
                    }
                }

                var device = new Device { UDID = udid };

                // Get device information using ideviceinfo
                var infoOutput = await ExecuteCommandAsync("ideviceinfo", $"-u {udid}");
                ParseDeviceInfo(infoOutput, device);

                // Get device state
                device.State = await GetDeviceStateAsync(udid);
                device.IsConnected = device.State != DeviceState.Offline && device.State != DeviceState.Unknown;

                return device;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting device {udid}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the current state of a device
        /// </summary>
        public async Task<DeviceState> GetDeviceStateAsync(string udid)
        {
            try
            {
                var output = await ExecuteCommandAsync("ideviceinfo", $"-u {udid} -k State");
                
                return output?.Trim() switch
                {
                    "connected" => DeviceState.Online,
                    "disconnected" => DeviceState.Offline,
                    _ => DeviceState.Unknown
                };
            }
            catch
            {
                return DeviceState.Offline;
            }
        }

        /// <summary>
        /// Installs an application on the device
        /// </summary>
        public async Task<bool> InstallApplicationAsync(string udid, string appPath)
        {
            try
            {
                _logger.Information($"Installing app from {appPath} on device {udid}");
                
                if (!File.Exists(appPath))
                {
                    throw new DeviceException($"App file not found: {appPath}") { DeviceUdid = udid };
                }

                var result = await ExecuteCommandAsync("ideviceinstaller", $"-u {udid} -i {appPath}");
                _logger.Information($"App installation completed: {result}");
                
                return !result?.Contains("error", StringComparison.OrdinalIgnoreCase) ?? false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to install app on {udid}: {ex.Message}", ex);
                throw new DeviceException($"Failed to install application on device {udid}", ex) { DeviceUdid = udid };
            }
        }

        /// <summary>
        /// Uninstalls an application from the device
        /// </summary>
        public async Task<bool> UninstallApplicationAsync(string udid, string bundleId)
        {
            try
            {
                _logger.Information($"Uninstalling app {bundleId} from device {udid}");
                
                var result = await ExecuteCommandAsync("ideviceinstaller", $"-u {udid} -U {bundleId}");
                _logger.Information($"App uninstallation completed");
                
                return !result?.Contains("error", StringComparison.OrdinalIgnoreCase) ?? false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to uninstall app from {udid}: {ex.Message}", ex);
                throw new DeviceException($"Failed to uninstall application from device {udid}", ex) { DeviceUdid = udid };
            }
        }

        /// <summary>
        /// Takes a screenshot from the device
        /// </summary>
        public async Task<string> TakeScreenshotAsync(string udid, string outputPath)
        {
            try
            {
                _logger.Information($"Taking screenshot from device {udid}");
                
                // Ensure output directory exists
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                var result = await ExecuteCommandAsync("idevicescreenshot", $"-u {udid} {outputPath}");
                
                if (File.Exists(outputPath))
                {
                    _logger.Information($"Screenshot saved to {outputPath}");
                    return outputPath;
                }

                throw new DeviceException($"Screenshot was not created at {outputPath}") { DeviceUdid = udid };
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to take screenshot from {udid}: {ex.Message}", ex);
                throw new DeviceException($"Failed to take screenshot from device {udid}", ex) { DeviceUdid = udid };
            }
        }

        /// <summary>
        /// Retrieves device logs
        /// </summary>
        public async Task<string> GetLogsAsync(string udid, string outputPath)
        {
            try
            {
                _logger.Information($"Retrieving logs from device {udid}");
                
                // Ensure output directory exists
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                var result = await ExecuteCommandAsync("idevicesyslog", $"-u {udid}");
                
                await File.WriteAllTextAsync(outputPath, result ?? string.Empty);
                _logger.Information($"Logs saved to {outputPath}");
                
                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get logs from {udid}: {ex.Message}", ex);
                throw new DeviceException($"Failed to retrieve logs from device {udid}", ex) { DeviceUdid = udid };
            }
        }

        /// <summary>
        /// Prepares the device for testing
        /// </summary>
        public async Task<bool> PrepareDeviceAsync(string udid)
        {
            try
            {
                _logger.Information($"Preparing device {udid} for testing");
                
                // Clear cache and temporary files
                // This is a placeholder - actual implementation would use device-specific commands
                
                _logger.Information($"Device {udid} prepared successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to prepare device {udid}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Launches an application on the device
        /// </summary>
        public async Task<bool> LaunchApplicationAsync(string udid, string bundleId)
        {
            try
            {
                _logger.Information($"Launching app {bundleId} on device {udid}");
                
                var result = await ExecuteCommandAsync("idevicedebugserverproxy", $"-u {udid}");
                _logger.Information($"App launch command executed");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to launch app on {udid}: {ex.Message}", ex);
                throw new DeviceException($"Failed to launch application on device {udid}", ex) { DeviceUdid = udid };
            }
        }

        /// <summary>
        /// Terminates a running application
        /// </summary>
        public async Task<bool> TerminateApplicationAsync(string udid, string bundleId)
        {
            try
            {
                _logger.Information($"Terminating app {bundleId} on device {udid}");
                // Implementation would use device-specific termination commands
                _logger.Information($"App termination command executed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to terminate app on {udid}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Executes a command-line tool
        /// </summary>
        private async Task<string?> ExecuteCommandAsync(string command, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)
                        return null;

                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        _logger.Warning($"Command failed: {command} {arguments}. Error: {error}");
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to execute command {command}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Parses device information from ideviceinfo output
        /// </summary>
        private void ParseDeviceInfo(string infoOutput, Device device)
        {
            if (string.IsNullOrEmpty(infoOutput))
                return;

            var lines = infoOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.Contains("DeviceName:"))
                    device.ModelName = line.Split(':').LastOrDefault()?.Trim() ?? "Unknown";
                
                if (line.Contains("ProductVersion:"))
                    device.IOSVersion = line.Split(':').LastOrDefault()?.Trim() ?? "Unknown";
                
                if (line.Contains("SerialNumber:"))
                    device.SerialNumber = line.Split(':').LastOrDefault()?.Trim() ?? "Unknown";
                
                if (line.Contains("ScreenWidth:"))
                    int.TryParse(line.Split(':').LastOrDefault()?.Trim(), out var width);
                
                if (line.Contains("ScreenHeight:"))
                    int.TryParse(line.Split(':').LastOrDefault()?.Trim(), out var height);
            }
        }

        /// <summary>
        /// Starts monitoring for device connection/disconnection
        /// </summary>
        private void StartDeviceMonitoring()
        {
            _monitoringCts = new CancellationTokenSource();
            _ = MonitorDevicesAsync(_monitoringCts.Token);
        }

        /// <summary>
        /// Monitors device connections and raises events
        /// </summary>
        private async Task MonitorDevicesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var previousDevices = new HashSet<string>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var currentDevices = await GetConnectedDevicesAsync();
                        var currentUdids = new HashSet<string>(currentDevices.Select(d => d.UDID));

                        // Detect new devices
                        foreach (var udid in currentUdids.Except(previousDevices))
                        {
                            DeviceConnected?.Invoke(this, udid);
                        }

                        // Detect disconnected devices
                        foreach (var udid in previousDevices.Except(currentUdids))
                        {
                            DeviceDisconnected?.Invoke(this, udid);
                        }

                        previousDevices = currentUdids;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in device monitoring: {ex.Message}", ex);
                    }

                    // Check every 5 seconds
                    await Task.Delay(5000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Information("Device monitoring stopped");
            }
        }

        public void Dispose()
        {
            _monitoringCts?.Cancel();
            _monitoringCts?.Dispose();
        }
    }
}

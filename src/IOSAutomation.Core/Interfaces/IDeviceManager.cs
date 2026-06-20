namespace IOSAutomation.Core.Interfaces
{
    /// <summary>
    /// Manages iOS device operations
    /// </summary>
    public interface IDeviceManager
    {
        /// <summary>Detects and returns all connected iOS devices</summary>
        Task<IEnumerable<IDevice>> GetConnectedDevicesAsync();

        /// <summary>Gets a specific device by UDID</summary>
        Task<IDevice?> GetDeviceAsync(string udid);

        /// <summary>Gets the current state of a device</summary>
        Task<DeviceState> GetDeviceStateAsync(string udid);

        /// <summary>Installs an application on the device</summary>
        Task<bool> InstallApplicationAsync(string udid, string appPath);

        /// <summary>Uninstalls an application from the device</summary>
        Task<bool> UninstallApplicationAsync(string udid, string bundleId);

        /// <summary>Takes a screenshot from the device</summary>
        Task<string> TakeScreenshotAsync(string udid, string outputPath);

        /// <summary>Retrieves device logs</summary>
        Task<string> GetLogsAsync(string udid, string outputPath);

        /// <summary>Prepares the device for testing (clearing cache, etc.)</summary>
        Task<bool> PrepareDeviceAsync(string udid);

        /// <summary>Launches an application on the device</summary>
        Task<bool> LaunchApplicationAsync(string udid, string bundleId);

        /// <summary>Terminates a running application</summary>
        Task<bool> TerminateApplicationAsync(string udid, string bundleId);

        /// <summary>Event raised when a device is connected</summary>
        event EventHandler<string>? DeviceConnected;

        /// <summary>Event raised when a device is disconnected</summary>
        event EventHandler<string>? DeviceDisconnected;
    }
}

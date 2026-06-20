namespace IOSAutomation.Core.Interfaces
{
    /// <summary>
    /// Represents an iOS device connected to the system
    /// </summary>
    public interface IDevice
    {
        /// <summary>Gets the unique device identifier (UDID)</summary>
        string UDID { get; }

        /// <summary>Gets the device model name (e.g., iPhone 12, iPad 10)</summary>
        string ModelName { get; }

        /// <summary>Gets the device serial number</summary>
        string SerialNumber { get; }

        /// <summary>Gets the iOS version</summary>
        string IOSVersion { get; }

        /// <summary>Gets the current device state</summary>
        DeviceState State { get; }

        /// <summary>Gets the screen resolution width</summary>
        int ScreenWidth { get; }

        /// <summary>Gets the screen resolution height</summary>
        int ScreenHeight { get; }

        /// <summary>Gets whether the device is currently connected</summary>
        bool IsConnected { get; }
    }

    /// <summary>Represents the state of an iOS device</summary>
    public enum DeviceState
    {
        Unknown,
        Offline,
        Online,
        Locked,
        Error
    }
}

namespace IOSAutomation.DeviceManagement.Models
{
    using IOSAutomation.Core.Interfaces;

    /// <summary>
    /// Implementation of iOS device
    /// </summary>
    public class Device : IDevice
    {
        public string UDID { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string IOSVersion { get; set; } = string.Empty;
        public DeviceState State { get; set; } = DeviceState.Unknown;
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public bool IsConnected { get; set; }

        public override string ToString()
        {
            return $"{ModelName} ({UDID}) - {IOSVersion} - {State}";
        }
    }
}

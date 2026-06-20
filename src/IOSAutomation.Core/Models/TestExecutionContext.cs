namespace IOSAutomation.Core.Models
{
    using IOSAutomation.Core.Interfaces;

    /// <summary>
    /// Represents the context for a test execution
    /// </summary>
    public class TestExecutionContext
    {
        public string ExecutionId { get; set; } = Guid.NewGuid().ToString();
        public string DeviceUdid { get; set; } = string.Empty;
        public string ScenarioId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> SharedData { get; set; } = new();
        public ILogger? Logger { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}

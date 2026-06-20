namespace IOSAutomation.Core.Exceptions
{
    /// <summary>
    /// Base exception for automation framework
    /// </summary>
    public class AutomationException : Exception
    {
        public AutomationException(string message) : base(message) { }
        public AutomationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Thrown when device operation fails
    /// </summary>
    public class DeviceException : AutomationException
    {
        public string? DeviceUdid { get; set; }
        public DeviceException(string message) : base(message) { }
        public DeviceException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Thrown when visual processing fails
    /// </summary>
    public class VisualProcessingException : AutomationException
    {
        public VisualProcessingException(string message) : base(message) { }
        public VisualProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Thrown when scenario execution fails
    /// </summary>
    public class ScenarioExecutionException : AutomationException
    {
        public string? ScenarioId { get; set; }
        public string? StepId { get; set; }
        public ScenarioExecutionException(string message) : base(message) { }
        public ScenarioExecutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}

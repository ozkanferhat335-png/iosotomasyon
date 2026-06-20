namespace IOSAutomation.ScenarioEngine.Models
{
    using IOSAutomation.Core.Interfaces;

    /// <summary>
    /// Implementation of scenario execution result
    /// </summary>
    public class ScenarioExecutionResult : IScenarioExecutionResult
    {
        public IScenario Scenario { get; set; }
        public string DeviceUdid { get; set; } = string.Empty;
        public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
        public TimeSpan Duration => EndTime - StartTime;
        public IReadOnlyList<IStepExecutionResult> StepResults { get; set; } = new List<IStepExecutionResult>();
        public string? ErrorMessage { get; set; }
        
        public int PassedSteps => StepResults.Count(s => s.Status == ExecutionStatus.Passed);
        public int FailedSteps => StepResults.Count(s => s.Status == ExecutionStatus.Failed);
        public double SuccessRate => StepResults.Count > 0 ? PassedSteps / (double)StepResults.Count : 0.0;

        public ScenarioExecutionResult(IScenario scenario)
        {
            Scenario = scenario;
        }
    }

    /// <summary>
    /// Implementation of step execution result
    /// </summary>
    public class StepExecutionResult : IStepExecutionResult
    {
        public IScenarioStep Step { get; set; }
        public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
        public TimeSpan Duration => EndTime - StartTime;
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> CapturedData { get; set; } = new();

        public StepExecutionResult(IScenarioStep step)
        {
            Step = step;
        }
    }
}

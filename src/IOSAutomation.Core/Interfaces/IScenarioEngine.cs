namespace IOSAutomation.Core.Interfaces
{
    /// <summary>
    /// Executes test scenarios
    /// </summary>
    public interface IScenarioEngine
    {
        /// <summary>Loads a scenario from file</summary>
        Task<IScenario> LoadScenarioAsync(string scenarioFilePath);

        /// <summary>Executes a scenario on a specific device</summary>
        Task<IScenarioExecutionResult> ExecuteScenarioAsync(string deviceUdid, IScenario scenario, CancellationToken cancellationToken = default);

        /// <summary>Executes a scenario step</summary>
        Task<IStepExecutionResult> ExecuteStepAsync(string deviceUdid, IScenarioStep step, CancellationToken cancellationToken = default);

        /// <summary>Validates a scenario for correctness before execution</summary>
        Task<bool> ValidateScenarioAsync(IScenario scenario);

        /// <summary>Pauses the currently running scenario</summary>
        Task PauseExecutionAsync();

        /// <summary>Resumes a paused scenario</summary>
        Task ResumeExecutionAsync();

        /// <summary>Stops the scenario execution</summary>
        Task StopExecutionAsync();
    }

    /// <summary>
    /// Represents the result of a scenario execution
    /// </summary>
    public interface IScenarioExecutionResult
    {
        /// <summary>Gets the scenario that was executed</summary>
        IScenario Scenario { get; }

        /// <summary>Gets the device UDID where it was executed</summary>
        string DeviceUdid { get; }

        /// <summary>Gets the overall execution status</summary>
        ExecutionStatus Status { get; }

        /// <summary>Gets the execution start time</summary>
        DateTime StartTime { get; }

        /// <summary>Gets the execution end time</summary>
        DateTime EndTime { get; }

        /// <summary>Gets the total duration of execution</summary>
        TimeSpan Duration { get; }

        /// <summary>Gets the results of individual steps</summary>
        IReadOnlyList<IStepExecutionResult> StepResults { get; }

        /// <summary>Gets error details if execution failed</summary>
        string? ErrorMessage { get; }

        /// <summary>Gets the number of passed steps</summary>
        int PassedSteps { get; }

        /// <summary>Gets the number of failed steps</summary>
        int FailedSteps { get; }

        /// <summary>Gets success rate as percentage</summary>
        double SuccessRate { get; }
    }

    /// <summary>
    /// Represents the result of a single step execution
    /// </summary>
    public interface IStepExecutionResult
    {
        /// <summary>Gets the executed step</summary>
        IScenarioStep Step { get; }

        /// <summary>Gets the execution status</summary>
        ExecutionStatus Status { get; }

        /// <summary>Gets the step execution start time</summary>
        DateTime StartTime { get; }

        /// <summary>Gets the step execution end time</summary>
        DateTime EndTime { get; }

        /// <summary>Gets the step execution duration</summary>
        TimeSpan Duration { get; }

        /// <summary>Gets error details if step failed</summary>
        string? ErrorMessage { get; }

        /// <summary>Gets captured data during execution</summary>
        Dictionary<string, object> CapturedData { get; }
    }

    /// <summary>Status of scenario/step execution</summary>
    public enum ExecutionStatus
    {
        Pending,
        Running,
        Passed,
        Failed,
        Skipped,
        Error
    }
}

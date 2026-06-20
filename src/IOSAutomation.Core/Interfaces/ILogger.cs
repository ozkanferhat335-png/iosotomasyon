namespace IOSAutomation.Core.Interfaces
{
    /// <summary>
    /// Core logging interface
    /// </summary>
    public interface ILogger
    {
        /// <summary>Logs an informational message</summary>
        void LogInfo(string message);

        /// <summary>Logs a debug message</summary>
        void LogDebug(string message);

        /// <summary>Logs a warning message</summary>
        void LogWarning(string message);

        /// <summary>Logs an error message</summary>
        void LogError(string message, Exception? exception = null);

        /// <summary>Logs a critical error message</summary>
        void LogCritical(string message, Exception? exception = null);
    }

    /// <summary>
    /// Logs test execution events
    /// </summary>
    public interface ITestExecutionLogger : ILogger
    {
        /// <summary>Logs scenario execution start</summary>
        void LogScenarioStart(string scenarioId, string deviceUdid);

        /// <summary>Logs scenario execution completion</summary>
        void LogScenarioComplete(string scenarioId, string deviceUdid, bool success, TimeSpan duration);

        /// <summary>Logs step execution</summary>
        void LogStepExecution(string stepId, string action, bool success, string? details = null);

        /// <summary>Logs device event</summary>
        void LogDeviceEvent(string deviceUdid, string eventType, string message);

        /// <summary>Logs captured screenshot</summary>
        void LogScreenshot(string deviceUdid, string screenshotPath);

        /// <summary>Logs extracted text</summary>
        void LogExtractedText(string deviceUdid, string text);
    }

    /// <summary>
    /// Manages test reporting
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>Generates a daily report</summary>
        Task<string> GenerateDailyReportAsync(DateTime date, string outputPath);

        /// <summary>Generates a weekly report</summary>
        Task<string> GenerateWeeklyReportAsync(DateTime week, string outputPath);

        /// <summary>Generates a scenario execution report</summary>
        Task<string> GenerateScenarioReportAsync(IScenarioExecutionResult result, string outputPath);

        /// <summary>Generates statistics report with success/failure rates</summary>
        Task<string> GenerateStatisticsReportAsync(DateTime startDate, DateTime endDate, string outputPath);
    }
}

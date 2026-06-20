namespace IOSAutomation.Logging.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents test execution statistics
    /// </summary>
    public class TestStatistics
    {
        public int TotalScenarios { get; set; }
        public int PassedScenarios { get; set; }
        public int FailedScenarios { get; set; }
        public int SkippedScenarios { get; set; }
        public double SuccessRate => TotalScenarios > 0 ? PassedScenarios / (double)TotalScenarios * 100 : 0;
        public double FailureRate => TotalScenarios > 0 ? FailedScenarios / (double)TotalScenarios * 100 : 0;
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan AverageDuration => TotalScenarios > 0 ? TimeSpan.FromMilliseconds(TotalDuration.TotalMilliseconds / TotalScenarios) : TimeSpan.Zero;
        public List<ScenarioLog> ScenarioLogs { get; set; } = new();
    }

    /// <summary>
    /// Represents log entry for a single scenario execution
    /// </summary>
    public class ScenarioLog
    {
        public string ScenarioId { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public string DeviceUdid { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string Status { get; set; } = string.Empty;
        public int PassedSteps { get; set; }
        public int FailedSteps { get; set; }
        public double SuccessRate => PassedSteps + FailedSteps > 0 ? PassedSteps / (double)(PassedSteps + FailedSteps) * 100 : 0;
        public string? ErrorMessage { get; set; }
        public List<string> Screenshots { get; set; } = new();
        public List<string> Logs { get; set; } = new();
    }
}

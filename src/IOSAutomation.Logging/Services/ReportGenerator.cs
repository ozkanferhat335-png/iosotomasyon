namespace IOSAutomation.Logging.Services
{
    using IOSAutomation.Core.Interfaces;
    using IOSAutomation.Logging.Models;
    using Newtonsoft.Json;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Generates comprehensive test reports
    /// </summary>
    public class ReportGenerator : IReportGenerator
    {
        private readonly ILogger _logger;
        private readonly string _reportsDirectory;
        private readonly List<ScenarioLog> _executionHistory;

        public ReportGenerator(string? reportsDirectory = null)
        {
            _logger = Log.Logger;
            _reportsDirectory = reportsDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            _executionHistory = new List<ScenarioLog>();

            if (!Directory.Exists(_reportsDirectory))
            {
                Directory.CreateDirectory(_reportsDirectory);
            }
        }

        /// <summary>
        /// Generates a daily report
        /// </summary>
        public async Task<string> GenerateDailyReportAsync(DateTime date, string outputPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Generating daily report for {date:yyyy-MM-dd}");

                    var dailyExecutions = _executionHistory
                        .Where(x => x.StartTime.Date == date.Date)
                        .ToList();

                    var stats = CalculateStatistics(dailyExecutions);
                    var reportPath = Path.Combine(_reportsDirectory, outputPath);
                    
                    var reportContent = GenerateHtmlReport("Daily Report", date, stats);
                    var reportDir = Path.GetDirectoryName(reportPath);
                    if (!string.IsNullOrEmpty(reportDir) && !Directory.Exists(reportDir))
                    {
                        Directory.CreateDirectory(reportDir);
                    }

                    File.WriteAllText(reportPath, reportContent);
                    _logger.Information($"Daily report saved to {reportPath}");
                    return reportPath;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error generating daily report: {ex.Message}", ex);
                    throw;
                }
            });
        }

        /// <summary>
        /// Generates a weekly report
        /// </summary>
        public async Task<string> GenerateWeeklyReportAsync(DateTime week, string outputPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Generating weekly report for week of {week:yyyy-MM-dd}");

                    var startOfWeek = week.AddDays(-(int)week.DayOfWeek);
                    var weeklyExecutions = _executionHistory
                        .Where(x => x.StartTime.Date >= startOfWeek.Date && x.StartTime.Date < startOfWeek.AddDays(7).Date)
                        .ToList();

                    var stats = CalculateStatistics(weeklyExecutions);
                    var reportPath = Path.Combine(_reportsDirectory, outputPath);
                    
                    var reportContent = GenerateHtmlReport("Weekly Report", week, stats);
                    var reportDir = Path.GetDirectoryName(reportPath);
                    if (!string.IsNullOrEmpty(reportDir) && !Directory.Exists(reportDir))
                    {
                        Directory.CreateDirectory(reportDir);
                    }

                    File.WriteAllText(reportPath, reportContent);
                    _logger.Information($"Weekly report saved to {reportPath}");
                    return reportPath;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error generating weekly report: {ex.Message}", ex);
                    throw;
                }
            });
        }

        /// <summary>
        /// Generates a scenario execution report
        /// </summary>
        public async Task<string> GenerateScenarioReportAsync(IScenarioExecutionResult result, string outputPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Generating scenario report for {result.Scenario.Name}");

                    var scenarioLog = new ScenarioLog
                    {
                        ScenarioId = result.Scenario.Id,
                        ScenarioName = result.Scenario.Name,
                        DeviceUdid = result.DeviceUdid,
                        StartTime = result.StartTime,
                        EndTime = result.EndTime,
                        Status = result.Status.ToString(),
                        PassedSteps = result.PassedSteps,
                        FailedSteps = result.FailedSteps,
                        ErrorMessage = result.ErrorMessage
                    };

                    _executionHistory.Add(scenarioLog);

                    var reportPath = Path.Combine(_reportsDirectory, outputPath);
                    var reportContent = GenerateScenarioHtmlReport(result);
                    
                    var reportDir = Path.GetDirectoryName(reportPath);
                    if (!string.IsNullOrEmpty(reportDir) && !Directory.Exists(reportDir))
                    {
                        Directory.CreateDirectory(reportDir);
                    }

                    File.WriteAllText(reportPath, reportContent);
                    _logger.Information($"Scenario report saved to {reportPath}");
                    return reportPath;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error generating scenario report: {ex.Message}", ex);
                    throw;
                }
            });
        }

        /// <summary>
        /// Generates statistics report
        /// </summary>
        public async Task<string> GenerateStatisticsReportAsync(DateTime startDate, DateTime endDate, string outputPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Generating statistics report for {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                    var periodExecutions = _executionHistory
                        .Where(x => x.StartTime.Date >= startDate.Date && x.StartTime.Date <= endDate.Date)
                        .ToList();

                    var stats = CalculateStatistics(periodExecutions);
                    var reportPath = Path.Combine(_reportsDirectory, outputPath);
                    
                    var reportContent = GenerateStatisticsHtmlReport(stats, startDate, endDate);
                    var reportDir = Path.GetDirectoryName(reportPath);
                    if (!string.IsNullOrEmpty(reportDir) && !Directory.Exists(reportDir))
                    {
                        Directory.CreateDirectory(reportDir);
                    }

                    File.WriteAllText(reportPath, reportContent);
                    _logger.Information($"Statistics report saved to {reportPath}");
                    return reportPath;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error generating statistics report: {ex.Message}", ex);
                    throw;
                }
            });
        }

        /// <summary>
        /// Logs a scenario execution for reporting
        /// </summary>
        public void LogScenarioExecution(IScenarioExecutionResult result, string deviceModel)
        {
            var scenarioLog = new ScenarioLog
            {
                ScenarioId = result.Scenario.Id,
                ScenarioName = result.Scenario.Name,
                DeviceUdid = result.DeviceUdid,
                DeviceModel = deviceModel,
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                Status = result.Status.ToString(),
                PassedSteps = result.PassedSteps,
                FailedSteps = result.FailedSteps,
                ErrorMessage = result.ErrorMessage
            };

            _executionHistory.Add(scenarioLog);
        }

        /// <summary>
        /// Calculates statistics from execution logs
        /// </summary>
        private TestStatistics CalculateStatistics(List<ScenarioLog> logs)
        {
            var stats = new TestStatistics
            {
                TotalScenarios = logs.Count,
                PassedScenarios = logs.Count(x => x.Status == "Passed"),
                FailedScenarios = logs.Count(x => x.Status == "Failed"),
                SkippedScenarios = logs.Count(x => x.Status == "Skipped"),
                TotalDuration = TimeSpan.FromMilliseconds(logs.Sum(x => x.Duration.TotalMilliseconds)),
                ScenarioLogs = logs
            };

            return stats;
        }

        /// <summary>
        /// Generates HTML report content
        /// </summary>
        private string GenerateHtmlReport(string title, DateTime date, TestStatistics stats)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{title}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #333; }}
        .stats {{ display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin: 20px 0; }}
        .stat-card {{ border: 1px solid #ddd; padding: 15px; border-radius: 5px; }}
        .stat-card h3 {{ margin: 0 0 10px 0; }}
        .stat-value {{ font-size: 24px; font-weight: bold; }}
        .success {{ color: #28a745; }}
        .failure {{ color: #dc3545; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <h1>{title} - {date:yyyy-MM-dd}</h1>
    <div class="stats">
        <div class="stat-card">
            <h3>Total Scenarios</h3>
            <div class="stat-value">{stats.TotalScenarios}</div>
        </div>
        <div class="stat-card">
            <h3>Success Rate</h3>
            <div class="stat-value success">{stats.SuccessRate:F2}%</div>
        </div>
        <div class="stat-card">
            <h3>Failure Rate</h3>
            <div class="stat-value failure">{stats.FailureRate:F2}%</div>
        </div>
        <div class="stat-card">
            <h3>Passed</h3>
            <div class="stat-value success">{stats.PassedScenarios}</div>
        </div>
        <div class="stat-card">
            <h3>Failed</h3>
            <div class="stat-value failure">{stats.FailedScenarios}</div>
        </div>
        <div class="stat-card">
            <h3>Average Duration</h3>
            <div class="stat-value">{stats.AverageDuration.TotalSeconds:F2}s</div>
        </div>
    </div>
    <h2>Details</h2>
    <table>
        <tr>
            <th>Scenario</th>
            <th>Device</th>
            <th>Status</th>
            <th>Duration</th>
            <th>Passed Steps</th>
            <th>Failed Steps</th>
        </tr>
        {string.Join("\n", stats.ScenarioLogs.Select(x => $@"
        <tr>
            <td>{x.ScenarioName}</td>
            <td>{x.DeviceModel}</td>
            <td>{x.Status}</td>
            <td>{x.Duration.TotalSeconds:F2}s</td>
            <td>{x.PassedSteps}</td>
            <td>{x.FailedSteps}</td>
        </tr>"))}
    </table>
</body>
</html>
";
            return html;
        }

        /// <summary>
        /// Generates scenario-specific HTML report
        /// </summary>
        private string GenerateScenarioHtmlReport(IScenarioExecutionResult result)
        {
            var statusClass = result.Status == ExecutionStatus.Passed ? "success" : "failure";
            var stepsHtml = string.Join("\n", result.StepResults.Select((step, index) => $@"
            <tr>
                <td>{index + 1}</td>
                <td>{step.Step.Description}</td>
                <td>{step.Status}</td>
                <td>{step.Duration.TotalSeconds:F2}s</td>
                <td>{(step.ErrorMessage ?? "-")}</td>
            </tr>"));

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Scenario Report - {result.Scenario.Name}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #333; }}
        .status {{ padding: 10px; border-radius: 5px; color: white; }}
        .status.success {{ background-color: #28a745; }}
        .status.failure {{ background-color: #dc3545; }}
        .summary {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <h1>Scenario Report - {result.Scenario.Name}</h1>
    <div class="status {statusClass}">{result.Status}</div>
    <div class="summary">
        <p><strong>Device:</strong> {result.DeviceUdid}</p>
        <p><strong>Start Time:</strong> {result.StartTime:yyyy-MM-dd HH:mm:ss}</p>
        <p><strong>End Time:</strong> {result.EndTime:yyyy-MM-dd HH:mm:ss}</p>
        <p><strong>Duration:</strong> {result.Duration.TotalSeconds:F2}s</p>
        <p><strong>Success Rate:</strong> {result.SuccessRate:P}</p>
        <p><strong>Passed Steps:</strong> {result.PassedSteps}/{result.StepResults.Count}</p>
        {(string.IsNullOrEmpty(result.ErrorMessage) ? "" : $"<p><strong>Error:</strong> {result.ErrorMessage}</p>")}
    </div>
    <h2>Step Details</h2>
    <table>
        <tr>
            <th>#</th>
            <th>Step</th>
            <th>Status</th>
            <th>Duration</th>
            <th>Error</th>
        </tr>
        {stepsHtml}
    </table>
</body>
</html>
";
            return html;
        }

        /// <summary>
        /// Generates statistics HTML report
        /// </summary>
        private string GenerateStatisticsHtmlReport(TestStatistics stats, DateTime startDate, DateTime endDate)
        {
            return GenerateHtmlReport("Statistics Report", startDate, stats);
        }
    }
}

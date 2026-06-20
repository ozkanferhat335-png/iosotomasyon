namespace IOSAutomation.Logging.Services
{
    using IOSAutomation.Core.Interfaces;
    using IOSAutomation.Core.Models;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Logs test execution events with structured logging
    /// </summary>
    public class TestExecutionLogger : ITestExecutionLogger
    {
        private readonly Serilog.ILogger _logger;
        private readonly string _logsDirectory;

        public TestExecutionLogger(string? logsDirectory = null)
        {
            _logsDirectory = logsDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
            }

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(_logsDirectory, "automation-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            _logger = Log.Logger;
        }

        public void LogInfo(string message) => _logger.Information(message);
        public void LogDebug(string message) => _logger.Debug(message);
        public void LogWarning(string message) => _logger.Warning(message);
        public void LogError(string message, Exception? exception = null) => _logger.Error(exception, message);
        public void LogCritical(string message, Exception? exception = null) => _logger.Fatal(exception, message);

        public void LogScenarioStart(string scenarioId, string deviceUdid)
        {
            _logger.Information("Scenario started | ScenarioId: {ScenarioId} | Device: {DeviceUdid}", scenarioId, deviceUdid);
        }

        public void LogScenarioComplete(string scenarioId, string deviceUdid, bool success, TimeSpan duration)
        {
            var status = success ? "PASSED" : "FAILED";
            _logger.Information("Scenario completed | ScenarioId: {ScenarioId} | Device: {DeviceUdid} | Status: {Status} | Duration: {Duration}ms",
                scenarioId, deviceUdid, status, duration.TotalMilliseconds);
        }

        public void LogStepExecution(string stepId, string action, bool success, string? details = null)
        {
            var status = success ? "PASSED" : "FAILED";
            _logger.Information("Step executed | StepId: {StepId} | Action: {Action} | Status: {Status} | Details: {Details}",
                stepId, action, status, details ?? "N/A");
        }

        public void LogDeviceEvent(string deviceUdid, string eventType, string message)
        {
            _logger.Information("Device event | Device: {DeviceUdid} | EventType: {EventType} | Message: {Message}",
                deviceUdid, eventType, message);
        }

        public void LogScreenshot(string deviceUdid, string screenshotPath)
        {
            _logger.Information("Screenshot captured | Device: {DeviceUdid} | Path: {ScreenshotPath}",
                deviceUdid, screenshotPath);
        }

        public void LogExtractedText(string deviceUdid, string text)
        {
            _logger.Information("Text extracted | Device: {DeviceUdid} | TextLength: {TextLength}",
                deviceUdid, text?.Length ?? 0);
        }
    }
}

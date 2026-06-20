namespace IOSAutomation.Desktop
{
    using IOSAutomation.Core.Interfaces;
    using IOSAutomation.DeviceManagement.Services;
    using IOSAutomation.VisualProcessing.Services;
    using IOSAutomation.ScenarioEngine.Services;
    using IOSAutomation.Logging.Services;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Main application class for iOS automation system
    /// </summary>
    public class AutomationApplication
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IVisualProcessingEngine _visualEngine;
        private readonly IScenarioEngine _scenarioEngine;
        private readonly ITestExecutionLogger _logger;
        private readonly ReportGenerator _reportGenerator;

        public AutomationApplication()
        {
            _deviceManager = new IosDeviceManager();
            _visualEngine = new VisualProcessingEngine();
            _scenarioEngine = new ScenarioEngine(_deviceManager, _visualEngine);
            _logger = new TestExecutionLogger();
            _reportGenerator = new ReportGenerator();
        }

        /// <summary>
        /// Initializes the application
        /// </summary>
        public async Task InitializeAsync()
        {
            _logger.LogInfo("iOS Automation System starting...");
            _logger.LogInfo("Detecting connected devices...");
            
            var devices = await _deviceManager.GetConnectedDevicesAsync();
            _logger.LogInfo($"Found {devices.Count()} connected device(s)");

            foreach (var device in devices)
            {
                _logger.LogDeviceEvent(device.UDID, "Connected", $"{device.ModelName} - iOS {device.IOSVersion}");
            }
        }

        /// <summary>
        /// Runs a test scenario on a specific device
        /// </summary>
        public async Task<IScenarioExecutionResult> RunScenarioAsync(string scenarioFilePath, string deviceUdid)
        {
            try
            {
                _logger.LogInfo($"Loading scenario from {scenarioFilePath}");
                var scenario = await _scenarioEngine.LoadScenarioAsync(scenarioFilePath);

                _logger.LogScenarioStart(scenario.Id, deviceUdid);
                var result = await _scenarioEngine.ExecuteScenarioAsync(deviceUdid, scenario);
                
                _logger.LogScenarioComplete(scenario.Id, deviceUdid, result.Status == ExecutionStatus.Passed, result.Duration);
                
                // Generate scenario report
                var device = await _deviceManager.GetDeviceAsync(deviceUdid);
                if (device != null)
                {
                    _reportGenerator.LogScenarioExecution(result, device.ModelName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running scenario: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Runs multiple scenarios across all connected devices
        /// </summary>
        public async Task<Dictionary<string, IScenarioExecutionResult>> RunScenariosAsync(string[] scenarioFilePaths)
        {
            var results = new Dictionary<string, IScenarioExecutionResult>();
            
            try
            {
                var devices = await _deviceManager.GetConnectedDevicesAsync();
                
                if (devices.Count() == 0)
                {
                    _logger.LogWarning("No connected devices found");
                    return results;
                }

                foreach (var scenarioPath in scenarioFilePaths)
                {
                    foreach (var device in devices)
                    {
                        var result = await RunScenarioAsync(scenarioPath, device.UDID);
                        results[$"{scenarioPath}_{device.UDID}"] = result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running scenarios: {ex.Message}", ex);
            }

            return results;
        }

        /// <summary>
        /// Generates reports for testing period
        /// </summary>
        public async Task GenerateReportsAsync()
        {
            try
            {
                _logger.LogInfo("Generating test reports...");
                
                var today = DateTime.Now;
                var dailyReportPath = await _reportGenerator.GenerateDailyReportAsync(today, $"DailyReport_{today:yyyy-MM-dd}.html");
                _logger.LogInfo($"Daily report generated: {dailyReportPath}");

                var statisticsReportPath = await _reportGenerator.GenerateStatisticsReportAsync(
                    today.AddDays(-7), today, $"StatisticsReport_{today:yyyy-MM-dd}.html");
                _logger.LogInfo($"Statistics report generated: {statisticsReportPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating reports: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Shuts down the application
        /// </summary>
        public void Shutdown()
        {
            _logger.LogInfo("iOS Automation System shutting down...");
        }
    }
}

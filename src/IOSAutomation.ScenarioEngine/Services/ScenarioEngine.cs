namespace IOSAutomation.ScenarioEngine.Services
{
    using IOSAutomation.Core.Interfaces;
    using IOSAutomation.Core.Exceptions;
    using IOSAutomation.ScenarioEngine.Models;
    using Serilog;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Executes test scenarios
    /// </summary>
    public class ScenarioEngine : IScenarioEngine
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IVisualProcessingEngine _visualEngine;
        private readonly ILogger _logger;
        private readonly ScenarioLoader _scenarioLoader;
        private CancellationTokenSource? _executionCts;
        private bool _isPaused;

        public ScenarioEngine(IDeviceManager deviceManager, IVisualProcessingEngine visualEngine)
        {
            _deviceManager = deviceManager;
            _visualEngine = visualEngine;
            _logger = Log.Logger;
            _scenarioLoader = new ScenarioLoader();
        }

        /// <summary>
        /// Loads a scenario from file
        /// </summary>
        public async Task<IScenario> LoadScenarioAsync(string scenarioFilePath)
        {
            return await _scenarioLoader.LoadScenarioAsync(scenarioFilePath);
        }

        /// <summary>
        /// Executes a scenario on a specific device
        /// </summary>
        public async Task<IScenarioExecutionResult> ExecuteScenarioAsync(string deviceUdid, IScenario scenario, CancellationToken cancellationToken = default)
        {
            _executionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var result = new ScenarioExecutionResult(scenario)
            {
                DeviceUdid = deviceUdid,
                StartTime = DateTime.UtcNow,
                Status = ExecutionStatus.Running
            };

            try
            {
                _logger.Information($"Starting scenario execution: {scenario.Name} on device {deviceUdid}");

                // Verify device exists
                var device = await _deviceManager.GetDeviceAsync(deviceUdid);
                if (device == null)
                    throw new ScenarioExecutionException($"Device not found: {deviceUdid}");

                // Launch the target application
                await _deviceManager.LaunchApplicationAsync(deviceUdid, scenario.TargetAppBundleId);
                await Task.Delay(2000); // Wait for app to launch

                var stepResults = new List<IStepExecutionResult>();

                // Execute each step
                foreach (var step in scenario.Steps)
                {
                    if (_executionCts.Token.IsCancellationRequested)
                        break;

                    // Handle pause
                    while (_isPaused && !_executionCts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(100);
                    }

                    var stepResult = await ExecuteStepAsync(deviceUdid, step, _executionCts.Token);
                    stepResults.Add(stepResult);

                    if (stepResult.Status == ExecutionStatus.Failed)
                    {
                        _logger.Warning($"Step failed: {step.Description}");
                        break; // Stop execution on failure
                    }
                }

                result.StepResults = stepResults.AsReadOnly();
                result.Status = result.FailedSteps == 0 ? ExecutionStatus.Passed : ExecutionStatus.Failed;

                _logger.Information($"Scenario execution completed: {result.Status} ({result.PassedSteps}/{stepResults.Count} steps passed)");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing scenario: {ex.Message}", ex);
                result.Status = ExecutionStatus.Error;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
                _executionCts?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Executes a scenario step
        /// </summary>
        public async Task<IStepExecutionResult> ExecuteStepAsync(string deviceUdid, IScenarioStep step, CancellationToken cancellationToken = default)
        {
            var result = new StepExecutionResult(step)
            {
                StartTime = DateTime.UtcNow,
                Status = ExecutionStatus.Running
            };

            try
            {
                _logger.Information($"Executing step: {step.Description}");

                switch (step.ActionType)
                {
                    case StepActionType.TakeScreenshot:
                        await HandleTakeScreenshot(deviceUdid, step, result);
                        break;

                    case StepActionType.TapElement:
                        await HandleTapElement(deviceUdid, step, result);
                        break;

                    case StepActionType.EnterText:
                        await HandleEnterText(deviceUdid, step, result);
                        break;

                    case StepActionType.VerifyElement:
                        await HandleVerifyElement(deviceUdid, step, result);
                        break;

                    case StepActionType.ExtractText:
                        await HandleExtractText(deviceUdid, step, result);
                        break;

                    case StepActionType.Delay:
                        await HandleDelay(step, result);
                        break;

                    default:
                        _logger.Warning($"Unsupported action type: {step.ActionType}");
                        result.Status = ExecutionStatus.Skipped;
                        break;
                }

                if (result.Status == ExecutionStatus.Running)
                    result.Status = ExecutionStatus.Passed;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing step: {ex.Message}", ex);
                result.Status = ExecutionStatus.Failed;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
            }

            return result;
        }

        /// <summary>
        /// Validates a scenario for correctness before execution
        /// </summary>
        public async Task<bool> ValidateScenarioAsync(IScenario scenario)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Validating scenario: {scenario.Name}");

                    if (string.IsNullOrEmpty(scenario.Id))
                    {
                        _logger.Warning("Scenario ID is empty");
                        return false;
                    }

                    if (string.IsNullOrEmpty(scenario.TargetAppBundleId))
                    {
                        _logger.Warning("Target app bundle ID is empty");
                        return false;
                    }

                    if (scenario.Steps.Count == 0)
                    {
                        _logger.Warning("Scenario has no steps");
                        return false;
                    }

                    _logger.Information("Scenario validation passed");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error validating scenario: {ex.Message}", ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Pauses the currently running scenario
        /// </summary>
        public async Task PauseExecutionAsync()
        {
            _isPaused = true;
            _logger.Information("Scenario execution paused");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Resumes a paused scenario
        /// </summary>
        public async Task ResumeExecutionAsync()
        {
            _isPaused = false;
            _logger.Information("Scenario execution resumed");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Stops the scenario execution
        /// </summary>
        public async Task StopExecutionAsync()
        {
            _executionCts?.Cancel();
            _logger.Information("Scenario execution stopped");
            await Task.CompletedTask;
        }

        // Step action handlers

        private async Task HandleTakeScreenshot(string deviceUdid, IScenarioStep step, IStepExecutionResult result)
        {
            var screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots", $"{Guid.NewGuid()}.png");
            var savedPath = await _deviceManager.TakeScreenshotAsync(deviceUdid, screenshotPath);
            result.CapturedData["screenshotPath"] = savedPath;
        }

        private async Task HandleTapElement(string deviceUdid, IScenarioStep step, IStepExecutionResult result)
        {
            if (!step.Parameters.TryGetValue("elementId", out var elementId))
                throw new ScenarioExecutionException("TapElement requires 'elementId' parameter");

            _logger.Information($"Tapping element: {elementId}");
            // Implementation would interact with device
            await Task.Delay(100);
        }

        private async Task HandleEnterText(string deviceUdid, IScenarioStep step, IStepExecutionResult result)
        {
            if (!step.Parameters.TryGetValue("text", out var text))
                throw new ScenarioExecutionException("EnterText requires 'text' parameter");

            _logger.Information($"Entering text: {text}");
            // Implementation would interact with device
            await Task.Delay(100);
        }

        private async Task HandleVerifyElement(string deviceUdid, IScenarioStep step, IStepExecutionResult result)
        {
            var screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots", $"verify_{Guid.NewGuid()}.png");
            await _deviceManager.TakeScreenshotAsync(deviceUdid, screenshotPath);

            var elements = await _visualEngine.DetectUIElementsAsync(screenshotPath);
            result.CapturedData["detectedElements"] = elements.Count();
        }

        private async Task HandleExtractText(string deviceUdid, IScenarioStep step, IStepExecutionResult result)
        {
            var screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots", $"ocr_{Guid.NewGuid()}.png");
            await _deviceManager.TakeScreenshotAsync(deviceUdid, screenshotPath);

            var extractedText = await _visualEngine.PerformOCRAsync(screenshotPath);
            result.CapturedData["extractedText"] = extractedText;
        }

        private async Task HandleDelay(IScenarioStep step, IStepExecutionResult result)
        {
            if (step.Parameters.TryGetValue("duration", out var duration))
            {
                var delayMs = Convert.ToInt32(duration);
                await Task.Delay(delayMs);
            }
        }
    }
}

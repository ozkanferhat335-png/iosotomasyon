using IOSAutomation.Desktop;
using Serilog;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine("Logs", "automation-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("================== iOS Automation System Starting ==================");

            // Initialize the application
            var app = new AutomationApplication();
            await app.InitializeAsync();

            Log.Information("Application initialized successfully");

            // Example 1: Run a single scenario
            await RunSingleScenarioExample(app);

            // Example 2: Run multiple scenarios
            // await RunMultipleScenariosExample(app);

            // Example 3: Generate reports
            await app.GenerateReportsAsync();

            Log.Information("All tests completed successfully");
            Log.Information("================== iOS Automation System Stopping ===================");

            app.Shutdown();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            Environment.Exit(1);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Example: Run a single scenario on a device
    /// </summary>
    static async Task RunSingleScenarioExample(AutomationApplication app)
    {
        Log.Information("Running single scenario example...");

        try
        {
            // Get first connected device
            var deviceManager = new IOSAutomation.DeviceManagement.Services.IosDeviceManager();
            var devices = await deviceManager.GetConnectedDevicesAsync();

            if (devices.Count() == 0)
            {
                Log.Warning("No connected devices found. Skipping scenario execution.");
                return;
            }

            var device = devices.First();
            Log.Information($"Using device: {device}");

            // Run the login scenario
            var scenarioPath = "scenarios/example-login-scenario.json";
            if (!File.Exists(scenarioPath))
            {
                Log.Warning($"Scenario file not found: {scenarioPath}");
                return;
            }

            Log.Information($"Executing scenario: {scenarioPath}");
            var result = await app.RunScenarioAsync(scenarioPath, device.UDID);

            // Display results
            LogExecutionResult(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error running single scenario example");
        }
    }

    /// <summary>
    /// Example: Run multiple scenarios on all connected devices
    /// </summary>
    static async Task RunMultipleScenariosExample(AutomationApplication app)
    {
        Log.Information("Running multiple scenarios example...");

        try
        {
            var scenarioPaths = new[]
            {
                "scenarios/example-login-scenario.json",
                "scenarios/example-form-scenario.json",
                "scenarios/example-ui-test-scenario.json"
            };

            // Filter existing scenarios
            var existingScenarios = scenarioPaths.Where(File.Exists).ToArray();
            
            if (existingScenarios.Length == 0)
            {
                Log.Warning("No scenario files found");
                return;
            }

            Log.Information($"Running {existingScenarios.Length} scenarios");
            var results = await app.RunScenariosAsync(existingScenarios);

            // Display summary
            Log.Information($"Executed {results.Count} test runs");
            var passedCount = results.Values.Count(r => r.Status == IOSAutomation.Core.Interfaces.ExecutionStatus.Passed);
            var failedCount = results.Values.Count(r => r.Status == IOSAutomation.Core.Interfaces.ExecutionStatus.Failed);
            Log.Information($"Passed: {passedCount}, Failed: {failedCount}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error running multiple scenarios example");
        }
    }

    /// <summary>
    /// Logs the execution results in a readable format
    /// </summary>
    static void LogExecutionResult(IOSAutomation.Core.Interfaces.IScenarioExecutionResult result)
    {
        Log.Information("================== Scenario Execution Result ===================");
        Log.Information($"Scenario: {result.Scenario.Name}");
        Log.Information($"Device: {result.DeviceUdid}");
        Log.Information($"Status: {result.Status}");
        Log.Information($"Duration: {result.Duration.TotalSeconds:F2}s");
        Log.Information($"Passed Steps: {result.PassedSteps}/{result.StepResults.Count}");
        Log.Information($"Failed Steps: {result.FailedSteps}/{result.StepResults.Count}");
        Log.Information($"Success Rate: {result.SuccessRate:P}");

        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            Log.Warning($"Error: {result.ErrorMessage}");
        }

        // Log step details
        foreach (var stepResult in result.StepResults)
        {
            var statusIcon = stepResult.Status == IOSAutomation.Core.Interfaces.ExecutionStatus.Passed ? "✓" : "✗";
            Log.Information($"{statusIcon} Step {stepResult.Step.Id}: {stepResult.Step.Description} ({stepResult.Duration.TotalMilliseconds:F0}ms)");
            
            if (!string.IsNullOrEmpty(stepResult.ErrorMessage))
            {
                Log.Warning($"  Error: {stepResult.ErrorMessage}");
            }
        }

        Log.Information("================================================================");
    }
}

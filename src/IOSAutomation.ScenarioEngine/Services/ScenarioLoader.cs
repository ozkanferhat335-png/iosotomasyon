namespace IOSAutomation.ScenarioEngine.Services
{
    using IOSAutomation.Core.Interfaces;
    using IOSAutomation.Core.Exceptions;
    using IOSAutomation.ScenarioEngine.Models;
    using Newtonsoft.Json;
    using Serilog;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Loads scenarios from JSON files
    /// </summary>
    public class ScenarioLoader
    {
        private readonly ILogger _logger;

        public ScenarioLoader()
        {
            _logger = Log.Logger;
        }

        /// <summary>
        /// Loads a scenario from a JSON file
        /// </summary>
        public async Task<IScenario> LoadScenarioAsync(string scenarioFilePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Loading scenario from {scenarioFilePath}");
                    
                    if (!File.Exists(scenarioFilePath))
                        throw new ScenarioExecutionException($"Scenario file not found: {scenarioFilePath}");

                    var json = File.ReadAllText(scenarioFilePath);
                    var scenario = JsonConvert.DeserializeObject<Scenario>(json);

                    if (scenario == null)
                        throw new ScenarioExecutionException($"Failed to deserialize scenario from {scenarioFilePath}");

                    _logger.Information($"Scenario loaded: {scenario.Name} ({scenario.Id})");
                    return scenario;
                }
                catch (JsonException ex)
                {
                    throw new ScenarioExecutionException($"Invalid JSON in scenario file: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new ScenarioExecutionException($"Failed to load scenario: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Saves a scenario to a JSON file
        /// </summary>
        public async Task SaveScenarioAsync(IScenario scenario, string outputPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Saving scenario to {outputPath}");
                    
                    var outputDir = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    var json = JsonConvert.SerializeObject(scenario, Formatting.Indented);
                    File.WriteAllText(outputPath, json);

                    _logger.Information($"Scenario saved successfully");
                }
                catch (Exception ex)
                {
                    throw new ScenarioExecutionException($"Failed to save scenario: {ex.Message}", ex);
                }
            });
        }
    }
}

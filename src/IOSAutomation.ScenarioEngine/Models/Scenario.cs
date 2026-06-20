namespace IOSAutomation.ScenarioEngine.Models
{
    using IOSAutomation.Core.Interfaces;
    using System.Collections.Generic;

    /// <summary>
    /// Implementation of a test scenario
    /// </summary>
    public class Scenario : IScenario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IReadOnlyList<IScenarioStep> Steps { get; set; } = new List<IScenarioStep>();
        public string TargetAppBundleId { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Implementation of a scenario step
    /// </summary>
    public class ScenarioStep : IScenarioStep
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; } = string.Empty;
        public StepActionType ActionType { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public IEnumerable<IExpectationCriteria> ExpectationCriteria { get; set; } = new List<IExpectationCriteria>();
    }

    /// <summary>
    /// Implementation of expectation criteria
    /// </summary>
    public class ExpectationCriteria : IExpectationCriteria
    {
        public ExpectationType ExpectationType { get; set; }
        public object? Value { get; set; }
        public ComparisonOperator Operator { get; set; }
    }
}

namespace IOSAutomation.Core.Interfaces
{
    /// <summary>
    /// Represents a test scenario
    /// </summary>
    public interface IScenario
    {
        /// <summary>Gets the scenario unique identifier</summary>
        string Id { get; }

        /// <summary>Gets the scenario name</summary>
        string Name { get; }

        /// <summary>Gets the scenario description</summary>
        string Description { get; }

        /// <summary>Gets the list of steps in this scenario</summary>
        IReadOnlyList<IScenarioStep> Steps { get; }

        /// <summary>Gets the target application bundle ID</summary>
        string TargetAppBundleId { get; }

        /// <summary>Gets custom parameters for the scenario</summary>
        Dictionary<string, object> Parameters { get; }
    }

    /// <summary>
    /// Represents a single step in a scenario
    /// </summary>
    public interface IScenarioStep
    {
        /// <summary>Gets the step unique identifier</summary>
        string Id { get; }

        /// <summary>Gets the step description</summary>
        string Description { get; }

        /// <summary>Gets the type of action to perform</summary>
        StepActionType ActionType { get; }

        /// <summary>Gets the step parameters</summary>
        Dictionary<string, object> Parameters { get; }

        /// <summary>Gets the expected result criteria</summary>
        IEnumerable<IExpectationCriteria> ExpectationCriteria { get; }
    }

    /// <summary>
    /// Represents expected result criteria for a step
    /// </summary>
    public interface IExpectationCriteria
    {
        /// <summary>Gets the criteria type</summary>
        ExpectationType ExpectationType { get; }

        /// <summary>Gets the criteria value</summary>
        object? Value { get; }

        /// <summary>Gets the comparison operator</summary>
        ComparisonOperator Operator { get; }
    }

    /// <summary>Types of actions that can be performed in a step</summary>
    public enum StepActionType
    {
        OpenApp,
        CloseApp,
        TapElement,
        LongTapElement,
        EnterText,
        ClearTextField,
        SwipeScreen,
        ScrollScreen,
        WaitForElement,
        CaptureScreenshot,
        ExtractText,
        VerifyElement,
        Delay,
        Custom
    }

    /// <summary>Types of expectations for scenario results</summary>
    public enum ExpectationType
    {
        ElementVisible,
        ElementNotVisible,
        TextMatches,
        TextContains,
        Screenshot,
        State,
        Custom
    }

    /// <summary>Comparison operators for expectations</summary>
    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        Contains,
        StartsWith,
        EndsWith,
        GreaterThan,
        LessThan
    }
}

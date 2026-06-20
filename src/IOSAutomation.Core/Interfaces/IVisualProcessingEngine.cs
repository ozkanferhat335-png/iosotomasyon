namespace IOSAutomation.Core.Interfaces
{
    /// <summary>
    /// Handles visual processing and UI element detection
    /// </summary>
    public interface IVisualProcessingEngine
    {
        /// <summary>Analyzes a screenshot and detects UI elements</summary>
        Task<IEnumerable<IVisualElement>> DetectUIElementsAsync(string screenshotPath);

        /// <summary>Performs OCR on the screenshot to extract text</summary>
        Task<string> PerformOCRAsync(string screenshotPath);

        /// <summary>Matches a template image against the screenshot</summary>
        Task<IVisualElement?> MatchTemplateAsync(string screenshotPath, string templatePath, double threshold = 0.7);

        /// <summary>Detects buttons in the screenshot</summary>
        Task<IEnumerable<IVisualElement>> DetectButtonsAsync(string screenshotPath);

        /// <summary>Detects text fields in the screenshot</summary>
        Task<IEnumerable<IVisualElement>> DetectTextFieldsAsync(string screenshotPath);

        /// <summary>Detects alert dialogs in the screenshot</summary>
        Task<IVisualElement?> DetectAlertDialogAsync(string screenshotPath);

        /// <summary>Classifies the current screen state based on visual content</summary>
        Task<string> ClassifyScreenAsync(string screenshotPath);

        /// <summary>Compares two screenshots for visual differences</summary>
        Task<double> CompareScreenshotsAsync(string screenshotPath1, string screenshotPath2);
    }
}

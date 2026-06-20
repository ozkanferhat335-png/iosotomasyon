namespace IOSAutomation.VisualProcessing.Models
{
    using IOSAutomation.Core.Interfaces;

    /// <summary>
    /// Implementation of a visual element detected on screen
    /// </summary>
    public class VisualElement : IVisualElement
    {
        public UIElementType ElementType { get; set; }
        public string? Text { get; set; }
        public Rectangle Bounds { get; set; }
        public double Confidence { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();

        public override string ToString()
        {
            return $"{ElementType} at ({Bounds.X}, {Bounds.Y}) - {Bounds.Width}x{Bounds.Height} - Confidence: {Confidence:P}";
        }
    }
}

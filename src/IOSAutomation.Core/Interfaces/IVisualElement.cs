namespace IOSAutomation.Core.Interfaces
{
    /// <summary>
    /// Represents a detected UI element on the screen
    /// </summary>
    public interface IVisualElement
    {
        /// <summary>Gets the element type</summary>
        UIElementType ElementType { get; }

        /// <summary>Gets the element's text content</summary>
        string? Text { get; }

        /// <summary>Gets the element's bounding rectangle</summary>
        Rectangle Bounds { get; }

        /// <summary>Gets the confidence level of detection (0-1)</summary>
        double Confidence { get; }

        /// <summary>Gets additional metadata about the element</summary>
        Dictionary<string, object> Metadata { get; }
    }

    /// <summary>Represents the types of UI elements that can be detected</summary>
    public enum UIElementType
    {
        Button,
        TextField,
        Label,
        Image,
        Switch,
        Slider,
        PickerView,
        TableView,
        CollectionView,
        AlertDialog,
        Popup,
        StatusBar,
        NavigationBar,
        TabBar,
        Unknown
    }

    /// <summary>Represents a rectangular area on screen</summary>
    public struct Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

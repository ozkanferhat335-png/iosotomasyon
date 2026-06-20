namespace IOSAutomation.VisualProcessing.Services
{
    using IOSAutomation.Core.Interfaces;
    using IOSAutomation.Core.Exceptions;
    using IOSAutomation.VisualProcessing.Models;
    using OpenCvSharp;
    using Serilog;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles visual processing and UI element detection using OpenCV
    /// </summary>
    public class VisualProcessingEngine : IVisualProcessingEngine
    {
        private readonly ILogger _logger;
        private readonly string _templatesPath;

        public VisualProcessingEngine(string? templatesPath = null)
        {
            _logger = Log.Logger;
            _templatesPath = templatesPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            
            if (!Directory.Exists(_templatesPath))
            {
                Directory.CreateDirectory(_templatesPath);
            }
        }

        /// <summary>
        /// Analyzes a screenshot and detects UI elements
        /// </summary>
        public async Task<IEnumerable<IVisualElement>> DetectUIElementsAsync(string screenshotPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Detecting UI elements in {screenshotPath}");
                    
                    if (!File.Exists(screenshotPath))
                        throw new VisualProcessingException($"Screenshot not found: {screenshotPath}");

                    using var image = Cv2.ImRead(screenshotPath);
                    if (image.Empty())
                        throw new VisualProcessingException($"Failed to load image: {screenshotPath}");

                    var elements = new List<IVisualElement>();

                    // Detect buttons
                    elements.AddRange(DetectButtons(image));

                    // Detect text fields
                    elements.AddRange(DetectTextFields(image));

                    // Detect other UI elements
                    elements.AddRange(DetectOtherElements(image));

                    _logger.Information($"Detected {elements.Count} UI elements");
                    return elements;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error detecting UI elements: {ex.Message}", ex);
                    throw new VisualProcessingException("Failed to detect UI elements", ex);
                }
            });
        }

        /// <summary>
        /// Performs OCR on the screenshot to extract text
        /// </summary>
        public async Task<string> PerformOCRAsync(string screenshotPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Performing OCR on {screenshotPath}");
                    
                    if (!File.Exists(screenshotPath))
                        throw new VisualProcessingException($"Screenshot not found: {screenshotPath}");

                    // Using basic text extraction via OpenCV edge detection and contours
                    using var image = Cv2.ImRead(screenshotPath);
                    if (image.Empty())
                        throw new VisualProcessingException($"Failed to load image: {screenshotPath}");

                    using var gray = new Mat();
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                    using var threshold = new Mat();
                    Cv2.Threshold(gray, threshold, 127, 255, ThresholdTypes.BinaryInv);

                    // This is a placeholder for Tesseract OCR integration
                    // In production, you would use Tesseract for actual text recognition
                    var extractedText = ExtractTextFromImage(image);
                    
                    _logger.Information($"OCR completed, extracted {extractedText.Length} characters");
                    return extractedText;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error performing OCR: {ex.Message}", ex);
                    throw new VisualProcessingException("Failed to perform OCR", ex);
                }
            });
        }

        /// <summary>
        /// Matches a template image against the screenshot
        /// </summary>
        public async Task<IVisualElement?> MatchTemplateAsync(string screenshotPath, string templatePath, double threshold = 0.7)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Matching template {templatePath} against {screenshotPath}");
                    
                    if (!File.Exists(screenshotPath))
                        throw new VisualProcessingException($"Screenshot not found: {screenshotPath}");
                    
                    if (!File.Exists(templatePath))
                        throw new VisualProcessingException($"Template not found: {templatePath}");

                    using var source = Cv2.ImRead(screenshotPath);
                    using var template = Cv2.ImRead(templatePath);

                    if (source.Empty() || template.Empty())
                        throw new VisualProcessingException("Failed to load source or template image");

                    using var result = new Mat();
                    Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);

                    Cv2.MinMaxLoc(result, out _, out var maxVal, out _, out var maxLoc);

                    if (maxVal >= threshold)
                    {
                        var element = new VisualElement
                        {
                            ElementType = UIElementType.Image,
                            Bounds = new Rectangle
                            {
                                X = maxLoc.X,
                                Y = maxLoc.Y,
                                Width = template.Width,
                                Height = template.Height
                            },
                            Confidence = maxVal,
                            Text = Path.GetFileNameWithoutExtension(templatePath)
                        };

                        _logger.Information($"Template matched with confidence {maxVal:P}");
                        return element;
                    }

                    _logger.Warning($"Template not matched (confidence {maxVal:P} below threshold {threshold:P})");
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error matching template: {ex.Message}", ex);
                    throw new VisualProcessingException("Failed to match template", ex);
                }
            });
        }

        /// <summary>
        /// Detects buttons in the screenshot
        /// </summary>
        public async Task<IEnumerable<IVisualElement>> DetectButtonsAsync(string screenshotPath)
        {
            return await Task.Run(() => DetectButtons(LoadImage(screenshotPath)));
        }

        /// <summary>
        /// Detects text fields in the screenshot
        /// </summary>
        public async Task<IEnumerable<IVisualElement>> DetectTextFieldsAsync(string screenshotPath)
        {
            return await Task.Run(() => DetectTextFields(LoadImage(screenshotPath)));
        }

        /// <summary>
        /// Detects alert dialogs in the screenshot
        /// </summary>
        public async Task<IVisualElement?> DetectAlertDialogAsync(string screenshotPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Detecting alert dialog in {screenshotPath}");
                    using var image = LoadImage(screenshotPath);
                    
                    // Look for centered rectangular shapes with specific characteristics
                    using var gray = new Mat();
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                    using var thresh = new Mat();
                    Cv2.Threshold(gray, thresh, 127, 255, ThresholdTypes.Binary);

                    Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                    // Find the largest rectangular contour in the center
                    foreach (var contour in contours)
                    {
                        var area = Cv2.ContourArea(contour);
                        if (area > 10000) // Minimum area for alert dialog
                        {
                            var rect = Cv2.BoundingRect(contour);
                            var element = new VisualElement
                            {
                                ElementType = UIElementType.AlertDialog,
                                Bounds = new Rectangle
                                {
                                    X = rect.X,
                                    Y = rect.Y,
                                    Width = rect.Width,
                                    Height = rect.Height
                                },
                                Confidence = Math.Min(area / (double)(image.Width * image.Height), 1.0)
                            };
                            
                            _logger.Information("Alert dialog detected");
                            return element;
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error detecting alert dialog: {ex.Message}", ex);
                    return null;
                }
            });
        }

        /// <summary>
        /// Classifies the current screen state based on visual content
        /// </summary>
        public async Task<string> ClassifyScreenAsync(string screenshotPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Classifying screen state for {screenshotPath}");
                    
                    using var image = LoadImage(screenshotPath);
                    
                    // Analyze image characteristics
                    using var gray = new Mat();
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                    var mean = Cv2.Mean(gray);
                    var meanValue = mean.Val0;

                    // Simple classification based on brightness
                    var classification = meanValue switch
                    {
                        < 50 => "DarkScreen",
                        < 100 => "LowBrightness",
                        < 200 => "NormalScreen",
                        _ => "BrightScreen"
                    };

                    _logger.Information($"Screen classified as: {classification}");
                    return classification;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error classifying screen: {ex.Message}", ex);
                    return "Unknown";
                }
            });
        }

        /// <summary>
        /// Compares two screenshots for visual differences
        /// </summary>
        public async Task<double> CompareScreenshotsAsync(string screenshotPath1, string screenshotPath2)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Information($"Comparing screenshots: {screenshotPath1} vs {screenshotPath2}");
                    
                    using var img1 = LoadImage(screenshotPath1);
                    using var img2 = LoadImage(screenshotPath2);

                    if (img1.Size() != img2.Size())
                    {
                        Cv2.Resize(img2, img2, img1.Size());
                    }

                    using var diff = new Mat();
                    Cv2.AbsDiff(img1, img2, diff);

                    using var gray = new Mat();
                    Cv2.CvtColor(diff, gray, ColorConversionCodes.BGR2GRAY);

                    var totalPixels = gray.Total();
                    var nonZeroPixels = Cv2.CountNonZero(gray);
                    var similarity = 1.0 - (nonZeroPixels / (double)totalPixels);

                    _logger.Information($"Screenshot similarity: {similarity:P}");
                    return similarity;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error comparing screenshots: {ex.Message}", ex);
                    return 0.0;
                }
            });
        }

        /// <summary>
        /// Detects buttons by analyzing color and shape characteristics
        /// </summary>
        private List<IVisualElement> DetectButtons(Mat image)
        {
            var buttons = new List<IVisualElement>();
            
            try
            {
                using var gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                using var thresh = new Mat();
                Cv2.Threshold(gray, thresh, 127, 255, ThresholdTypes.Binary);

                Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    if (area > 100 && area < 50000) // Typical button size
                    {
                        var rect = Cv2.BoundingRect(contour);
                        var approx = Cv2.ApproxPolyDP(contour, 0.02 * Cv2.ArcLength(contour, true), true);

                        // Check if it's roughly rectangular
                        if (approx.Length >= 4)
                        {
                            buttons.Add(new VisualElement
                            {
                                ElementType = UIElementType.Button,
                                Bounds = new Rectangle
                                {
                                    X = rect.X,
                                    Y = rect.Y,
                                    Width = rect.Width,
                                    Height = rect.Height
                                },
                                Confidence = 0.8
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Error detecting buttons: {ex.Message}");
            }

            return buttons;
        }

        /// <summary>
        /// Detects text fields by analyzing rectangular regions
        /// </summary>
        private List<IVisualElement> DetectTextFields(Mat image)
        {
            var textFields = new List<IVisualElement>();
            
            try
            {
                using var gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                using var thresh = new Mat();
                Cv2.Threshold(gray, thresh, 127, 255, ThresholdTypes.Binary);

                Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    var rect = Cv2.BoundingRect(contour);
                    
                    // Text fields are typically wider than tall
                    if (area > 50 && rect.Width > rect.Height * 2)
                    {
                        textFields.Add(new VisualElement
                        {
                            ElementType = UIElementType.TextField,
                            Bounds = new Rectangle
                            {
                                X = rect.X,
                                Y = rect.Y,
                                Width = rect.Width,
                                Height = rect.Height
                            },
                            Confidence = 0.7
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Error detecting text fields: {ex.Message}");
            }

            return textFields;
        }

        /// <summary>
        /// Detects other UI elements like labels and images
        /// </summary>
        private List<IVisualElement> DetectOtherElements(Mat image)
        {
            var elements = new List<IVisualElement>();
            
            try
            {
                using var gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                using var edges = new Mat();
                Cv2.Canny(gray, edges, 50, 150);

                Cv2.FindContours(edges, out var contours, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    if (area > 500 && area < 100000)
                    {
                        var rect = Cv2.BoundingRect(contour);
                        elements.Add(new VisualElement
                        {
                            ElementType = UIElementType.Label,
                            Bounds = new Rectangle
                            {
                                X = rect.X,
                                Y = rect.Y,
                                Width = rect.Width,
                                Height = rect.Height
                            },
                            Confidence = 0.6
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Error detecting other elements: {ex.Message}");
            }

            return elements;
        }

        /// <summary>
        /// Extracts text from image (placeholder for Tesseract integration)
        /// </summary>
        private string ExtractTextFromImage(Mat image)
        {
            // This is a placeholder implementation
            // In production, integrate with Tesseract OCR
            return "[OCR Text Extraction - Integration Required]";
        }

        /// <summary>
        /// Loads an image from file
        /// </summary>
        private Mat LoadImage(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new VisualProcessingException($"Image not found: {imagePath}");

            var image = Cv2.ImRead(imagePath);
            if (image.Empty())
                throw new VisualProcessingException($"Failed to load image: {imagePath}");

            return image;
        }
    }
}

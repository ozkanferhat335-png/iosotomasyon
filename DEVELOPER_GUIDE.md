# iOS Automation System - Developer Guide

## Project Structure Overview

```
iosotomasyon/
├── src/
│   ├── IOSAutomation.Core/           # Core interfaces and models
│   ├── IOSAutomation.DeviceManagement/   # Device operations
│   ├── IOSAutomation.VisualProcessing/   # Visual analysis
│   ├── IOSAutomation.ScenarioEngine/     # Test execution
│   ├── IOSAutomation.Logging/            # Logging & reporting
│   └── IOSAutomation.Desktop/            # Desktop application
├── scenarios/                         # Test scenario definitions
├── Templates/                         # Visual templates for matching
├── Logs/                             # Application logs
├── Reports/                          # Generated reports
├── Screenshots/                      # Captured screenshots
├── IOSAutomation.sln                 # Solution file
├── README.md                         # Project overview
├── SETUP_GUIDE.md                    # Installation and setup
└── DEVELOPER_GUIDE.md                # This file
```

## Development Setup

### Prerequisites

- Visual Studio 2022+ or VS Code
- .NET 8 SDK
- Git
- libimobiledevice (optional for device testing)

### Getting Started

1. Clone the repository
2. Open `IOSAutomation.sln` in Visual Studio
3. Restore NuGet packages: `dotnet restore`
4. Build solution: `dotnet build`
5. Run unit tests (when available)

## Module Development Guide

### IOSAutomation.Core

**Purpose**: Define all interfaces and abstract contracts

**Key Responsibilities**:
- Define service interfaces
- Define data models and enums
- Define custom exceptions
- Provide common interfaces for cross-module communication

**Development Guidelines**:
- Keep interfaces focused and single-responsibility
- Use async/await for I/O operations
- Document all public interfaces
- Don't add implementation details

**Key Files**:
- `Interfaces/IDevice.cs` - Device contract
- `Interfaces/IDeviceManager.cs` - Device operations
- `Interfaces/IVisualProcessingEngine.cs` - Visual processing
- `Interfaces/IScenarioEngine.cs` - Scenario execution
- `Interfaces/ILogger.cs` - Logging contract
- `Exceptions/AutomationException.cs` - Error handling

### IOSAutomation.DeviceManagement

**Purpose**: Implement iOS device operations

**Key Responsibilities**:
- Detect connected devices
- Manage application lifecycle
- Capture screenshots
- Retrieve device logs
- Monitor device connection/disconnection

**Architecture**:
```
IosDeviceManager (IDeviceManager)
├── Device detection via idevice_id
├── Device info via ideviceinfo
├── App management via ideviceinstaller
├── Screenshots via idevicescreenshot
├── Logs via idevicesyslog
└── Device monitoring thread
```

**Development Guidelines**:
- Use `ILogger` for consistent logging
- Implement proper error handling with custom exceptions
- Use async operations for long-running tasks
- Validate device state before operations
- Handle missing devices gracefully

**Testing**:
- Mock device operations for unit tests
- Test error scenarios (disconnected device, invalid UDID)
- Verify event notifications

**Example Usage**:
```csharp
var deviceManager = new IosDeviceManager();
var devices = await deviceManager.GetConnectedDevicesAsync();
var device = await deviceManager.GetDeviceAsync(udid);
await deviceManager.InstallApplicationAsync(udid, appPath);
var screenshotPath = await deviceManager.TakeScreenshotAsync(udid, outputPath);
```

### IOSAutomation.VisualProcessing

**Purpose**: Handle visual analysis and UI element detection

**Key Responsibilities**:
- Detect UI elements (buttons, text fields, alerts)
- Perform template matching
- Extract text via OCR
- Compare screenshots
- Classify screen states

**Architecture**:
```
VisualProcessingEngine (IVisualProcessingEngine)
├── OpenCV Image Processing
│   ├── Element detection (contour analysis)
│   ├── Template matching
│   └── Screenshot comparison
├── OCR Integration
│   └── Text extraction
└── Screen Classification
    └── State analysis
```

**Development Guidelines**:
- Use OpenCV for image processing
- Optimize performance with image resizing
- Implement proper error handling for invalid images
- Cache templates for performance
- Validate confidence thresholds

**Detection Algorithm**:
1. Load screenshot as Mat
2. Convert to grayscale
3. Apply threshold or edge detection
4. Find contours
5. Analyze contour properties
6. Classify as UI element type
7. Calculate bounding rectangle and confidence

**Example Usage**:
```csharp
var visualEngine = new VisualProcessingEngine();
var elements = await visualEngine.DetectUIElementsAsync(screenshotPath);
var buttons = await visualEngine.DetectButtonsAsync(screenshotPath);
var matchedElement = await visualEngine.MatchTemplateAsync(screenshotPath, templatePath, 0.7);
var text = await visualEngine.PerformOCRAsync(screenshotPath);
```

### IOSAutomation.ScenarioEngine

**Purpose**: Execute test scenarios with step-by-step control

**Key Responsibilities**:
- Load scenarios from JSON
- Execute steps sequentially
- Handle step actions
- Verify expectations
- Track execution results
- Support pause/resume/stop

**Architecture**:
```
ScenarioEngine (IScenarioEngine)
├── ScenarioLoader
│   ├── JSON deserialization
│   └── Scenario validation
├── Step Execution
│   ├── Action handlers
│   ├── Expectation validators
│   └── Result tracking
└── Execution Control
    ├── Pause/Resume
    └── Cancellation
```

**Step Action Handlers**:
- `HandleTakeScreenshot()` - Capture screen
- `HandleTapElement()` - Tap screen location
- `HandleEnterText()` - Type text
- `HandleVerifyElement()` - Verify element
- `HandleExtractText()` - Extract text via OCR
- `HandleDelay()` - Wait

**Development Guidelines**:
- Each step should be independent
- Implement timeout handling
- Log all step execution
- Capture data for reporting
- Support custom step types via plugins

**Example Scenario Structure**:
```json
{
  "id": "scenario-001",
  "name": "Test Name",
  "targetAppBundleId": "com.example.app",
  "steps": [
    {
      "id": "step-001",
      "actionType": "TakeScreenshot",
      "expectationCriteria": []
    }
  ]
}
```

### IOSAutomation.Logging

**Purpose**: Centralized logging and reporting

**Key Responsibilities**:
- Structured logging with Serilog
- Test execution tracking
- Report generation
- Statistics calculation
- HTML report creation

**Architecture**:
```
Logging System
├── Serilog Configuration
│   ├── Console sink
│   ├── File sink (daily rolling)
│   └── Structured properties
├── TestExecutionLogger
│   ├── Scenario events
│   ├── Step events
│   └── Device events
└── ReportGenerator
    ├── Daily reports
    ├── Weekly reports
    ├── Scenario reports
    └── Statistics reports
```

**Log Levels**:
- **Debug**: Detailed diagnostic info
- **Information**: General info (default)
- **Warning**: Warnings and recoverable errors
- **Error**: Errors requiring attention
- **Fatal**: Critical failures

**Report Types**:
1. **Scenario Reports**: Individual test execution details
2. **Daily Reports**: Day-by-day statistics
3. **Weekly Reports**: Week-by-week analysis
4. **Statistics Reports**: Period-based metrics

**Example Usage**:
```csharp
var logger = new TestExecutionLogger();
logger.LogScenarioStart(scenarioId, deviceUdid);
logger.LogStepExecution(stepId, action, success);
logger.LogScreenshot(deviceUdid, path);

var reportGen = new ReportGenerator();
var report = await reportGen.GenerateDailyReportAsync(DateTime.Now, "daily.html");
```

### IOSAutomation.Desktop

**Purpose**: Main application entry point

**Current Implementation**:
- `AutomationApplication` class
- Application lifecycle management
- Scenario orchestration

**Future Development**:
- Windows Forms UI
- WebSocket remote control
- Configuration UI
- Device management dashboard

**Development Guidelines**:
- Keep UI separate from business logic
- Use dependency injection
- Implement proper error handling
- Provide user feedback

## Adding New Features

### Add a New Step Action Type

1. **Add to enum** in `Core/Interfaces/IScenario.cs`:
   ```csharp
   public enum StepActionType {
       // ... existing types
       MyCustomAction
   }
   ```

2. **Implement handler** in `ScenarioEngine/Services/ScenarioEngine.cs`:
   ```csharp
   case StepActionType.MyCustomAction:
       await HandleMyCustomAction(deviceUdid, step, result);
       break;
   
   private async Task HandleMyCustomAction(...) {
       // Implementation
   }
   ```

3. **Update scenario** with new step type

### Add Device Operations

1. **Add method to interface** in `Core/Interfaces/IDeviceManager.cs`
2. **Implement in** `DeviceManagement/Services/IosDeviceManager.cs`
3. **Add test scenarios** using new operation

### Add Visual Detection

1. **Add method to interface** in `Core/Interfaces/IVisualProcessingEngine.cs`
2. **Implement detection algorithm** in `VisualProcessing/Services/VisualProcessingEngine.cs`
3. **Test with sample screenshots**

## Testing Strategy

### Unit Testing

Test individual components in isolation:

```csharp
[TestClass]
public class IosDeviceManagerTests
{
    [TestMethod]
    public async Task GetConnectedDevices_ReturnsDevices()
    {
        // Arrange
        var manager = new IosDeviceManager();
        
        // Act
        var devices = await manager.GetConnectedDevicesAsync();
        
        // Assert
        Assert.IsNotNull(devices);
    }
}
```

### Integration Testing

Test component interactions:
- Scenario Engine with Device Manager
- Visual Processing with screenshots
- Logging with report generation

### Device Testing

Test on actual devices:
- Run example scenarios
- Verify screenshot capture
- Test device detection
- Validate error handling

## Debugging

### Enabling Debug Logging

Set log level to Debug:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("log.txt")
    .CreateLogger();
```

### Common Issues

**Device Not Detected**
- Check USB connection
- Verify libimobiledevice installation
- Run `idevice_id -l`

**Screenshot Fails**
- Verify device connectivity
- Check file permissions
- Ensure disk space available

**Visual Detection Issues**
- Adjust template matching threshold
- Verify screenshot quality
- Check image loading

## Performance Considerations

### Optimization Tips

1. **Image Processing**: Resize images before processing
2. **Template Matching**: Use downsampled templates
3. **Parallel Execution**: Process multiple devices simultaneously
4. **Caching**: Cache template matches
5. **Async Operations**: Use async/await properly

### Profiling

Use Visual Studio profiler to identify bottlenecks:
- CPU usage by module
- Memory allocation
- I/O performance

## Code Standards

### Naming Conventions
- Classes: PascalCase (`IosDeviceManager`)
- Methods: PascalCase (`GetDeviceAsync`)
- Properties: PascalCase (`IsConnected`)
- Private fields: camelCase with underscore (`_logger`)
- Parameters: camelCase (`deviceUdid`)

### Documentation
- All public members should have XML comments
- Include parameter descriptions
- Document return values
- Note exceptions thrown

### Error Handling
- Use custom exceptions
- Log errors appropriately
- Provide meaningful error messages
- Clean up resources in finally blocks

## Contributing

### Pull Request Process

1. Create feature branch
2. Make changes following code standards
3. Add/update tests
4. Update documentation
5. Submit PR with description

### Code Review Checklist

- [ ] Follows naming conventions
- [ ] Properly documented
- [ ] Error handling implemented
- [ ] Tests added
- [ ] No hardcoded values
- [ ] Async/await used correctly
- [ ] Logging present

## References

- [.NET Best Practices](https://docs.microsoft.com/en-us/dotnet/fundamentals/)
- [OpenCV Documentation](https://docs.opencv.org/)
- [Serilog Documentation](https://serilog.net/)
- [libimobiledevice](https://libimobiledevice.org/)
- Project README: See `README.md`

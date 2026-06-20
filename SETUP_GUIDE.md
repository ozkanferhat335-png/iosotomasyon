# Setup Guide - iOS Automation System

## Prerequisites

### System Requirements
- Windows 10/11 64-bit
- .NET 8 SDK or Runtime
- 4GB RAM minimum
- USB ports for iOS device connection

### Required Software

1. **libimobiledevice** - For iOS device communication
   - Download: https://github.com/libimobiledevice/libimobiledevice
   - Install to PATH or configure in application

2. **OpenCV 4.8** - Visual processing (NuGet package, auto-installed)

3. **Tesseract OCR** - Text extraction
   - Download: https://github.com/UB-Mannheim/tesseract/wiki
   - Install and add to PATH

## Installation

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/iosotomasyon.git
cd iosotomasyon
```

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Build the Solution
```bash
dotnet build
```

### 4. Configure libimobiledevice

#### Option A: Add to PATH (Recommended)
1. Install libimobiledevice
2. Add installation directory to system PATH
3. Verify installation:
   ```bash
   idevice_id -l
   ```

#### Option B: Configure Manually
Edit `src/IOSAutomation.DeviceManagement/Services/IosDeviceManager.cs`:
```csharp
private readonly string _libimobiledevicePath = @"C:\path\to\libimobiledevice\bin";
```

### 5. Configure Directories

Create required directories:
```bash
mkdir Logs
mkdir Reports
mkdir Screenshots
mkdir Scenarios
mkdir Templates
```

## Device Setup

### Connect iOS Device

1. **Physical Connection**
   - Connect iPad/iPhone to Windows PC via USB cable
   - Unlock the device
   - Tap "Trust" if prompted

2. **Verify Connection**
   ```bash
   idevice_id -l
   ```
   Should output device UDID(s)

3. **Install Test App**
   ```bash
   ideviceinstaller -u <UDID> -i path\to\app.ipa
   ```

### Device Permissions
- Ensure Developer Mode is enabled (iOS 16+)
- Allow necessary app permissions
- Disable auto-lock if possible

## Configuration

### Application Settings

Create `appsettings.json` in the Desktop project root:

```json
{
  "DeviceManagement": {
    "ConnectionTimeout": 30000,
    "CommandTimeout": 60000,
    "MonitoringInterval": 5000
  },
  "VisualProcessing": {
    "TemplatesDirectory": "./Templates",
    "TemplateMatchThreshold": 0.7,
    "EnableOCR": true
  },
  "ScenarioEngine": {
    "DefaultTimeout": 30000,
    "MaxParallelExecutions": 2,
    "ScreenshotDirectory": "./Screenshots"
  },
  "Logging": {
    "LogsDirectory": "./Logs",
    "ReportsDirectory": "./Reports",
    "LogLevel": "Information",
    "RetentionDays": 30
  }
}
```

## Running Tests

### Command Line Usage

```csharp
// Example usage in Main()
var app = new AutomationApplication();
await app.InitializeAsync();

// Run single scenario
var result = await app.RunScenarioAsync(
    "scenarios/example-login-scenario.json",
    "<device-udid>"
);

// Run multiple scenarios
var results = await app.RunScenariosAsync(new[] {
    "scenarios/example-login-scenario.json",
    "scenarios/example-form-scenario.json"
});

// Generate reports
await app.GenerateReportsAsync();

app.Shutdown();
```

## Creating Test Scenarios

### Scenario File Structure

Scenarios are defined in JSON format. See `scenarios/` folder for examples.

#### Key Properties:

- **id**: Unique scenario identifier
- **name**: Human-readable name
- **description**: Scenario purpose
- **targetAppBundleId**: iOS app bundle identifier
- **parameters**: Global scenario parameters
- **steps**: Array of test steps

#### Step Structure:

```json
{
  "id": "step-001",
  "description": "Step description",
  "actionType": "ActionType",
  "parameters": {
    "param1": "value1"
  },
  "expectationCriteria": [
    {
      "expectationType": "ExpectationType",
      "value": "expected_value",
      "operator": "Equals"
    }
  ]
}
```

### Supported Action Types

- **TakeScreenshot**: Captures device screen
- **TapElement**: Taps on screen element
- **LongTapElement**: Long press on element
- **EnterText**: Types text into focused field
- **ClearTextField**: Clears text field content
- **SwipeScreen**: Swipes across screen
- **ScrollScreen**: Scrolls content
- **WaitForElement**: Waits for element to appear
- **ExtractText**: Performs OCR
- **VerifyElement**: Verifies element presence
- **Delay**: Waits specified milliseconds

### Supported Expectation Types

- **ElementVisible**: Element should be visible
- **ElementNotVisible**: Element should not be visible
- **TextMatches**: Text exactly matches
- **TextContains**: Text contains substring
- **Screenshot**: Screenshot verification
- **State**: State verification

## Troubleshooting

### Device Not Detected

```bash
# Check device connection
idevice_id -l

# Check device info
ideviceinfo -u <UDID>

# Restart USB daemon
usbmuxd --exit
```

### Device Connection Issues

1. Restart the device
2. Disconnect and reconnect USB
3. Check USB cable quality
4. Update device iOS version
5. Reinstall libimobiledevice

### Scenario Execution Failures

1. Check device logs:
   ```bash
   idevicesyslog
   ```

2. Verify app is installed:
   ```bash
   ideviceinstaller -u <UDID> -l
   ```

3. Check scenario JSON validity
4. Review application logs in `./Logs`

### OCR Not Working

1. Verify Tesseract installation:
   ```bash
   tesseract --version
   ```

2. Ensure Tesseract is in PATH
3. Check Tesseract data files location

## Logging and Debugging

### Log Files Location
- Main logs: `./Logs/automation-*.log`
- Daily rotation: One file per day
- Format: `[Timestamp] [Level] Message`

### Enable Debug Logging

In `appsettings.json`:
```json
"Logging": {
  "LogLevel": "Debug"
}
```

### Viewing Logs

```bash
# Real-time log viewing
Get-Content -Path Logs/automation-*.log -Wait

# Filter by level
Select-String -Path Logs/automation-*.log -Pattern "ERROR"
```

## Reports

### Available Reports

1. **Daily Report**: `./Reports/DailyReport_yyyy-MM-dd.html`
2. **Weekly Report**: `./Reports/WeeklyReport_yyyy-MM-dd.html`
3. **Statistics Report**: `./Reports/StatisticsReport_yyyy-MM-dd.html`
4. **Scenario Reports**: `./Reports/Scenario_<id>.html`

### Report Contents

- Test execution statistics
- Pass/fail rates
- Execution timings
- Step-by-step results
- Screenshots
- Error messages

## Performance Tips

1. **Parallel Execution**: Run scenarios on multiple devices simultaneously
2. **Screenshot Optimization**: Reduce screenshot frequency for speed
3. **Template Matching**: Use high-quality templates for accuracy
4. **Device Cleanup**: Clear cache/logs periodically

## Advanced Features

### Custom Step Handlers

Extend `ScenarioEngine` to add custom actions:

```csharp
private async Task HandleCustomAction(string deviceUdid, IScenarioStep step, IStepExecutionResult result)
{
    // Custom implementation
}
```

### Plugin System

(Documentation for plugin development will be added)

## Security

### Best Practices

1. Don't commit device UDIDs to version control
2. Secure screenshot storage
3. Encrypt sensitive test data
4. Use environment variables for credentials
5. Audit log access

## Support and Troubleshooting

For issues and questions:

1. Check application logs in `./Logs`
2. Consult troubleshooting section above
3. Review scenario file format
4. Verify device setup

## Next Steps

1. Create your first scenario file
2. Connect an iOS device
3. Run the example scenarios
4. Customize for your app
5. Set up automated scheduling

## Additional Resources

- [libimobiledevice Documentation](https://libimobiledevice.org/)
- [OpenCV C# Documentation](https://github.com/shimat/opencvsharp)
- [Tesseract OCR](https://github.com/UB-Mannheim/tesseract)
- Project Repository: https://github.com/yourusername/iosotomasyon

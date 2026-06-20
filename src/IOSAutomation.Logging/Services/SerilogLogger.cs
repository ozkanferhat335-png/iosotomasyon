namespace IOSAutomation.Logging.Services
{
    using IOSAutomation.Core.Interfaces;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using System;

    /// <summary>
    /// Adapter to use Serilog as ILogger
    /// </summary>
    public class SerilogLogger : ILogger
    {
        private readonly Serilog.ILogger _logger;

        public SerilogLogger(Serilog.ILogger? logger = null)
        {
            _logger = logger ?? Log.Logger;
        }

        public void LogInfo(string message) => _logger.Information(message);
        public void LogDebug(string message) => _logger.Debug(message);
        public void LogWarning(string message) => _logger.Warning(message);
        public void LogError(string message, Exception? exception = null) => _logger.Error(exception, message);
        public void LogCritical(string message, Exception? exception = null) => _logger.Fatal(exception, message);
    }
}

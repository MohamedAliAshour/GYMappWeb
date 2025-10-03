using GYMappWeb.Models;

namespace GYMappWeb.Interface
{
    public interface ILogging
    {
        // Basic logging methods
        Task LogInformationAsync(string message, string controller = null, string action = null);
        Task LogWarningAsync(string message, string controller = null, string action = null);
        Task LogErrorAsync(string message, Exception exception = null, string controller = null, string action = null);
        Task LogCriticalAsync(string message, Exception exception = null, string controller = null, string action = null);
        Task LogDebugAsync(string message, string controller = null, string action = null);

        // Custom logging with additional context
        Task LogAsync(LogEntryLevel level, string message, Exception exception = null,
                     string controller = null, string action = null, string user = null,
                     string url = null, string ipAddress = null, string httpMethod = null,
                     int? statusCode = null, long? duration = null);

        // Query methods
        Task<List<LogEntry>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null,
                                         string level = null, string controller = null);
        Task<LogEntry> GetLogByIdAsync(int id);
        Task<int> DeleteOldLogsAsync(DateTime cutoffDate);
        Task<int> GetLogCountAsync(DateTime? fromDate = null, string level = null);
    }

    public enum LogEntryLevel
    {
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }
}

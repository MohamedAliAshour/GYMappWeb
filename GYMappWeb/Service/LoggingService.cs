// Services/LoggingService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using GYMappWeb.Interface;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;

namespace GYMappWeb.Services
{
    public class LoggingService : ILogging
    {
        private readonly GYMappWebContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingService(GYMappWebContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogInformationAsync(string message, string controller = null, string action = null)
        {
            await LogAsync(LogEntryLevel.Information, message, null, controller, action);
        }

        public async Task LogWarningAsync(string message, string controller = null, string action = null)
        {
            await LogAsync(LogEntryLevel.Warning, message, null, controller, action);
        }

        public async Task LogErrorAsync(string message, Exception exception = null, string controller = null, string action = null)
        {
            await LogAsync(LogEntryLevel.Error, message, exception, controller, action);
        }

        public async Task LogCriticalAsync(string message, Exception exception = null, string controller = null, string action = null)
        {
            await LogAsync(LogEntryLevel.Critical, message, exception, controller, action);
        }

        public async Task LogDebugAsync(string message, string controller = null, string action = null)
        {
            await LogAsync(LogEntryLevel.Debug, message, null, controller, action);
        }

        public async Task LogAsync(LogEntryLevel level, string message, Exception exception = null,
                                 string controller = null, string action = null, string user = null,
                                 string url = null, string ipAddress = null, string httpMethod = null,
                                 int? statusCode = null, long? duration = null)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level.ToString(),
                Message = message,
                Exception = exception?.ToString(),
                Logger = "LoggingService",
                Controller = controller,
                Action = action,
                User = user ?? GetCurrentUser(),
                Url = url ?? GetCurrentUrl(),
                IpAddress = ipAddress ?? GetCurrentIpAddress(),
                HttpMethod = httpMethod ?? GetCurrentHttpMethod(),
                StatusCode = statusCode,
                Duration = duration
            };

            try
            {
                _context.Logs.Add(logEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Fallback to console logging if database logging fails
                Console.WriteLine($"Failed to log to database: {ex.Message}");
                Console.WriteLine($"Original log: {level} - {message}");
            }
        }

        // ... rest of the methods remain the same
        public async Task<List<LogEntry>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null,
                                                      string level = null, string controller = null)
        {
            var query = _context.Logs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.Timestamp <= toDate.Value);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(l => l.Level == level);

            if (!string.IsNullOrEmpty(controller))
                query = query.Where(l => l.Controller == controller);

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<LogEntry> GetLogByIdAsync(int id)
        {
            return await _context.Logs.FindAsync(id);
        }

        public async Task<int> DeleteOldLogsAsync(DateTime cutoffDate)
        {
            var oldLogs = _context.Logs.Where(l => l.Timestamp < cutoffDate);
            _context.Logs.RemoveRange(oldLogs);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> GetLogCountAsync(DateTime? fromDate = null, string level = null)
        {
            var query = _context.Logs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.Timestamp >= fromDate.Value);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(l => l.Level == level);

            return await query.CountAsync();
        }

        // Helper methods to get current request context
        private string GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        }

        private string GetCurrentUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request != null ? $"{request.Method} {request.Path}" : null;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string GetCurrentHttpMethod()
        {
            return _httpContextAccessor.HttpContext?.Request?.Method;
        }
    }
}
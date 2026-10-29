using Microsoft.Extensions.Logging;
using ProjectBuilder.Helpers.Logger;
using System;

namespace ProjectBuilder.Helpers
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public AppLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Info(string message, params object[] args)
        {
            _logger.LogInformation("[INFO] " + message, args);
        }

        public void Warning(string message, params object[] args)
        {
            _logger.LogWarning("[WARNING] " + message, args);
        }

        public void Debug(string message, params object[] args)
        {
            _logger.LogDebug("[DEBUG] " + message, args);
        }

        public void Error(string message, params object[] args)
        {
            _logger.LogError("[ERROR] " + message, args);
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, "[ERROR] " + message, args);
        }

        public void Critical(Exception ex, string message, params object[] args)
        {
            _logger.LogCritical(ex, "[CRITICAL] " + message, args);
        }

        public void DatabaseInfo(string message, params object[] args)
        {
            _logger.LogInformation("[DATABASE] " + message, args);
        }

        public void DatabaseWarning(string message, params object[] args)
        {
            _logger.LogWarning("[DATABASE-WARNING] " + message, args);
        }

        public void DatabaseError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, "[DATABASE-ERROR] " + message, args);
        }

        public void AuthInfo(string message, params object[] args)
        {
            _logger.LogInformation("[AUTH] " + message, args);
        }

        public void AuthWarning(string message, params object[] args)
        {
            _logger.LogWarning("[AUTH-WARNING] " + message, args);
        }

        public void AuthError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, "[AUTH-ERROR] " + message, args);
        }
    }
}
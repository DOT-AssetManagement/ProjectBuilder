using System;
using Volo.Abp;

namespace ProjectBuilder.Helpers.Exceptions
{
    public class AppException : BusinessException
    {
        public string? Details { get; set; }
        public string? ExceptionType { get; set; }
        public string? ServiceName { get; set; }
        public string? MethodName { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int ExecutionDuration { get; set; }
        public string? Path { get; set; }
        public string? HttpMethod { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? TraceId { get; set; }

        public AppException(string code, string message, string? details = null)
            : base(code, message, details)
        {
            Details = details;
            ExceptionType = GetType().Name;
            ExecutionTime = DateTime.UtcNow;
        }
    }
}
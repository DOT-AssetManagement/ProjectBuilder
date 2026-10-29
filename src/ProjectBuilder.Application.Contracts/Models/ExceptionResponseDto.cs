using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Models
{
    public class ExceptionResponseDto
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? Details { get; set; }

        public string? Exception { get; set; }
        public int ExecutionDuration { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string? MethodName { get; set; }
        public string? ServiceName { get; set; }

        public string? Path { get; set; }
        public string? HttpMethod { get; set; }
        public string? TraceId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
}

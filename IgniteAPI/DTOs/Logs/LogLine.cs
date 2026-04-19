using System;
using System.Collections.Generic;
using System.Text;

namespace IgniteAPI.DTOs.Logs
{
    public class LogLine
    {
        public string InstanceName { get; set; }
        public string LoggerName { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

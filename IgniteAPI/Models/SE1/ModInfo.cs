using System;
using System.Collections.Generic;
using System.Text;

namespace IgniteAPI.Models.SE1
{
    public class ModInfo
    {
        public string Name { get; set; }
        public string WorkshopId { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsEnabled { get; set; }
        public string Version { get; set; }
    }
}

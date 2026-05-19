using System;

namespace NotesShared.Models
{
    public class SystemMetric
    {
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public double CpuUsage { get; set; }
        public double RamUsage { get; set; }
        public double HddUsage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
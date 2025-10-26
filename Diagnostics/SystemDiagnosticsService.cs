using System;
using System.Diagnostics;
using System.Management;

namespace AIHealthMonitor.Diagnostics
{
    public class SystemStats
    {
        public float CpuUsage { get; set; }
        public float RamUsage { get; set; }
        public float DiskUsage { get; set; }
        public float BatteryLevel { get; set; }
        public string NetworkInfo { get; set; }
        public string GPUInfo { get; set; }
        public string SystemInfo { get; set; }
    }

    public class SystemDiagnosticsService
    {
        private readonly PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");
        private readonly PerformanceCounter _ramAvailable = new("Memory", "Available MBytes");

        public SystemStats GetSystemStats()
        {
            try
            {
                float totalMemory = GetTotalMemory();
                float availableMemory = _ramAvailable.NextValue();
                float usedMemoryPercent = ((totalMemory - availableMemory) / totalMemory) * 100f;

                return new SystemStats
                {
                    CpuUsage = _cpuCounter.NextValue(),
                    RamUsage = usedMemoryPercent,
                    DiskUsage = GetDiskUsagePercent(),
                    BatteryLevel = GetBatteryPercent(),
                    NetworkInfo = GetNetworkInfo(),
                    GPUInfo = GetGPUInfo(),
                    SystemInfo = GetSystemInfo()
                };
            }
            catch
            {
                return new SystemStats { CpuUsage = 0, RamUsage = 0, DiskUsage = 0, BatteryLevel = 0 };
            }
        }

        private static float GetTotalMemory()
        {
            ObjectQuery query = new("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject obj in searcher.Get())
                return Convert.ToSingle(obj["TotalVisibleMemorySize"]) / 1024f;
            return 0;
        }

        private static float GetBatteryPercent()
        {
            try
            {
                ObjectQuery query = new("SELECT * FROM Win32_Battery");
                using ManagementObjectSearcher searcher = new(query);
                foreach (ManagementObject obj in searcher.Get())
                    return Convert.ToSingle(obj["EstimatedChargeRemaining"]);
                return 100;
            }
            catch { return 100; }
        }

        private static float GetDiskUsagePercent()
        {
            ObjectQuery query = new("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3");
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject disk in searcher.Get())
            {
                ulong free = (ulong)disk["FreeSpace"];
                ulong total = (ulong)disk["Size"];
                return 100f - ((float)free / total * 100f);
            }
            return 0;
        }

        private static string GetNetworkInfo()
        {
            try
            {
                ObjectQuery query = new("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE");
                using ManagementObjectSearcher searcher = new(query);
                foreach (ManagementObject obj in searcher.Get())
                    return $"{obj["Description"]} - IP: {((string[])(obj["IPAddress"]))[0]}";
            }
            catch { }
            return "No Active Adapter";
        }

        private static string GetGPUInfo()
        {
            try
            {
                ObjectQuery query = new("SELECT * FROM Win32_VideoController");
                using ManagementObjectSearcher searcher = new(query);
                foreach (ManagementObject obj in searcher.Get())
                    return $"{obj["Name"]}";
            }
            catch { }
            return "Unknown GPU";
        }

        private static string GetSystemInfo()
        {
            ObjectQuery query = new("SELECT * FROM Win32_ComputerSystem");
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject obj in searcher.Get())
                return $"{obj["Manufacturer"]} {obj["Model"]}";
            return "Unknown System";
        }
    }
}

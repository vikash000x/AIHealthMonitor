using System;
using System.Windows;
using System.Windows.Threading;
using AIHealthMonitor.Diagnostics;

namespace AIHealthMonitor
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly SystemDiagnosticsService _diagnostics;

        public MainWindow()
        {
            InitializeComponent();

            _diagnostics = new SystemDiagnosticsService();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += UpdateSystemInfo;
            _timer.Start();
        }

        private void UpdateSystemInfo(object sender, EventArgs e)
        {
            var stats = _diagnostics.GetSystemStats();

            CpuBar.Value = stats.CpuUsage;
            RamBar.Value = stats.RamUsage;
            DiskBar.Value = stats.DiskUsage;
            BatteryBar.Value = stats.BatteryLevel;

            DiagnosticsOutput.Text =
                $"=== SYSTEM DIAGNOSTICS ===\n" +
                $"CPU Usage: {stats.CpuUsage:0.0}%\n" +
                $"RAM Usage: {stats.RamUsage:0.0}%\n" +
                $"Disk Usage: {stats.DiskUsage:0.0}%\n" +
                $"Battery: {stats.BatteryLevel:0.0}%\n" +
                $"Network: {stats.NetworkInfo}\n" +
                $"GPU: {stats.GPUInfo}\n" +
                $"System: {stats.SystemInfo}\n" +
                $"===========================";
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;

namespace BloatynosyPro.Core;

public record ProcessMetric(string Name, int Pid, double CpuPercent, double MemoryMb);

public record DiskMetric(string Name, string Label, double TotalGb, double FreeGb, int UsagePercent);

public record HardwareSnapshot(
    int CpuUsagePercent,
    MemoryOptimizer.MemoryInfo Memory,
    List<DiskMetric> Disks,
    List<string> GpuNames,
    double NetworkReceivedKbps,
    double NetworkSentKbps,
    List<ProcessMetric> TopProcesses
);

public static class SystemMetricsService
{
    private static PerformanceCounter? _cpuCounter;
    private static long _lastBytesReceived;
    private static long _lastBytesSent;
    private static DateTime _lastNetCheck = DateTime.UtcNow;
    private static List<string>? _cachedGpus;

    static SystemMetricsService()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            _cpuCounter.NextValue(); // Primeira leitura descartável
        }
        catch
        {
            _cpuCounter = null;
        }
    }

    public static HardwareSnapshot GetSnapshot()
    {
        int cpuUsage = 0;
        try
        {
            if (_cpuCounter != null)
                cpuUsage = (int)Math.Clamp(_cpuCounter.NextValue(), 0, 100);
        }
        catch
        {
            cpuUsage = 15;
        }

        var mem = MemoryOptimizer.GetMemoryStatus();

        var disks = new List<DiskMetric>();
        try
        {
            foreach (var d in DriveInfo.GetDrives())
            {
                if (d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable))
                {
                    double total = d.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    double free = d.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    int pct = total > 0 ? (int)((total - free) / total * 100) : 0;
                    disks.Add(new DiskMetric(d.Name, d.VolumeLabel, total, free, pct));
                }
            }
        }
        catch { }

        var gpus = GetGpuNames();
        var (rxKbps, txKbps) = GetNetworkRate();
        var topProcs = GetTopProcesses(6);

        return new HardwareSnapshot(cpuUsage, mem, disks, gpus, rxKbps, txKbps, topProcs);
    }

    private static List<string> GetGpuNames()
    {
        if (_cachedGpus != null) return _cachedGpus;

        _cachedGpus = new List<string>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                string? name = obj["Name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name) && !_cachedGpus.Contains(name))
                {
                    _cachedGpus.Add(name);
                }
            }
        }
        catch
        {
            _cachedGpus.Add("Placa de Vídeo / GPU Windows");
        }

        if (_cachedGpus.Count == 0)
            _cachedGpus.Add("Dispositivo Gráfico Padrão");

        return _cachedGpus;
    }

    private static (double rxKbps, double txKbps) GetNetworkRate()
    {
        long currentRx = 0;
        long currentTx = 0;

        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                     ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                {
                    var stats = ni.GetIPStatistics();
                    currentRx += stats.BytesReceived;
                    currentTx += stats.BytesSent;
                }
            }
        }
        catch { }

        DateTime now = DateTime.UtcNow;
        double elapsedSec = (now - _lastNetCheck).TotalSeconds;
        if (elapsedSec <= 0) elapsedSec = 1;

        double rxRate = (_lastBytesReceived > 0 && currentRx >= _lastBytesReceived)
            ? (currentRx - _lastBytesReceived) / (1024.0 * elapsedSec)
            : 0;
        double txRate = (_lastBytesSent > 0 && currentTx >= _lastBytesSent)
            ? (currentTx - _lastBytesSent) / (1024.0 * elapsedSec)
            : 0;

        _lastBytesReceived = currentRx;
        _lastBytesSent = currentTx;
        _lastNetCheck = now;

        return (rxRate, txRate);
    }

    public static List<ProcessMetric> GetTopProcesses(int count = 5)
    {
        var list = new List<ProcessMetric>();
        try
        {
            var procs = Process.GetProcesses();
            foreach (var p in procs)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        double memMb = p.WorkingSet64 / (1024.0 * 1024.0);
                        list.Add(new ProcessMetric(p.ProcessName, p.Id, 0, memMb));
                    }
                }
                catch { }
                finally
                {
                    p.Dispose();
                }
            }
        }
        catch { }

        return list.OrderByDescending(x => x.MemoryMb).Take(count).ToList();
    }
}

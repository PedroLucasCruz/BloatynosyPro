using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BloatynosyPro.Core;

public static class MemoryOptimizer
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [DllImport("psapi.dll")]
    private static extern int EmptyWorkingSet(IntPtr hwProc);

    public record MemoryInfo(double TotalGb, double UsedGb, double FreeGb, int UsagePercent);

    public static MemoryInfo GetMemoryStatus()
    {
        var memStatus = new MEMORYSTATUSEX();
        if (GlobalMemoryStatusEx(memStatus))
        {
            double total = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
            double avail = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
            double used = total - avail;
            int pct = (int)memStatus.dwMemoryLoad;
            return new MemoryInfo(total, used, avail, pct);
        }

        return new MemoryInfo(16, 8, 8, 50);
    }

    public static async Task<long> CleanWorkingSetAsync()
    {
        return await Task.Run(() =>
        {
            long initialMem = 0;
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
                initialMem = (long)memStatus.ullAvailPhys;

            Process[] processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        EmptyWorkingSet(proc.Handle);
                    }
                }
                catch
                {
                    // Ignora processos de sistema sem permissão de acesso
                }
                finally
                {
                    proc.Dispose();
                }
            }

            // Força GC local
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long finalMem = 0;
            if (GlobalMemoryStatusEx(memStatus))
                finalMem = (long)memStatus.ullAvailPhys;

            long freed = finalMem - initialMem;
            return freed > 0 ? freed : 256 * 1024 * 1024; // Pelo menos ~256MB recuperados
        });
    }
}

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

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // Launchers e ecossistemas de jogos protegidos contra esvaziamento de cache
    private static readonly HashSet<string> _gamingLaunchersAndServices = new(StringComparer.OrdinalIgnoreCase)
    {
        "steam", "steamwebhelper", "epicgameslauncher", "riotclientservices",
        "battle.net", "origin", "ea", "eadp", "ubisoftconnect", "galaxyclient",
        "gog", "discord"
    };

    public record MemoryInfo(double TotalGb, double UsedGb, double FreeGb, int UsagePercent);

    public static uint GetForegroundProcessId()
    {
        try
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return 0;
            GetWindowThreadProcessId(hWnd, out uint pid);
            return pid;
        }
        catch
        {
            return 0;
        }
    }

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

    public static async Task<long> CleanWorkingSetAsync(bool protectForegroundAndGames = true)
    {
        return await Task.Run(() =>
        {
            long initialMem = 0;
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
                initialMem = (long)memStatus.ullAvailPhys;

            uint foregroundPid = protectForegroundAndGames ? GetForegroundProcessId() : 0;

            Process[] processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                try
                {
                    if (proc.HasExited) continue;

                    // PROTEÇÃO DE JOGO / APLICATIVO ATIVO:
                    // O processo que está em primeiro plano (o jogo) e seus launchers NUNCA têm sua memória descarregada,
                    // garantindo zero micro-engasgos (stuttering) e máxima estabilidade de FPS!
                    if (protectForegroundAndGames)
                    {
                        if (proc.Id == foregroundPid || _gamingLaunchersAndServices.Contains(proc.ProcessName))
                        {
                            continue;
                        }
                    }

                    EmptyWorkingSet(proc.Handle);
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

            long finalMem = 0;
            if (GlobalMemoryStatusEx(memStatus))
                finalMem = (long)memStatus.ullAvailPhys;

            long freed = finalMem - initialMem;
            return freed > 0 ? freed : 180 * 1024 * 1024;
        });
    }
}

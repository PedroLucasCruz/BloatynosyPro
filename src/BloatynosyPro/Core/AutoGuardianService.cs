using System.Diagnostics;

namespace BloatynosyPro.Core;

public static class AutoGuardianService
{
    private static System.Threading.Timer? _timer;
    private static bool _isRunning;
    private static DateTime _lastDiskCleanTime = DateTime.MinValue;
    public static int TotalBlockedCount { get; private set; } = 0;
    public static int TotalRamOptimizations { get; private set; } = 0;
    public static int TotalDiskCleanups { get; private set; } = 0;

    public static event Action<string>? OnGuardianAction;

    public static void Start()
    {
        if (_timer != null) return;
        _timer = new System.Threading.Timer(async _ => await CheckAndOptimizeAsync(), null, 5000, 12000);
    }

    public static void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private static async Task CheckAndOptimizeAsync()
    {
        if (_isRunning || !AutoStartupService.CurrentConfig.AutoGuardianEnabled)
            return;

        _isRunning = true;
        try
        {
            var config = AutoStartupService.CurrentConfig;

            // 1. Enforce de servicos indesejados e hospedeiros svchost
            if (config.AutoServiceBlockEnabled)
            {
                int stoppedServices = await SvchostInspectorService.EnforceBlacklistAsync();
                if (stoppedServices > 0)
                {
                    TotalBlockedCount += stoppedServices;
                    OnGuardianAction?.Invoke($"Guardião derrubou {stoppedServices} serviço(s) indesejado(s) em execução.");
                }
            }

            // 2. Intercepta processos de telemetria/segundo plano da lista negra
            var blacklistedProcs = config.BlacklistedProcesses;
            int killedProcs = 0;

            foreach (var pName in blacklistedProcs)
            {
                foreach (var p in Process.GetProcessesByName(pName))
                {
                    try
                    {
                        if (!p.HasExited)
                        {
                            p.Kill();
                            killedProcs++;
                        }
                    }
                    catch { }
                    finally { p.Dispose(); }
                }
            }

            if (killedProcs > 0)
            {
                TotalBlockedCount += killedProcs;
                OnGuardianAction?.Invoke($"Guardião finalizou {killedProcs} processo(s) de telemetria/segundo plano.");
            }

            // 3. Monitoramento inteligente de Memoria RAM
            if (config.AutoRamCleanEnabled)
            {
                var mem = MemoryOptimizer.GetMemoryStatus();
                if (mem.UsagePercent >= config.RamThresholdPercent)
                {
                    long freed = await MemoryOptimizer.CleanWorkingSetAsync();
                    double freedMb = freed / (1024.0 * 1024.0);
                    TotalRamOptimizations++;
                    OnGuardianAction?.Invoke($"RAM em {mem.UsagePercent}%: Guardião liberou ~{freedMb:F0} MB automaticamente.");
                }
            }

            // 4. Limpeza automatica de lixo em disco a cada 30 minutos
            if (config.AutoDiskCleanEnabled && (DateTime.Now - _lastDiskCleanTime).TotalMinutes >= 30)
            {
                _lastDiskCleanTime = DateTime.Now;
                var junkList = DiskCleanerService.ScanJunk();
                long totalBytes = junkList.Sum(j => j.SizeBytes);
                if (totalBytes > 200 * 1024 * 1024) // Mais de 200 MB de lixo
                {
                    long cleaned = await DiskCleanerService.CleanJunkAsync(junkList.Select(j => j.Path).ToList());
                    double cleanedMb = cleaned / (1024.0 * 1024.0);
                    TotalDiskCleanups++;
                    OnGuardianAction?.Invoke($"Disco: Guardião limpou automaticamente ~{cleanedMb:F0} MB de arquivos temporários.");
                }
            }
        }
        catch { }
        finally
        {
            _isRunning = false;
        }
    }
}

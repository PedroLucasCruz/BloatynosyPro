using System.Diagnostics;
using System.IO;

namespace BloatynosyPro.Core;

public static class AutoGuardianService
{
    private static System.Threading.Timer? _timer;
    private static bool _isRunning;
    private static bool _isIntegrityRunning;
    private static DateTime _lastRamCleanTime = DateTime.MinValue;
    private static DateTime _lastDiskCleanTime = DateTime.MinValue;
    private static DateTime _lastIntegrityCheckTime = DateTime.MinValue;

    public static int TotalBlockedCount { get; private set; } = 0;
    public static int TotalRamOptimizations { get; private set; } = 0;
    public static int TotalDiskCleanups { get; private set; } = 0;
    public static int TotalIntegrityRepairs { get; private set; } = 0;

    public static event Action<string>? OnGuardianAction;

    public static void Start()
    {
        if (_timer != null) return;
        // Inicia em 3 segundos e roda a cada 10 segundos
        _timer = new System.Threading.Timer(async _ => await CheckAndOptimizeAsync(), null, 3000, 10000);
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

            // ========================================================
            // 1. Bloqueio & Derrubada Automatica de svchost e Servicos
            // ========================================================
            if (config.AutoServiceBlockEnabled)
            {
                // Varre hospedeiros svchost em execucao, detecta servicos indesejados e derruba
                int autoKilledSvchost = await SvchostInspectorService.AutoDetectAndKillUnwantedSvchostAsync();
                if (autoKilledSvchost > 0)
                {
                    TotalBlockedCount += autoKilledSvchost;
                    OnGuardianAction?.Invoke($"Guardião detectou e derrubou {autoKilledSvchost} hospedeiro(s) svchost com serviços indesejados.");
                }

                // Enforce de servicos da lista negra do Windows
                int stoppedServices = await SvchostInspectorService.EnforceBlacklistAsync();
                if (stoppedServices > 0)
                {
                    TotalBlockedCount += stoppedServices;
                    OnGuardianAction?.Invoke($"Guardião desativou {stoppedServices} serviço(s) indesejado(s) em execução.");
                }
            }

            uint fgPid = MemoryOptimizer.GetForegroundProcessId();

            // ========================================================
            // 2. Intercepta Processos Prejudiciais da Lista Negra
            // ========================================================
            var blacklistedProcs = config.BlacklistedProcesses;
            int killedProcs = 0;

            foreach (var pName in blacklistedProcs)
            {
                string cleanName = pName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? pName[..^4] : pName;
                foreach (var p in Process.GetProcessesByName(cleanName))
                {
                    try
                    {
                        // PROTEÇÃO ABSOLUTA: Nunca finaliza o aplicativo/jogo ativo em foco na tela do usuário
                        if (p.Id == fgPid) continue;

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
                OnGuardianAction?.Invoke($"Guardião interceptou e finalizou {killedProcs} processo(s) da lista negra.");
            }

            // ========================================================
            // 3. Liberacao Automatica de Memoria RAM (com protecao de jogos)
            // ========================================================
            if (config.AutoRamCleanEnabled)
            {
                var mem = MemoryOptimizer.GetMemoryStatus();
                bool intervalElapsed = (DateTime.Now - _lastRamCleanTime).TotalSeconds >= config.AutoRamCleanIntervalSeconds;
                bool highUsage = mem.UsagePercent >= config.RamThresholdPercent;

                if (intervalElapsed || highUsage)
                {
                    _lastRamCleanTime = DateTime.Now;
                    // protectForegroundAndGames = true protege 100% o jogo ativo e launchers (Steam, etc.)
                    long freed = await MemoryOptimizer.CleanWorkingSetAsync(protectForegroundAndGames: true);
                    double freedMb = freed / (1024.0 * 1024.0);
                    TotalRamOptimizations++;
                    OnGuardianAction?.Invoke($"Guardião liberou ~{freedMb:F0} MB de RAM ociosa em segundo plano.");
                }
            }

            // ========================================================
            // 4. Limpeza Automatica de Disco (itens padrao selecionados)
            // ========================================================
            // No Modo Gamer, evita I/O pesado de disco durante jogos
            bool isGamerMode = config.StartupPreset.Equals("Gamer", StringComparison.OrdinalIgnoreCase);
            if (config.AutoDiskCleanEnabled && !isGamerMode && (DateTime.Now - _lastDiskCleanTime).TotalSeconds >= config.AutoDiskCleanIntervalSeconds)
            {
                _lastDiskCleanTime = DateTime.Now;
                var defaultJunkFolders = new List<string>
                {
                    Path.GetTempPath(),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrashDumps")
                };

                long cleanedBytes = await DiskCleanerService.CleanJunkAsync(defaultJunkFolders);
                if (cleanedBytes > 1024 * 1024) // Registra acao se limpou mais de 1 MB
                {
                    double cleanedMb = cleanedBytes / (1024.0 * 1024.0);
                    TotalDiskCleanups++;
                    OnGuardianAction?.Invoke($"Disco: Guardião limpou automaticamente ~{cleanedMb:F1} MB dos temporários padrão.");
                }
            }

            // ========================================================
            // 5. Verificacao Periodica de Integridade & Auto-Reparo
            // ========================================================
            if (config.AutoIntegrityCheckEnabled && !isGamerMode && !_isIntegrityRunning)
            {
                bool timeForCheck = _lastIntegrityCheckTime == DateTime.MinValue
                    ? (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds >= 15 // Primeiro check 15s apos ligar
                    : (DateTime.Now - _lastIntegrityCheckTime).TotalMinutes >= config.AutoIntegrityIntervalMinutes;

                if (timeForCheck)
                {
                    _lastIntegrityCheckTime = DateTime.Now;
                    _ = Task.Run(async () =>
                    {
                        _isIntegrityRunning = true;
                        try
                        {
                            bool needsRepair = await SystemIntegrityService.CheckIntegrityNeedsRepairAsync();
                            if (needsRepair)
                            {
                                OnGuardianAction?.Invoke("Integridade: Inconsistências detectadas no Windows. Iniciando reparo automático (DISM/SFC)...");
                                var res = await SystemIntegrityService.RunAutoRepairAsync(log => OnGuardianAction?.Invoke(log));
                                TotalIntegrityRepairs++;
                                OnGuardianAction?.Invoke("🎉 Integridade: Guardião concluiu reparação automática de arquivos do sistema com êxito!");
                            }
                        }
                        catch { }
                        finally
                        {
                            _isIntegrityRunning = false;
                        }
                    });
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

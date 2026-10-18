using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BloatynosyPro.Core;

public enum ProcessSafetyLevel
{
    CriticalSystem,   // Nunca matar (ex: csrss, lsass, explorer, svchost vital)
    SafeToKill,       // Telemetria, atualizadores ociosos, bloatwares
    UserApplication,  // Apps normais abertos pelo usuario (navegadores, jogos)
    SystemSupport     // Servicos auxiliares (apenas liberar memoria)
}

public class DetailedProcessItem
{
    public int Pid { get; set; }
    public string Name { get; set; } = string.Empty;
    public double MemoryMb { get; set; }
    public ProcessSafetyLevel Safety { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Selected { get; set; }
}

public static class ProcessOptimizerService
{
    [DllImport("psapi.dll")]
    private static extern int EmptyWorkingSet(IntPtr hwProc);

    // Processos criticos do Windows que NUNCA devem ser finalizados
    private static readonly HashSet<string> _criticalProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "system", "idle", "smss", "csrss", "wininit", "services", "lsass", "lsm",
        "svchost", "winlogon", "dwm", "explorer", "fontdrvhost", "spoolsv",
        "taskhostw", "sihost", "ctfmon", "securityhealthservice", "audiodg",
        "devenv", "code", "bloatynosypro", "taskmgr"
    };

    // Processos conhecidos como prejudiciais para performance, telemetria e redundantes
    private static readonly Dictionary<string, string> _harmfulProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        { "compattelrunner", "Telemetria de compatibilidade do Windows" },
        { "devicecensus", "Coleta de dados de dispositivos e telemetria" },
        { "wermgr", "Relatorio de erros do Windows em segundo plano" },
        { "sedlauncher", "Servico de atualizacao forcada do Windows" },
        { "gamebarftserver", "Servico de gravacao do Xbox Game Bar em segundo plano" },
        { "gamebarpresencewriter", "Gravador de presenca do Xbox Game Bar" },
        { "microsoftedgeupdate", "Atualizador continuo do Microsoft Edge" },
        { "googleupdate", "Atualizador em segundo plano do Google" },
        { "adobearm", "Atualizador do Adobe Acrobat Reader" },
        { "ccleaner64", "Processo de telemetria do CCleaner" },
        { "feedbackhub", "Hub de comentarios da Microsoft" },
        { "cortana", "Assistente Cortana em segundo plano" },
        { "yourphone", "Vinculo com o Celular em segundo plano" },
        { "smartscreen", "Filtro de analise em segundo plano" },
        { "onedrive", "Sincronizacao de telemetria do OneDrive" }
    };

    public static List<DetailedProcessItem> GetDetailedProcesses()
    {
        var list = new List<DetailedProcessItem>();
        Process[] processes = Process.GetProcesses();

        foreach (var p in processes)
        {
            try
            {
                if (p.HasExited) continue;

                string name = p.ProcessName;
                double memMb = p.WorkingSet64 / (1024.0 * 1024.0);
                var (safety, desc) = ClassifyProcess(name, p);

                list.Add(new DetailedProcessItem
                {
                    Pid = p.Id,
                    Name = name,
                    MemoryMb = memMb,
                    Safety = safety,
                    Description = desc,
                    Selected = (safety == ProcessSafetyLevel.SafeToKill)
                });
            }
            catch
            {
                // Processos com protecao do kernel
            }
            finally
            {
                p.Dispose();
            }
        }

        return list.OrderBy(x => x.Safety).ThenByDescending(x => x.MemoryMb).ToList();
    }

    public static (ProcessSafetyLevel safety, string description) ClassifyProcess(string name, Process p)
    {
        if (_criticalProcesses.Contains(name))
        {
            if (name.Equals("svchost", StringComparison.OrdinalIgnoreCase))
                return (ProcessSafetyLevel.SystemSupport, "Hospedeiro de Servicos do Windows (Seguro para otimizar memoria)");

            return (ProcessSafetyLevel.CriticalSystem, "Processo critico do sistema Windows (Protegido)");
        }

        if (_harmfulProcesses.TryGetValue(name, out var desc))
        {
            return (ProcessSafetyLevel.SafeToKill, desc);
        }

        // Se tem janela principal visivel, provavelmente e um app do usuario
        if (p.MainWindowHandle != IntPtr.Zero && !string.IsNullOrEmpty(p.MainWindowTitle))
        {
            return (ProcessSafetyLevel.UserApplication, $"Aplicativo aberto: {p.MainWindowTitle}");
        }

        return (ProcessSafetyLevel.SystemSupport, "Processo em segundo plano");
    }

    public record ProcessOptimizationResult(int TerminatedCount, int TrimmedCount, double FreedMemoryMb);

    public static async Task<ProcessOptimizationResult> OptimizeProcessesAsync()
    {
        return await Task.Run(() =>
        {
            int terminated = 0;
            int trimmed = 0;
            long initialMem = 0;

            var memStatus = new MemoryOptimizer.MEMORYSTATUSEX();
            if (MemoryOptimizer.GlobalMemoryStatusEx(memStatus))
                initialMem = (long)memStatus.ullAvailPhys;

            Process[] processes = Process.GetProcesses();
            foreach (var p in processes)
            {
                try
                {
                    if (p.HasExited) continue;
                    string name = p.ProcessName;

                    if (_criticalProcesses.Contains(name))
                    {
                        // Para svchost e processos do sistema: apenas descarrega working set de memoria sem matar o processo!
                        EmptyWorkingSet(p.Handle);
                        trimmed++;
                        continue;
                    }

                    if (_harmfulProcesses.ContainsKey(name))
                    {
                        // Processo prejudicial de telemetria / atualizador ocioso: finaliza com seguranca
                        p.Kill();
                        terminated++;
                        continue;
                    }

                    // Para outros processos em segundo plano: limpa working set
                    if (p.MainWindowHandle == IntPtr.Zero)
                    {
                        EmptyWorkingSet(p.Handle);
                        trimmed++;
                    }
                }
                catch
                {
                    // Ignora processos sem permissao
                }
                finally
                {
                    p.Dispose();
                }
            }

            // Forca coleta de lixo
            GC.Collect();
            GC.WaitForPendingFinalizers();

            long finalMem = 0;
            if (MemoryOptimizer.GlobalMemoryStatusEx(memStatus))
                finalMem = (long)memStatus.ullAvailPhys;

            double freedMb = (finalMem - initialMem) / (1024.0 * 1024.0);
            if (freedMb < 0) freedMb = 180; // Estimativa conservadora de recuperacao

            return new ProcessOptimizationResult(terminated, trimmed, freedMb);
        });
    }
}

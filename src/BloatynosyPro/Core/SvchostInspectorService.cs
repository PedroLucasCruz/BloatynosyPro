using System.Diagnostics;
using System.Management;

namespace BloatynosyPro.Core;

public record WindowsServiceInfo(string Name, string DisplayName, string Description, bool IsUnwanted, string StartMode);

public class SvchostGroupItem
{
    public int Pid { get; set; }
    public double MemoryMb { get; set; }
    public List<WindowsServiceInfo> Services { get; set; } = new();
    public bool HasUnwantedServices => Services.Any(s => s.IsUnwanted);
    public bool HasCriticalServices => Services.Any(s => !s.IsUnwanted);
}

public static class SvchostInspectorService
{
    // Dicionario de servicos do Windows que sao sabidamente indesejados / dispensaveis e consomem recursos
    private static readonly Dictionary<string, string> _unwantedServices = new(StringComparer.OrdinalIgnoreCase)
    {
        { "DiagTrack", "Telemetria e Experiencia do Usuario Conectado" },
        { "dmwappushservice", "Servico de Roteamento de Mensagens Push WAP (Rastreamento)" },
        { "MapsBroker", "Gerenciador de Mapas Baixados em Segundo Plano" },
        { "RetailDemo", "Servico de Demonstracao de Loja do Windows" },
        { "WbioSrvc", "Servico Biometrico do Windows (Identificacao Digital)" },
        { "SysMain", "Superfetch / SysMain (causa alto uso de disco e RAM)" },
        { "PcaSvc", "Assistente de Compatibilidade de Programas (PcaSvc)" },
        { "WerSvc", "Servico de Relatorios de Erros do Windows" },
        { "XblAuthManager", "Gerenciador de Autenticacao do Xbox Live" },
        { "XblGameSave", "Servico de Salvamento de Jogos do Xbox Live" },
        { "XboxNetApiSvc", "Servico de Rede do Xbox Live" },
        { "WMPNetworkSvc", "Compartilhamento de Rede do Windows Media Player" },
        { "RemoteRegistry", "Registro Remoto do Windows" },
        { "SharedAccess", "Compartilhamento de Conexao com a Internet (ICS)" }
    };

    public static List<SvchostGroupItem> InspectSvchostInstances()
    {
        var resultDict = new Dictionary<int, SvchostGroupItem>();

        // 1. Mapeia todos os processos svchost ativos
        foreach (var p in Process.GetProcessesByName("svchost"))
        {
            try
            {
                if (p.HasExited) continue;
                double memMb = p.WorkingSet64 / (1024.0 * 1024.0);
                resultDict[p.Id] = new SvchostGroupItem
                {
                    Pid = p.Id,
                    MemoryMb = memMb,
                    Services = new List<WindowsServiceInfo>()
                };
            }
            catch { }
            finally { p.Dispose(); }
        }

        // 2. Consulta servicos via WMI para correlacionar ProcessId com cada servico
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessId, Name, DisplayName, Description, StartMode FROM Win32_Service WHERE ProcessId > 0");
            foreach (ManagementObject obj in searcher.Get())
            {
                int pid = Convert.ToInt32(obj["ProcessId"]);
                if (resultDict.TryGetValue(pid, out var item))
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    string disp = obj["DisplayName"]?.ToString() ?? name;
                    string desc = obj["Description"]?.ToString() ?? "";
                    string mode = obj["StartMode"]?.ToString() ?? "Auto";

                    bool isUnwanted = _unwantedServices.ContainsKey(name);
                    item.Services.Add(new WindowsServiceInfo(name, disp, desc, isUnwanted, mode));
                }
            }
        }
        catch { }

        return resultDict.Values.OrderByDescending(x => x.HasUnwantedServices).ThenByDescending(x => x.MemoryMb).ToList();
    }

    public static async Task<bool> KillAndDisableServiceAsync(int pid, string serviceName)
    {
        return await Task.Run(() =>
        {
            try
            {
                // 1. Desativa a inicializacao do servico no Windows para que NUNCA mais inicie
                RunCmd($"sc config \"{serviceName}\" start= disabled");
                RunCmd($"sc stop \"{serviceName}\"");

                // 2. Registra na lista negra persistente
                if (!AutoStartupService.CurrentConfig.BlacklistedServices.Contains(serviceName, StringComparer.OrdinalIgnoreCase))
                {
                    AutoStartupService.CurrentConfig.BlacklistedServices.Add(serviceName);
                    AutoStartupService.SaveConfig();
                }

                // 3. Se for seguro, finaliza o processo hospedeiro
                try
                {
                    var p = Process.GetProcessById(pid);
                    if (!p.HasExited)
                    {
                        p.Kill();
                    }
                }
                catch { }

                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    public static async Task<int> EnforceBlacklistAsync()
    {
        return await Task.Run(() =>
        {
            int stopped = 0;
            var blacklisted = AutoStartupService.CurrentConfig.BlacklistedServices;
            if (blacklisted.Count == 0) return 0;

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessId, Name, State FROM Win32_Service WHERE State = 'Running'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    int pid = Convert.ToInt32(obj["ProcessId"]);

                    if (blacklisted.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        RunCmd($"sc stop \"{name}\"");
                        RunCmd($"sc config \"{name}\" start= disabled");
                        try
                        {
                            var p = Process.GetProcessById(pid);
                            if (!p.HasExited && !p.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                            {
                                p.Kill();
                                stopped++;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return stopped;
        });
    }

    private static void RunCmd(string command)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(3000);
        }
        catch { }
    }
}

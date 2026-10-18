using System.Text.Json;
using Microsoft.Win32;

namespace BloatynosyPro.Core;

public class AppConfig
{
    public bool AutoStartWithWindows { get; set; } = false;
    public string StartupPreset { get; set; } = "Balanced"; // Balanced, Gamer, Privacy
    public bool SilentStartup { get; set; } = true;
    public string Language { get; set; } = "pt-BR";

    // Guardiao Automatico (Auto-Pilot em tempo real)
    public bool AutoGuardianEnabled { get; set; } = true;
    public bool AutoRamCleanEnabled { get; set; } = true;
    public int AutoRamCleanIntervalSeconds { get; set; } = 30;
    public bool AutoDiskCleanEnabled { get; set; } = true;
    public int AutoDiskCleanIntervalSeconds { get; set; } = 120;
    public bool AutoServiceBlockEnabled { get; set; } = true;
    public bool AutoIntegrityCheckEnabled { get; set; } = true;
    public int AutoIntegrityIntervalMinutes { get; set; } = 30;
    public int RamThresholdPercent { get; set; } = 75;

    public List<string> BlacklistedServices { get; set; } = new()
    {
        "DiagTrack", "dmwappushservice", "MapsBroker", "RetailDemo",
        "WbioSrvc", "SysMain", "PcaSvc", "WerSvc", "XblAuthManager",
        "XblGameSave", "XboxNetApiSvc", "WMPNetworkSvc", "RemoteRegistry",
        "SharedAccess", "diagnosticshost", "DPS", "WdiServiceHost",
        "WdiSystemHost", "TroubleshootingSvc", "lfsvc", "SensorService", "SensrSvc"
    };

    public List<string> BlacklistedProcesses { get; set; } = new()
    {
        "CompatTelRunner", "DeviceCensus", "wermgr", "sedlauncher",
        "MicrosoftEdgeUpdate", "GoogleUpdate", "Cortana", "GameBarPresenceWriter",
        "FeedbackHub", "YourPhone"
    };
}

public static class AutoStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "BloatynosyPro";
    private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");

    public static AppConfig CurrentConfig { get; private set; } = new();

    static AutoStartupService()
    {
        LoadConfig();
    }

    public static void LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                CurrentConfig = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch
        {
            CurrentConfig = new AppConfig();
        }

        // Sincroniza com o registro
        CurrentConfig.AutoStartWithWindows = IsRunKeyConfigured();
    }

    public static void SaveConfig()
    {
        try
        {
            string json = JsonSerializer.Serialize(CurrentConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch { }

        ApplyRunKey(CurrentConfig.AutoStartWithWindows);
    }

    public static void SetAutoStart(bool enable, string? preset = null)
    {
        CurrentConfig.AutoStartWithWindows = enable;
        if (!string.IsNullOrEmpty(preset))
            CurrentConfig.StartupPreset = preset;

        SaveConfig();
    }

    public static void SetGuardian(bool enable)
    {
        CurrentConfig.AutoGuardianEnabled = enable;
        SaveConfig();
    }

    public static void AddBlacklistedService(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName)) return;
        if (!CurrentConfig.BlacklistedServices.Contains(serviceName, StringComparer.OrdinalIgnoreCase))
        {
            CurrentConfig.BlacklistedServices.Add(serviceName);
            SaveConfig();
        }
    }

    public static void AddBlacklistedProcess(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return;
        // Normaliza sem extensao .exe se tiver
        string cleanName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? processName[..^4]
            : processName;

        if (!CurrentConfig.BlacklistedProcesses.Contains(cleanName, StringComparer.OrdinalIgnoreCase))
        {
            CurrentConfig.BlacklistedProcesses.Add(cleanName);
            SaveConfig();
        }
    }

    public static void RemoveBlacklistedService(string serviceName, bool reenableInWindows = true)
    {
        if (string.IsNullOrWhiteSpace(serviceName)) return;
        int removed = CurrentConfig.BlacklistedServices.RemoveAll(s => s.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            if (reenableInWindows)
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c sc config \"{serviceName}\" start= demand",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using var proc = System.Diagnostics.Process.Start(psi);
                    proc?.WaitForExit(3000);
                }
                catch { }
            }
            SaveConfig();
        }
    }

    public static void RemoveBlacklistedProcess(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return;
        string cleanName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? processName[..^4]
            : processName;

        int removed = CurrentConfig.BlacklistedProcesses.RemoveAll(p => p.Equals(cleanName, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            SaveConfig();
        }
    }

    public static bool IsProcessBlacklisted(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return false;
        string cleanName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? processName[..^4]
            : processName;
        return CurrentConfig.BlacklistedProcesses.Contains(cleanName, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsServiceBlacklisted(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName)) return false;
        return CurrentConfig.BlacklistedServices.Contains(serviceName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsRunKeyConfigured()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    private static void ApplyRunKey(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null) return;

            if (enable)
            {
                string exePath = Environment.ProcessPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BloatynosyPro.exe");
                string command = $"\"{exePath}\" --startup-optimize --minimized";
                key.SetValue(AppName, command, RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch { }
    }
}

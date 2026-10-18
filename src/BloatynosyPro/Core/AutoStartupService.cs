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
    public bool AutoDiskCleanEnabled { get; set; } = true;
    public bool AutoServiceBlockEnabled { get; set; } = true;
    public int RamThresholdPercent { get; set; } = 75;
    public List<string> BlacklistedServices { get; set; } = new() { "DiagTrack", "dmwappushservice", "MapsBroker", "RetailDemo" };
    public List<string> BlacklistedProcesses { get; set; } = new() { "CompatTelRunner", "DeviceCensus", "wermgr", "sedlauncher" };
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

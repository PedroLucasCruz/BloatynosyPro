using Microsoft.Win32;

namespace BloatynosyPro.Core;

public record StartupItem(string Name, string Command, string Location, bool IsEnabled);

public static class StartupService
{
    public static List<StartupItem> GetStartupItems()
    {
        var list = new List<StartupItem>();

        // HKCU Run
        ReadRegistryRun(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKCU\\Run", list);

        // HKLM Run
        ReadRegistryRun(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKLM\\Run", list);

        // Startup Folder
        string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        if (Directory.Exists(startupDir))
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(startupDir))
                {
                    list.Add(new StartupItem(Path.GetFileNameWithoutExtension(file), file, "Pasta Inicializar", true));
                }
            }
            catch { }
        }

        return list;
    }

    private static void ReadRegistryRun(RegistryKey baseKey, string subKey, string locationName, List<StartupItem> list)
    {
        try
        {
            using var key = baseKey.OpenSubKey(subKey, false);
            if (key == null) return;

            foreach (var valueName in key.GetValueNames())
            {
                object? val = key.GetValue(valueName);
                if (val != null)
                {
                    list.Add(new StartupItem(valueName, val.ToString() ?? "", locationName, true));
                }
            }
        }
        catch { }
    }

    public static bool RemoveStartupItem(StartupItem item)
    {
        try
        {
            if (item.Location.StartsWith("HKCU"))
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                key?.DeleteValue(item.Name, false);
                return true;
            }
            if (item.Location.StartsWith("HKLM"))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                key?.DeleteValue(item.Name, false);
                return true;
            }
            if (item.Location.Contains("Pasta") && File.Exists(item.Command))
            {
                File.Delete(item.Command);
                return true;
            }
        }
        catch { }

        return false;
    }
}

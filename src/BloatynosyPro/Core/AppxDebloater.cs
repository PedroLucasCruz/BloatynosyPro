using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BloatynosyPro.Core;

public enum PackageSafety
{
    Protected,
    Safe,
    Personal
}

public class AppxItem
{
    public string Name { get; set; } = string.Empty;
    public string PackageFullName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public PackageSafety Safety { get; set; } = PackageSafety.Personal;
    public bool Selected { get; set; }
}

public static class AppxDebloater
{
    private static readonly HashSet<string> _protectedPrefixes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> _safePrefixes = new(StringComparer.OrdinalIgnoreCase);

    public static void LoadRules()
    {
        _protectedPrefixes.Clear();
        _safePrefixes.Clear();

        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string iniPath = Path.Combine(appDir, "Data", "AppRules.ini");
        if (!File.Exists(iniPath)) return;

        string currentSafety = "";
        foreach (string rawLine in File.ReadAllLines(iniPath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(';')) continue;

            if (line.StartsWith("Safety=", StringComparison.OrdinalIgnoreCase))
            {
                currentSafety = line[7..].Trim();
            }
            else if (line.StartsWith("Package=", StringComparison.OrdinalIgnoreCase))
            {
                string pkg = line[8..].Trim();
                if (currentSafety.Equals("Protected", StringComparison.OrdinalIgnoreCase))
                    _protectedPrefixes.Add(pkg);
                else if (currentSafety.Equals("Safe", StringComparison.OrdinalIgnoreCase))
                    _safePrefixes.Add(pkg);
            }
        }
    }

    public static async Task<List<AppxItem>> ScanInstalledPackagesAsync()
    {
        LoadRules();
        var list = new List<AppxItem>();

        string script = "Get-AppxPackage | Select-Object Name, PackageFullName, Version | ConvertTo-Csv -NoTypeInformation";
        string output = await RunPowerShellAsync(script);

        using var reader = new StringReader(output);
        string? header = reader.ReadLine();
        if (header == null) return list;

        string? row;
        while ((row = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;
            // Parse CSV simples: "Name","PackageFullName","Version"
            var parts = Regex.Matches(row, @"(?<=^|,)(?:""(?<val>[^""]*)""|(?<val>[^,]*))")
                             .Cast<Match>()
                             .Select(m => m.Groups["val"].Value)
                             .ToArray();

            if (parts.Length >= 2)
            {
                string name = parts[0];
                string fullName = parts[1];
                string ver = parts.Length > 2 ? parts[2] : "";

                var safety = ClassifyPackage(name, fullName);

                list.Add(new AppxItem
                {
                    Name = name,
                    PackageFullName = fullName,
                    Version = ver,
                    Safety = safety,
                    Selected = (safety == PackageSafety.Safe)
                });
            }
        }

        return list.OrderBy(x => x.Safety).ThenBy(x => x.Name).ToList();
    }

    public static PackageSafety ClassifyPackage(string name, string fullName)
    {
        // Regras de proteção do sistema Windows
        if (fullName.Contains("_cw5n1h2txyewy", StringComparison.OrdinalIgnoreCase))
            return PackageSafety.Protected;

        foreach (var p in _protectedPrefixes)
        {
            if (p.StartsWith('*') && fullName.EndsWith(p[1..], StringComparison.OrdinalIgnoreCase))
                return PackageSafety.Protected;
            if (name.StartsWith(p, StringComparison.OrdinalIgnoreCase) || fullName.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                return PackageSafety.Protected;
        }

        // Regras seguras conhecidas
        foreach (var s in _safePrefixes)
        {
            if (s.StartsWith('*') && fullName.EndsWith(s[1..], StringComparison.OrdinalIgnoreCase))
                return PackageSafety.Safe;
            if (name.StartsWith(s, StringComparison.OrdinalIgnoreCase) || fullName.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                return PackageSafety.Safe;
        }

        return PackageSafety.Personal;
    }

    public static async Task<bool> RemovePackageAsync(string packageFullName)
    {
        string script = $"Remove-AppxPackage -Package '{packageFullName}' -ErrorAction SilentlyContinue";
        await RunPowerShellAsync(script);
        return true;
    }

    private static async Task<string> RunPowerShellAsync(string script)
    {
        return await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc == null) return string.Empty;
                string result = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(15000);
                return result;
            }
            catch
            {
                return string.Empty;
            }
        });
    }
}

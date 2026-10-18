using Microsoft.Win32;

namespace BloatynosyPro.Core;

public class WinRule
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Description { get; set; } = string.Empty;
    public bool DefaultSelected { get; set; }

    public string Path { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public string ValueType { get; set; } = "DWORD";
    public string RecommendedValue { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;

    // Valores adicionais (Path2, ValueName2, etc.)
    public List<WinRuleExtra> ExtraValues { get; set; } = new();

    public bool IsApplied { get; set; }
    public bool Selected { get; set; }
}

public class WinRuleExtra
{
    public string Path { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public string ValueType { get; set; } = "DWORD";
    public string RecommendedValue { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
}

public static class WinRulesEngine
{
    public static List<WinRule> Rules { get; private set; } = new();

    public static void LoadRules()
    {
        Rules.Clear();
        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string iniPath = System.IO.Path.Combine(appDir, "Data", "WinRules.ini");

        if (!File.Exists(iniPath))
            return;

        string[] lines = File.ReadAllLines(iniPath);
        WinRule? currentRule = null;
        Dictionary<int, WinRuleExtra> pendingExtras = new();

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(';'))
                continue;

            // Remove trailing inline comments
            int commentIdx = line.IndexOf(" ;", StringComparison.Ordinal);
            if (commentIdx >= 0)
                line = line[..commentIdx].Trim();

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                if (currentRule != null && !string.IsNullOrEmpty(currentRule.Path))
                {
                    currentRule.ExtraValues = pendingExtras.Values.ToList();
                    Rules.Add(currentRule);
                }

                pendingExtras.Clear();
                string sectionName = line[1..^1].Trim();
                currentRule = new WinRule
                {
                    Name = sectionName
                };
                continue;
            }

            if (currentRule == null)
                continue;

            int eqIdx = line.IndexOf('=');
            if (eqIdx <= 0)
                continue;

            string key = line[..eqIdx].Trim();
            string val = line[(eqIdx + 1)..].Trim();

            if (key.Equals("Category", StringComparison.OrdinalIgnoreCase))
                currentRule.Category = val;
            else if (key.Equals("Description", StringComparison.OrdinalIgnoreCase))
                currentRule.Description = val;
            else if (key.Equals("DefaultSelected", StringComparison.OrdinalIgnoreCase))
                currentRule.DefaultSelected = val.Equals("true", StringComparison.OrdinalIgnoreCase);
            else if (key.Equals("Path", StringComparison.OrdinalIgnoreCase))
                currentRule.Path = val;
            else if (key.Equals("ValueName", StringComparison.OrdinalIgnoreCase))
                currentRule.ValueName = val;
            else if (key.Equals("ValueType", StringComparison.OrdinalIgnoreCase))
                currentRule.ValueType = val;
            else if (key.Equals("RecommendedValue", StringComparison.OrdinalIgnoreCase))
                currentRule.RecommendedValue = val;
            else if (key.Equals("DefaultValue", StringComparison.OrdinalIgnoreCase))
                currentRule.DefaultValue = val;
            else if (key.StartsWith("Path", StringComparison.OrdinalIgnoreCase) && int.TryParse(key[4..], out int pNum))
            {
                GetOrCreateExtra(pendingExtras, pNum).Path = val;
            }
            else if (key.StartsWith("ValueName", StringComparison.OrdinalIgnoreCase) && int.TryParse(key[9..], out int vNum))
            {
                GetOrCreateExtra(pendingExtras, vNum).ValueName = val;
            }
            else if (key.StartsWith("ValueType", StringComparison.OrdinalIgnoreCase) && int.TryParse(key[9..], out int tNum))
            {
                GetOrCreateExtra(pendingExtras, tNum).ValueType = val;
            }
            else if (key.StartsWith("RecommendedValue", StringComparison.OrdinalIgnoreCase) && int.TryParse(key[16..], out int rNum))
            {
                GetOrCreateExtra(pendingExtras, rNum).RecommendedValue = val;
            }
            else if (key.StartsWith("DefaultValue", StringComparison.OrdinalIgnoreCase) && int.TryParse(key[12..], out int dNum))
            {
                GetOrCreateExtra(pendingExtras, dNum).DefaultValue = val;
            }
        }

        if (currentRule != null && !string.IsNullOrEmpty(currentRule.Path))
        {
            currentRule.ExtraValues = pendingExtras.Values.ToList();
            Rules.Add(currentRule);
        }
    }

    private static WinRuleExtra GetOrCreateExtra(Dictionary<int, WinRuleExtra> dict, int index)
    {
        if (!dict.TryGetValue(index, out var extra))
        {
            extra = new WinRuleExtra();
            dict[index] = extra;
        }
        return extra;
    }

    public static void CheckAll()
    {
        foreach (var rule in Rules)
        {
            rule.IsApplied = IsRuleApplied(rule);
            rule.Selected = rule.DefaultSelected && !rule.IsApplied;
        }
    }

    public static bool IsRuleApplied(WinRule rule)
    {
        if (!CheckRegistryValue(rule.Path, rule.ValueName, rule.ValueType, rule.RecommendedValue))
            return false;

        foreach (var extra in rule.ExtraValues)
        {
            string p = string.IsNullOrEmpty(extra.Path) ? rule.Path : extra.Path;
            string t = string.IsNullOrEmpty(extra.ValueType) ? rule.ValueType : extra.ValueType;
            if (!CheckRegistryValue(p, extra.ValueName, t, extra.RecommendedValue))
                return false;
        }

        return true;
    }

    private static bool CheckRegistryValue(string fullPath, string valueName, string valueType, string expected)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return false;

        try
        {
            var (baseKey, subKey) = ParseRegistryPath(fullPath);
            using var key = baseKey.OpenSubKey(subKey, false);
            if (key == null)
                return false;

            object? actual = key.GetValue(valueName.Equals("(Default)", StringComparison.OrdinalIgnoreCase) ? "" : valueName);
            if (actual == null)
                return false;

            if (valueType.Equals("DWORD", StringComparison.OrdinalIgnoreCase) || valueType.Equals("QWORD", StringComparison.OrdinalIgnoreCase))
            {
                if (long.TryParse(expected.StartsWith("0x") ? expected[2..] : expected, expected.StartsWith("0x") ? System.Globalization.NumberStyles.HexNumber : System.Globalization.NumberStyles.Integer, null, out long expNum))
                {
                    long actNum = Convert.ToInt64(actual);
                    return actNum == expNum;
                }
            }

            return actual.ToString()?.Equals(expected, StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch
        {
            return false;
        }
    }

    public static bool ApplyRule(WinRule rule)
    {
        bool success = WriteRegistryValue(rule.Path, rule.ValueName, rule.ValueType, rule.RecommendedValue);

        foreach (var extra in rule.ExtraValues)
        {
            string p = string.IsNullOrEmpty(extra.Path) ? rule.Path : extra.Path;
            string t = string.IsNullOrEmpty(extra.ValueType) ? rule.ValueType : extra.ValueType;
            success &= WriteRegistryValue(p, extra.ValueName, t, extra.RecommendedValue);
        }

        rule.IsApplied = IsRuleApplied(rule);
        return success;
    }

    public static bool RestoreRule(WinRule rule)
    {
        bool success = RestoreRegistryValue(rule.Path, rule.ValueName, rule.ValueType, rule.DefaultValue);

        foreach (var extra in rule.ExtraValues)
        {
            string p = string.IsNullOrEmpty(extra.Path) ? rule.Path : extra.Path;
            string t = string.IsNullOrEmpty(extra.ValueType) ? rule.ValueType : extra.ValueType;
            success &= RestoreRegistryValue(p, extra.ValueName, t, extra.DefaultValue);
        }

        rule.IsApplied = IsRuleApplied(rule);
        return success;
    }

    private static bool WriteRegistryValue(string fullPath, string valueName, string valueType, string value)
    {
        if (string.IsNullOrWhiteSpace(fullPath)) return false;
        try
        {
            var (baseKey, subKey) = ParseRegistryPath(fullPath);
            using var key = baseKey.CreateSubKey(subKey, true);
            if (key == null) return false;

            string vName = valueName.Equals("(Default)", StringComparison.OrdinalIgnoreCase) ? "" : valueName;

            if (valueType.Equals("DWORD", StringComparison.OrdinalIgnoreCase))
            {
                int intVal = ParseInt(value);
                key.SetValue(vName, intVal, RegistryValueKind.DWord);
            }
            else if (valueType.Equals("QWORD", StringComparison.OrdinalIgnoreCase))
            {
                long longVal = ParseLong(value);
                key.SetValue(vName, longVal, RegistryValueKind.QWord);
            }
            else
            {
                key.SetValue(vName, value, RegistryValueKind.String);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool RestoreRegistryValue(string fullPath, string valueName, string valueType, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(fullPath)) return false;
        try
        {
            var (baseKey, subKey) = ParseRegistryPath(fullPath);
            using var key = baseKey.OpenSubKey(subKey, true);
            if (key == null) return true;

            string vName = valueName.Equals("(Default)", StringComparison.OrdinalIgnoreCase) ? "" : valueName;

            if (defaultValue.Equals("<deletevalue>", StringComparison.OrdinalIgnoreCase))
            {
                key.DeleteValue(vName, false);
                return true;
            }
            if (defaultValue.Equals("<delete>", StringComparison.OrdinalIgnoreCase))
            {
                baseKey.DeleteSubKeyTree(subKey, false);
                return true;
            }

            return WriteRegistryValue(fullPath, valueName, valueType, defaultValue);
        }
        catch
        {
            return false;
        }
    }

    private static (RegistryKey baseKey, string subKey) ParseRegistryPath(string path)
    {
        string p = path.Trim();
        if (p.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase) || p.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
        {
            int idx = p.IndexOf('\\');
            return (Registry.CurrentUser, p[(idx + 1)..]);
        }
        if (p.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase) || p.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase))
        {
            int idx = p.IndexOf('\\');
            return (Registry.LocalMachine, p[(idx + 1)..]);
        }
        if (p.StartsWith("HKCR\\", StringComparison.OrdinalIgnoreCase) || p.StartsWith("HKEY_CLASSES_ROOT\\", StringComparison.OrdinalIgnoreCase))
        {
            int idx = p.IndexOf('\\');
            return (Registry.ClassesRoot, p[(idx + 1)..]);
        }
        return (Registry.CurrentUser, p);
    }

    private static int ParseInt(string s)
    {
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return Convert.ToInt32(s[2..], 16);
        return int.TryParse(s, out int r) ? r : 0;
    }

    private static long ParseLong(string s)
    {
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return Convert.ToInt64(s[2..], 16);
        return long.TryParse(s, out long r) ? r : 0;
    }
}

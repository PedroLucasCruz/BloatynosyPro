using System.IO;
using Microsoft.Win32;

namespace BloatynosyPro.Core;

public record DeepCleanResult(int FoldersRemoved, int FilesRemoved, int RegistryKeysRemoved);

public static class DeepUninstallService
{
    public static async Task<DeepCleanResult> PurgeAppResidualsAsync(string appName, string packageFullName)
    {
        return await Task.Run(() =>
        {
            int foldersCount = 0;
            int filesCount = 0;
            int regCount = 0;

            if (string.IsNullOrWhiteSpace(appName) && string.IsNullOrWhiteSpace(packageFullName))
                return new DeepCleanResult(0, 0, 0);

            // 1. Limpeza em %LocalAppData%\Packages\<PackageFullName>*
            string localPackages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages");
            if (Directory.Exists(localPackages))
            {
                try
                {
                    foreach (var dir in Directory.EnumerateDirectories(localPackages))
                    {
                        string dirName = Path.GetFileName(dir);
                        if ((!string.IsNullOrEmpty(packageFullName) && dirName.Contains(packageFullName, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(appName) && dirName.StartsWith(appName, StringComparison.OrdinalIgnoreCase)))
                        {
                            var (fld, fls) = SafeDeleteDirectory(dir);
                            foldersCount += fld;
                            filesCount += fls;
                        }
                    }
                }
                catch { }
            }

            // 2. Limpeza em %LocalAppData%\<AppName>
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            CleanMatchingAppFolder(localAppData, appName, ref foldersCount, ref filesCount);

            // 3. Limpeza em %AppData%\<AppName>
            string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            CleanMatchingAppFolder(roamingAppData, appName, ref foldersCount, ref filesCount);

            // 4. Limpeza em C:\ProgramData\<AppName>
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            CleanMatchingAppFolder(programData, appName, ref foldersCount, ref filesCount);

            // 5. Limpeza de chaves de registro residuais em HKCU\Software\<AppName>
            if (!string.IsNullOrWhiteSpace(appName) && appName.Length > 3)
            {
                regCount += SafeDeleteRegistryKey(Registry.CurrentUser, @"Software", appName);
            }

            return new DeepCleanResult(foldersCount, filesCount, regCount);
        });
    }

    private static void CleanMatchingAppFolder(string baseDir, string appName, ref int foldersCount, ref int filesCount)
    {
        if (string.IsNullOrWhiteSpace(appName) || appName.Length < 3 || !Directory.Exists(baseDir))
            return;

        try
        {
            foreach (var sub in Directory.EnumerateDirectories(baseDir))
            {
                string name = Path.GetFileName(sub);
                // Evita remover pastas genericas vitais como "Microsoft" ou "Windows"
                if (name.Equals("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Programs", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Temp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (name.Equals(appName, StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith(appName + ".", StringComparison.OrdinalIgnoreCase))
                {
                    var (fld, fls) = SafeDeleteDirectory(sub);
                    foldersCount += fld;
                    filesCount += fls;
                }
            }
        }
        catch { }
    }

    private static (int folders, int files) SafeDeleteDirectory(string path)
    {
        int fld = 0;
        int fls = 0;
        try
        {
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                fls = dir.GetFiles("*", SearchOption.AllDirectories).Length;
                fld = dir.GetDirectories("*", SearchOption.AllDirectories).Length + 1;
                dir.Delete(true);
            }
        }
        catch
        {
            // Se algum arquivo estiver em uso, tenta excluir os possiveis
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var f in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try { f.Delete(); fls++; } catch { }
                }
            }
            catch { }
        }

        return (fld, fls);
    }

    private static int SafeDeleteRegistryKey(RegistryKey root, string subKeyPath, string targetName)
    {
        int deleted = 0;
        try
        {
            using var parent = root.OpenSubKey(subKeyPath, true);
            if (parent != null)
            {
                foreach (var name in parent.GetSubKeyNames())
                {
                    if (name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        parent.DeleteSubKeyTree(name, false);
                        deleted++;
                    }
                }
            }
        }
        catch { }

        return deleted;
    }
}

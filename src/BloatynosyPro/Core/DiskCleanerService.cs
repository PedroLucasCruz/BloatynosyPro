using System.IO;

namespace BloatynosyPro.Core;

public record JunkLocation(string Name, string Path, long SizeBytes, int FileCount);

public static class DiskCleanerService
{
    public static List<JunkLocation> ScanJunk()
    {
        var locations = new List<JunkLocation>();

        // 1. User Temp
        string userTemp = Path.GetTempPath();
        locations.Add(MeasureFolder("Arquivos Temporários do Usuário (%TEMP%)", userTemp));

        // 2. Windows Temp
        string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
        locations.Add(MeasureFolder("Arquivos Temporários do Sistema (Windows\\Temp)", winTemp));

        // 3. Windows Prefetch
        string prefetch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
        locations.Add(MeasureFolder("Cache de Inicialização (Prefetch)", prefetch));

        // 4. Windows Crash Dumps / Error Reports
        string crashDumps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrashDumps");
        locations.Add(MeasureFolder("Relatórios de Erros e Falhas (CrashDumps)", crashDumps));

        return locations;
    }

    private static JunkLocation MeasureFolder(string name, string folderPath)
    {
        long bytes = 0;
        int count = 0;

        if (Directory.Exists(folderPath))
        {
            try
            {
                var dirInfo = new DirectoryInfo(folderPath);
                foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        bytes += file.Length;
                        count++;
                    }
                    catch { }
                }
            }
            catch { }
        }

        return new JunkLocation(name, folderPath, bytes, count);
    }

    public static async Task<long> CleanJunkAsync(List<string> folderPaths)
    {
        return await Task.Run(() =>
        {
            long freed = 0;
            foreach (var folder in folderPaths)
            {
                if (!Directory.Exists(folder)) continue;
                try
                {
                    var dir = new DirectoryInfo(folder);
                    foreach (var file in dir.EnumerateFiles())
                    {
                        try
                        {
                            long len = file.Length;
                            file.Delete();
                            freed += len;
                        }
                        catch { }
                    }

                    foreach (var sub in dir.EnumerateDirectories())
                    {
                        try
                        {
                            sub.Delete(true);
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return freed;
        });
    }
}

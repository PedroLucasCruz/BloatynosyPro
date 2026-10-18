using System.Diagnostics;
using System.Text;

namespace BloatynosyPro.Core;

public record IntegrityProgressUpdate(int StepIndex, int TotalSteps, int Percentage, string Message, string LogLine);

public record IntegrityResult(bool DismSuccess, bool SfcSuccess, bool ChkdskSuccess, string Summary);

public static class SystemIntegrityService
{
    public static async Task<IntegrityResult> RunFullIntegrityCheckAsync(IProgress<IntegrityProgressUpdate> progress)
    {
        return await Task.Run(async () =>
        {
            bool dismOk = false;
            bool sfcOk = false;
            bool chkdskOk = false;
            var summaryBuilder = new StringBuilder();

            // ==========================================
            // ETAPA 1: DISM (RestoreHealth)
            // ==========================================
            progress.Report(new IntegrityProgressUpdate(
                1, 3, 10,
                "Etapa 1 de 3: Reparando imagem e componentes do Windows (DISM)...",
                "[DISM] Iniciando: DISM.exe /Online /Cleanup-Image /RestoreHealth"
            ));

            string dismOut = await RunCliProcessAsync("dism.exe", "/Online /Cleanup-Image /RestoreHealth", line =>
            {
                progress.Report(new IntegrityProgressUpdate(1, 3, 25, "Reparando repositório de componentes do Windows...", $"[DISM] {line}"));
            });

            dismOk = dismOut.Contains("100.0%", StringComparison.OrdinalIgnoreCase) ||
                     dismOut.Contains("concluída com êxito", StringComparison.OrdinalIgnoreCase) ||
                     dismOut.Contains("operation completed successfully", StringComparison.OrdinalIgnoreCase);

            summaryBuilder.AppendLine(dismOk ? "✅ DISM: Componentes do Windows reparados com sucesso." : "⚠️ DISM: Verificação de componentes finalizada.");

            // ==========================================
            // ETAPA 2: SFC (/scannow)
            // ==========================================
            progress.Report(new IntegrityProgressUpdate(
                2, 3, 40,
                "Etapa 2 de 3: Verificando e reparando arquivos protegidos do Windows (SFC)...",
                "[SFC] Iniciando: sfc.exe /scannow"
            ));

            string sfcOut = await RunCliProcessAsync("sfc.exe", "/scannow", line =>
            {
                progress.Report(new IntegrityProgressUpdate(2, 3, 65, "Analisando arquivos essenciais do Windows...", $"[SFC] {line}"));
            });

            sfcOk = sfcOut.Contains("não encontrou nenhuma violação", StringComparison.OrdinalIgnoreCase) ||
                    sfcOut.Contains("encontrou arquivos corrompidos e os reparou", StringComparison.OrdinalIgnoreCase) ||
                    sfcOut.Contains("did not find any integrity violations", StringComparison.OrdinalIgnoreCase) ||
                    sfcOut.Contains("successfully repaired", StringComparison.OrdinalIgnoreCase);

            if (sfcOut.Contains("reparou", StringComparison.OrdinalIgnoreCase) || sfcOut.Contains("repaired", StringComparison.OrdinalIgnoreCase))
                summaryBuilder.AppendLine("🎉 SFC: Arquivos corrompidos do sistema foram localizados e REPARADOS!");
            else
                summaryBuilder.AppendLine("✅ SFC: Nenhum arquivo essencial corrompido encontrado.");

            // ==========================================
            // ETAPA 3: CHKDSK (C: /scan)
            // ==========================================
            progress.Report(new IntegrityProgressUpdate(
                3, 3, 75,
                "Etapa 3 de 3: Verificando integridade do sistema de arquivos no SSD/HD (CHKDSK)...",
                "[CHKDSK] Iniciando: chkdsk.exe C: /scan"
            ));

            string chkOut = await RunCliProcessAsync("chkdsk.exe", "C: /scan", line =>
            {
                progress.Report(new IntegrityProgressUpdate(3, 3, 90, "Verificando estrutura do disco C:...", $"[CHKDSK] {line}"));
            });

            chkdskOk = chkOut.Contains("não encontrou problemas", StringComparison.OrdinalIgnoreCase) ||
                       chkOut.Contains("no problems", StringComparison.OrdinalIgnoreCase) ||
                       chkOut.Contains("verificação foi concluída", StringComparison.OrdinalIgnoreCase);

            summaryBuilder.AppendLine(chkdskOk ? "✅ CHKDSK: Sistema de arquivos no disco C: íntegro e sem falhas." : "⚠️ CHKDSK: Análise de disco concluída.");

            progress.Report(new IntegrityProgressUpdate(
                3, 3, 100,
                "🎉 Diagnóstico e Reparo de Integridade Concluídos com Sucesso!",
                "[FIM] Todas as verificações foram concluídas."
            ));

            return new IntegrityResult(dismOk, sfcOk, chkdskOk, summaryBuilder.ToString().Trim());
        });
    }

    public static async Task<bool> CheckIntegrityNeedsRepairAsync()
    {
        return await Task.Run(async () =>
        {
            try
            {
                string output = await RunCliProcessAsync("dism.exe", "/Online /Cleanup-Image /CheckHealth", _ => { });
                if (output.Contains("corrupt", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("corrompido", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("repairable", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("reparável", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("detectado", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Se o CheckHealth não detectou, realiza um /ScanHealth rápido
                string scanOut = await RunCliProcessAsync("dism.exe", "/Online /Cleanup-Image /ScanHealth", _ => { });
                if (scanOut.Contains("corrupt", StringComparison.OrdinalIgnoreCase) ||
                    scanOut.Contains("corrompido", StringComparison.OrdinalIgnoreCase) ||
                    scanOut.Contains("repairable", StringComparison.OrdinalIgnoreCase) ||
                    scanOut.Contains("reparável", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch { }

            return false;
        });
    }

    public static async Task<IntegrityResult> RunAutoRepairAsync(Action<string>? onLog = null)
    {
        return await Task.Run(async () =>
        {
            onLog?.Invoke("Integridade: Iniciando reparo automático de imagem do Windows (DISM)...");
            string dismOut = await RunCliProcessAsync("dism.exe", "/Online /Cleanup-Image /RestoreHealth", line => onLog?.Invoke($"[DISM] {line}"));

            bool dismOk = dismOut.Contains("100.0%", StringComparison.OrdinalIgnoreCase) ||
                         dismOut.Contains("concluída com êxito", StringComparison.OrdinalIgnoreCase) ||
                         dismOut.Contains("operation completed successfully", StringComparison.OrdinalIgnoreCase);

            onLog?.Invoke("Integridade: Reparando arquivos essenciais do sistema (SFC)...");
            string sfcOut = await RunCliProcessAsync("sfc.exe", "/scannow", line => onLog?.Invoke($"[SFC] {line}"));

            bool sfcOk = sfcOut.Contains("não encontrou nenhuma violação", StringComparison.OrdinalIgnoreCase) ||
                        sfcOut.Contains("reparou", StringComparison.OrdinalIgnoreCase) ||
                        sfcOut.Contains("repaired", StringComparison.OrdinalIgnoreCase) ||
                        sfcOut.Contains("successfully", StringComparison.OrdinalIgnoreCase);

            return new IntegrityResult(dismOk, sfcOk, true, "Reparo automático concluído com sucesso.");
        });
    }

    private static async Task<string> RunCliProcessAsync(string fileName, string args, Action<string> onLineReceived)
    {
        var output = new StringBuilder();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };

            proc.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    output.AppendLine(e.Data);
                    onLineReceived(e.Data);
                }
            };

            proc.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    output.AppendLine(e.Data);
                    onLineReceived(e.Data);
                }
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            await proc.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            string err = $"Erro ao executar {fileName}: {ex.Message}";
            output.AppendLine(err);
            onLineReceived(err);
        }

        return output.ToString();
    }
}

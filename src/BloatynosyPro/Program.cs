using BloatynosyPro.Core;
using BloatynosyPro.Forms;

namespace BloatynosyPro;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Inicia o Guardiao Automatico em tempo real (Auto-Pilot)
        AutoGuardianService.Start();

        bool startupOptimize = args.Any(a => a.Equals("--startup-optimize", StringComparison.OrdinalIgnoreCase));
        bool minimized = args.Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase));

        var mainForm = new MainForm(minimized || startupOptimize);

        if (startupOptimize)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(1500); // Aguarda o Windows carregar a bandeja
                await mainForm.ExecuteStartupOptimizeAsync();
            });
        }

        Application.Run(mainForm);
    }
}

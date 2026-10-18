using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class PerformanceView : UserControl
{
    private System.Windows.Forms.Timer _timer = null!;
    private ModernProgressBar _barRam = null!;
    private ModernProgressBar _barCpu = null!;
    private Label _lblRamValue = null!;
    private Label _lblCpuValue = null!;
    private Label _lblGpuInfo = null!;
    private Label _lblNetInfo = null!;
    private ListView _lvTopProcesses = null!;
    private ModernButton _btnCleanRam = null!;
    private Label _lblRamStatus = null!;

    public PerformanceView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
        StartMonitor();
    }

    private void InitializeUi()
    {
        Controls.Clear();

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(24),
            BackColor = Color.FromArgb(15, 17, 26)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 55));

        // Card 1: RAM & Clean
        var cardRam = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 8, 8),
            BackColor = Color.FromArgb(22, 25, 40)
        };
        var flowRam = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        flowRam.Controls.Add(new Label { Text = "🧠 Memória RAM & Otimização", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(139, 92, 246), AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

        _lblRamValue = new Label { Text = "Calculando...", Font = new Font("Segoe UI", 10.5f), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        _barRam = new ModernProgressBar { Width = 340, Height = 14, ProgressColor = Color.FromArgb(139, 92, 246), Margin = new Padding(0, 0, 0, 12) };

        _btnCleanRam = new ModernButton
        {
            Text = "🧹 Liberar Memória RAM Agora",
            NormalColor = Color.FromArgb(139, 92, 246),
            HoverColor = Color.FromArgb(167, 139, 250),
            Size = new Size(240, 40),
            Margin = new Padding(0, 0, 0, 6)
        };
        _btnCleanRam.Click += async (s, e) => await CleanRamAsync();

        _lblRamStatus = new Label { Text = "Descarrega páginas de memória ociosas com segurança.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(148, 163, 184), AutoSize = true };

        flowRam.Controls.Add(_lblRamValue);
        flowRam.Controls.Add(_barRam);
        flowRam.Controls.Add(_btnCleanRam);
        flowRam.Controls.Add(_lblRamStatus);
        cardRam.Controls.Add(flowRam);

        // Card 2: CPU & GPU & Network
        var cardCpu = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 0, 0, 8),
            BackColor = Color.FromArgb(22, 25, 40)
        };
        var flowCpu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        flowCpu.Controls.Add(new Label { Text = "⚡ Processador, GPU & Rede", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(6, 182, 212), AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

        _lblCpuValue = new Label { Text = "CPU: 0%", Font = new Font("Segoe UI", 10.5f), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        _barCpu = new ModernProgressBar { Width = 340, Height = 14, ProgressColor = Color.FromArgb(6, 182, 212), Margin = new Padding(0, 0, 0, 14) };

        _lblGpuInfo = new Label { Text = "GPU: Detectando...", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(226, 232, 240), AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        _lblNetInfo = new Label { Text = "Rede: 0 KB/s", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(226, 232, 240), AutoSize = true };

        flowCpu.Controls.Add(_lblCpuValue);
        flowCpu.Controls.Add(_barCpu);
        flowCpu.Controls.Add(_lblGpuInfo);
        flowCpu.Controls.Add(_lblNetInfo);
        cardCpu.Controls.Add(flowCpu);

        // Bottom: Top Processes ListView
        var cardProcs = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 0),
            BackColor = Color.FromArgb(20, 23, 36)
        };
        cardProcs.Controls.Add(new Label
        {
            Text = "🔥 Processos que mais consomem Recursos no Windows",
            Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Top,
            Height = 32
        });

        _lvTopProcesses = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            HeaderStyle = ColumnHeaderStyle.Nonclickable
        };
        _lvTopProcesses.Columns.Add("Processo", 220);
        _lvTopProcesses.Columns.Add("PID", 80);
        _lvTopProcesses.Columns.Add("Consumo de RAM", 140);

        cardProcs.Controls.Add(_lvTopProcesses);
        table.Controls.Add(cardRam, 0, 0);
        table.Controls.Add(cardCpu, 1, 0);
        table.SetColumnSpan(cardProcs, 2);
        table.Controls.Add(cardProcs, 0, 1);

        Controls.Add(table);
    }

    private void StartMonitor()
    {
        _timer = new System.Windows.Forms.Timer { Interval = 1500 };
        _timer.Tick += (s, e) => UpdateSnapshot();
        _timer.Start();
        UpdateSnapshot();
    }

    private void UpdateSnapshot()
    {
        var snap = SystemMetricsService.GetSnapshot();

        _barRam.Value = snap.Memory.UsagePercent;
        _lblRamValue.Text = $"Uso de RAM: {snap.Memory.UsagePercent}% ({snap.Memory.UsedGb:F1} GB usados de {snap.Memory.TotalGb:F1} GB)";

        _barCpu.Value = snap.CpuUsagePercent;
        _lblCpuValue.Text = $"Uso de CPU: {snap.CpuUsagePercent}%";

        _lblGpuInfo.Text = "🎮 GPU: " + string.Join(", ", snap.GpuNames);
        _lblNetInfo.Text = $"🌐 Tráfego de Rede: ↓ {snap.NetworkReceivedKbps:F1} KB/s  ↑ {snap.NetworkSentKbps:F1} KB/s";

        _lvTopProcesses.BeginUpdate();
        _lvTopProcesses.Items.Clear();
        foreach (var p in snap.TopProcesses)
        {
            var lvi = new ListViewItem(p.Name);
            lvi.SubItems.Add(p.Pid.ToString());
            lvi.SubItems.Add($"{p.MemoryMb:F1} MB");
            _lvTopProcesses.Items.Add(lvi);
        }
        _lvTopProcesses.EndUpdate();
    }

    private async Task CleanRamAsync()
    {
        _btnCleanRam.Enabled = false;
        _lblRamStatus.Text = "Descarregando memória de trabalho...";
        _lblRamStatus.ForeColor = Color.FromArgb(6, 182, 212);

        long freed = await MemoryOptimizer.CleanWorkingSetAsync();
        double freedMb = freed / (1024.0 * 1024.0);

        _lblRamStatus.Text = $"🎉 Sucesso! ~{freedMb:F0} MB de memória RAM liberados!";
        _lblRamStatus.ForeColor = Color.FromArgb(16, 185, 129);

        UpdateSnapshot();
        _btnCleanRam.Enabled = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
        base.Dispose(disposing);
    }
}

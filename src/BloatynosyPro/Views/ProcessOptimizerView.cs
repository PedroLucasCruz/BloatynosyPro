using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class ProcessOptimizerView : UserControl
{
    private ListView _lvProcesses = null!;
    private ComboBox _cbFilter = null!;
    private ModernButton _btnScan = null!;
    private ModernButton _btnOptimizeNow = null!;
    private Label _lblStatus = null!;
    private List<DetailedProcessItem> _items = new();

    public ProcessOptimizerView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
        LoadProcesses();
    }

    private void InitializeUi()
    {
        Controls.Clear();

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            BackColor = Color.FromArgb(15, 17, 26)
        };

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "🔥 Otimizador de Processos em Detalhes (Task Details)",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Analisa PIDs e processos em segundo plano. Finaliza processos de telemetria/ociosos e otimiza instâncias do svchost sem travar o Windows.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(0, 30)
        };

        var toolPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _cbFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f),
            Width = 240,
            Height = 36,
            Margin = new Padding(0, 4, 10, 0)
        };
        _cbFilter.Items.Add("⚠️ Apenas Prejudiciais / Seguros");
        _cbFilter.Items.Add("Todos os Processos");
        _cbFilter.Items.Add("Hospedeiros svchost");
        _cbFilter.SelectedIndex = 0;
        _cbFilter.SelectedIndexChanged += (s, e) => FilterList();

        _btnScan = new ModernButton
        {
            Text = "🔄 Atualizar PIDs",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(140, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnScan.Click += (s, e) => LoadProcesses();

        _btnOptimizeNow = new ModernButton
        {
            Text = "⚡ Otimizar Processos (1 Clique)",
            NormalColor = Color.FromArgb(16, 185, 129),
            Size = new Size(240, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnOptimizeNow.Click += async (s, e) => await ExecuteOptimizationAsync();

        toolPanel.Controls.Add(_cbFilter);
        toolPanel.Controls.Add(_btnScan);
        toolPanel.Controls.Add(_btnOptimizeNow);

        topPanel.Controls.Add(lblTitle);
        topPanel.Controls.Add(lblSubtitle);
        topPanel.Controls.Add(toolPanel);

        var cardList = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 10, 0, 0)
        };

        _lvProcesses = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvProcesses.Columns.Add("Processo", 220);
        _lvProcesses.Columns.Add("PID", 80);
        _lvProcesses.Columns.Add("Uso de RAM", 120);
        _lvProcesses.Columns.Add("Classificação de Segurança", 220);
        _lvProcesses.Columns.Add("Descrição / Ação Recomendada", 360);

        cardList.Controls.Add(_lvProcesses);

        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Text = "Carregando processos..."
        };

        mainPanel.Controls.Add(cardList);
        mainPanel.Controls.Add(topPanel);
        mainPanel.Controls.Add(_lblStatus);

        Controls.Add(mainPanel);
    }

    private void LoadProcesses()
    {
        _lblStatus.Text = "Examinando tabela de processos do Windows...";
        _items = ProcessOptimizerService.GetDetailedProcesses();
        FilterList();
        int harmful = _items.Count(x => x.Safety == ProcessSafetyLevel.SafeToKill);
        _lblStatus.Text = $"Total de processos examinados: {_items.Count} ({harmful} identificados como seguros para finalização).";
    }

    private void FilterList()
    {
        _lvProcesses.BeginUpdate();
        _lvProcesses.Items.Clear();

        IEnumerable<DetailedProcessItem> filtered = _cbFilter.SelectedIndex switch
        {
            0 => _items.Where(x => x.Safety == ProcessSafetyLevel.SafeToKill),
            2 => _items.Where(x => x.Name.Equals("svchost", StringComparison.OrdinalIgnoreCase)),
            _ => _items
        };

        foreach (var item in filtered)
        {
            var lvi = new ListViewItem(item.Name) { Tag = item };
            lvi.SubItems.Add(item.Pid.ToString());
            lvi.SubItems.Add($"{item.MemoryMb:F1} MB");

            string safetyText = item.Safety switch
            {
                ProcessSafetyLevel.SafeToKill => "⚠️ Prejudicial / Seguro Finalizar",
                ProcessSafetyLevel.CriticalSystem => "🛡️ Sistema Crítico (Protegido)",
                ProcessSafetyLevel.SystemSupport => "🧹 Segundo Plano (Otimizar RAM)",
                _ => "👤 Aplicativo do Usuário"
            };
            lvi.SubItems.Add(safetyText);
            lvi.SubItems.Add(item.Description);

            if (item.Safety == ProcessSafetyLevel.SafeToKill)
                lvi.ForeColor = Color.FromArgb(248, 113, 113);
            else if (item.Safety == ProcessSafetyLevel.CriticalSystem)
                lvi.ForeColor = Color.FromArgb(148, 163, 184);
            else
                lvi.ForeColor = Color.FromArgb(226, 232, 240);

            _lvProcesses.Items.Add(lvi);
        }
        _lvProcesses.EndUpdate();
    }

    private async Task ExecuteOptimizationAsync()
    {
        _btnOptimizeNow.Enabled = false;
        _lblStatus.Text = "Otimizando processos e finalizando telemetria/atualizadores ociosos...";
        _lblStatus.ForeColor = Color.FromArgb(6, 182, 212);

        var result = await ProcessOptimizerService.OptimizeProcessesAsync();

        _lblStatus.Text = $"🎉 Sucesso! {result.TerminatedCount} processos prejudiciais finalizados e {result.TrimmedCount} processos otimizados. ~{result.FreedMemoryMb:F0} MB recuperados!";
        _lblStatus.ForeColor = Color.FromArgb(16, 185, 129);

        LoadProcesses();
        _btnOptimizeNow.Enabled = true;
    }
}

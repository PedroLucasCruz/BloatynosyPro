using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class DiskCleanerView : UserControl
{
    private ListView _lvJunk = null!;
    private ModernButton _btnAnalyze = null!;
    private ModernButton _btnClean = null!;
    private Label _lblTotalJunk = null!;
    private Label _lblStatus = null!;
    private List<JunkLocation> _locations = new();

    public DiskCleanerView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
        AnalyzeJunk();
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
            Text = "🧹 Limpeza de Disco e Arquivos Temporários",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Elimine caches acumulados, relatórios de erro e resíduos de programas para liberar espaço e agilizar o disco.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(0, 30)
        };

        _lblTotalJunk = new Label
        {
            Text = "Espaço ocupado por lixo: Calculando...",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(245, 158, 11),
            AutoSize = true,
            Location = new Point(0, 56)
        };

        var toolPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _btnAnalyze = new ModernButton
        {
            Text = "🔍 Analisar Lixo",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(140, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnAnalyze.Click += (s, e) => AnalyzeJunk();

        _btnClean = new ModernButton
        {
            Text = "🧹 Limpar Selecionados",
            NormalColor = Color.FromArgb(16, 185, 129),
            Size = new Size(180, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnClean.Click += async (s, e) => await CleanJunkAsync();

        toolPanel.Controls.Add(_btnAnalyze);
        toolPanel.Controls.Add(_btnClean);

        topPanel.Controls.Add(lblTitle);
        topPanel.Controls.Add(lblSubtitle);
        topPanel.Controls.Add(_lblTotalJunk);
        topPanel.Controls.Add(toolPanel);

        var cardList = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 10, 0, 0)
        };

        _lvJunk = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvJunk.Columns.Add("Área de Limpeza", 340);
        _lvJunk.Columns.Add("Espaço Ocupado", 140);
        _lvJunk.Columns.Add("Qtd. de Arquivos", 140);
        _lvJunk.Columns.Add("Caminho no Disco", 420);

        cardList.Controls.Add(_lvJunk);

        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Text = "Pronto."
        };

        mainPanel.Controls.Add(cardList);
        mainPanel.Controls.Add(topPanel);
        mainPanel.Controls.Add(_lblStatus);

        Controls.Add(mainPanel);
    }

    private void AnalyzeJunk()
    {
        _lblStatus.Text = "Analisando diretórios temporários...";
        _locations = DiskCleanerService.ScanJunk();

        _lvJunk.BeginUpdate();
        _lvJunk.Items.Clear();

        long totalBytes = 0;
        foreach (var loc in _locations)
        {
            totalBytes += loc.SizeBytes;
            double mb = loc.SizeBytes / (1024.0 * 1024.0);

            var lvi = new ListViewItem(loc.Name)
            {
                Checked = true,
                Tag = loc
            };
            lvi.SubItems.Add($"{mb:F1} MB");
            lvi.SubItems.Add(loc.FileCount.ToString());
            lvi.SubItems.Add(loc.Path);

            _lvJunk.Items.Add(lvi);
        }
        _lvJunk.EndUpdate();

        double totalMb = totalBytes / (1024.0 * 1024.0);
        _lblTotalJunk.Text = totalMb >= 1024
            ? $"Espaço recuperável: {totalMb / 1024.0:F2} GB de resíduos"
            : $"Espaço recuperável: {totalMb:F1} MB de resíduos";

        _lblStatus.Text = "Análise concluída com sucesso.";
    }

    private async Task CleanJunkAsync()
    {
        var pathsToClean = new List<string>();
        foreach (ListViewItem item in _lvJunk.Items)
        {
            if (item.Checked && item.Tag is JunkLocation loc)
            {
                pathsToClean.Add(loc.Path);
            }
        }

        if (pathsToClean.Count == 0)
        {
            MessageBox.Show("Nenhuma pasta foi selecionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _btnClean.Enabled = false;
        _lblStatus.Text = "Excluindo arquivos temporários...";

        long freedBytes = await DiskCleanerService.CleanJunkAsync(pathsToClean);
        double freedMb = freedBytes / (1024.0 * 1024.0);

        _lblStatus.Text = $"🎉 Limpeza concluída! ~{freedMb:F1} MB liberados no seu disco!";
        AnalyzeJunk();
        _btnClean.Enabled = true;
    }
}

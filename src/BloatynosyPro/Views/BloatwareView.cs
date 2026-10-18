using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class BloatwareView : UserControl
{
    private ListView _lvApps = null!;
    private ModernButton _btnScan = null!;
    private ModernButton _btnSelectSafe = null!;
    private ModernButton _btnUninstall = null!;
    private CheckBox _chkDeepClean = null!;
    private Label _lblStatus = null!;
    private List<AppxItem> _installedPackages = new();

    public BloatwareView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
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
            Text = "🗑️ Desinstalador de Bloatwares & Limpeza Profunda",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Remova aplicativos nativos e patrocinados. A Desinstalação Profunda vasculha e apaga 100% dos resíduos em AppData, ProgramData e Registro.",
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

        _btnScan = new ModernButton
        {
            Text = "🔍 Buscar Apps",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(130, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnScan.Click += async (s, e) => await ScanAppsAsync();

        _btnSelectSafe = new ModernButton
        {
            Text = "✨ Selecionar Seguros",
            NormalColor = Color.FromArgb(59, 130, 246),
            Size = new Size(160, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnSelectSafe.Click += (s, e) => SelectSafeOnly();

        _btnUninstall = new ModernButton
        {
            Text = "🗑️ Desinstalar Selecionados",
            NormalColor = Color.FromArgb(239, 68, 68),
            Size = new Size(200, 36),
            Margin = new Padding(0, 2, 12, 0)
        };
        _btnUninstall.Click += async (s, e) => await UninstallSelectedAsync();

        _chkDeepClean = new CheckBox
        {
            Text = "🧹 Desinstalação Profunda (Limpar 100% de vestígios)",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 211, 153),
            AutoSize = true,
            Checked = true,
            Margin = new Padding(0, 8, 0, 0)
        };

        toolPanel.Controls.Add(_btnScan);
        toolPanel.Controls.Add(_btnSelectSafe);
        toolPanel.Controls.Add(_btnUninstall);
        toolPanel.Controls.Add(_chkDeepClean);

        topPanel.Controls.Add(lblTitle);
        topPanel.Controls.Add(lblSubtitle);
        topPanel.Controls.Add(toolPanel);

        var cardList = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 10, 0, 0)
        };

        _lvApps = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvApps.Columns.Add("Nome do Pacote", 340);
        _lvApps.Columns.Add("Classificação / Segurança", 180);
        _lvApps.Columns.Add("Versão", 140);
        _lvApps.Columns.Add("Nome Completo", 380);

        cardList.Controls.Add(_lvApps);

        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Text = "Clique em 'Buscar Apps' para listar pacotes instalados."
        };

        mainPanel.Controls.Add(cardList);
        mainPanel.Controls.Add(topPanel);
        mainPanel.Controls.Add(_lblStatus);

        Controls.Add(mainPanel);
    }

    private async Task ScanAppsAsync()
    {
        _btnScan.Enabled = false;
        _lblStatus.Text = "Consultando pacotes AppX no Windows via PowerShell...";

        _installedPackages = await AppxDebloater.ScanInstalledPackagesAsync();

        _lvApps.BeginUpdate();
        _lvApps.Items.Clear();

        foreach (var item in _installedPackages)
        {
            var lvi = new ListViewItem(item.Name)
            {
                Checked = item.Selected,
                Tag = item
            };

            string safetyLabel = item.Safety switch
            {
                PackageSafety.Safe => "🗑️ Seguro para Remover",
                PackageSafety.Protected => "🛡️ Protegido do Sistema",
                _ => "👤 Aplicativo Pessoal"
            };

            lvi.SubItems.Add(safetyLabel);
            lvi.SubItems.Add(item.Version);
            lvi.SubItems.Add(item.PackageFullName);

            if (item.Safety == PackageSafety.Safe)
                lvi.ForeColor = Color.FromArgb(248, 113, 113);
            else if (item.Safety == PackageSafety.Protected)
                lvi.ForeColor = Color.FromArgb(148, 163, 184);

            _lvApps.Items.Add(lvi);
        }
        _lvApps.EndUpdate();

        int safeCount = _installedPackages.Count(p => p.Safety == PackageSafety.Safe);
        _lblStatus.Text = $"Encontrados {_installedPackages.Count} pacotes instalados ({safeCount} identificados como bloatware seguro).";
        _btnScan.Enabled = true;
    }

    private void SelectSafeOnly()
    {
        foreach (ListViewItem item in _lvApps.Items)
        {
            if (item.Tag is AppxItem app)
            {
                item.Checked = (app.Safety == PackageSafety.Safe);
            }
        }
    }

    private async Task UninstallSelectedAsync()
    {
        var toRemove = new List<AppxItem>();
        foreach (ListViewItem item in _lvApps.Items)
        {
            if (item.Checked && item.Tag is AppxItem app)
            {
                toRemove.Add(app);
            }
        }

        if (toRemove.Count == 0)
        {
            MessageBox.Show("Nenhum aplicativo foi selecionado para remoção.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string deepMsg = _chkDeepClean.Checked
            ? "\n\n🧹 Desinstalação Profunda ATIVADA: Todos os vestígios em AppData, ProgramData e Registro serão expurgados."
            : "";

        var confirm = MessageBox.Show(
            $"Deseja realmente desinstalar os {toRemove.Count} aplicativos selecionados?{deepMsg}",
            "Confirmar Remoção",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (confirm != DialogResult.Yes) return;

        _btnUninstall.Enabled = false;
        int removed = 0;
        int totalFolders = 0;
        int totalFiles = 0;
        int totalReg = 0;

        foreach (var app in toRemove)
        {
            _lblStatus.Text = $"Desinstalando {app.Name}...";
            await AppxDebloater.RemovePackageAsync(app.PackageFullName);
            removed++;

            if (_chkDeepClean.Checked)
            {
                _lblStatus.Text = $"Vasculhando e expurgando vestígios de {app.Name}...";
                var cleanRes = await DeepUninstallService.PurgeAppResidualsAsync(app.Name, app.PackageFullName);
                totalFolders += cleanRes.FoldersRemoved;
                totalFiles += cleanRes.FilesRemoved;
                totalReg += cleanRes.RegistryKeysRemoved;
            }
        }

        string extraCleanMsg = _chkDeepClean.Checked
            ? $" Foram eliminados {totalFolders} pastas, {totalFiles} arquivos residuais e {totalReg} chaves de registro."
            : "";

        _lblStatus.Text = $"🎉 {removed} pacotes desinstalados com sucesso!{extraCleanMsg}";
        await ScanAppsAsync();
        _btnUninstall.Enabled = true;
    }
}

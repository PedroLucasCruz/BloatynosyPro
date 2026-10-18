using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Forms;

public class BlacklistManagerForm : Form
{
    private TabControl _tabMain = null!;
    private ListView _lvProcesses = null!;
    private ListView _lvServices = null!;
    private TextBox _txtNewProcess = null!;
    private TextBox _txtNewService = null!;
    private Label _lblStatus = null!;

    public BlacklistManagerForm()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Text = "Gerenciador da Lista Negra - Guardião BloatynosyPro";
        Size = new Size(760, 580);
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(15, 17, 26);
        ForeColor = Color.White;
        ShowIcon = false;
        InitializeUi();
        LoadData();
    }

    private void InitializeUi()
    {
        Controls.Clear();

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            BackColor = Color.FromArgb(15, 17, 26)
        };

        var lblTitle = new Label
        {
            Text = "📋 Gerenciador da Lista Negra do Guardião",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Consulte, adicione ou remova processos e serviços nativos que o Guardião monitora e derruba automaticamente.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(0, 30)
        };

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.Transparent
        };
        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblSubtitle);

        // TabControl Dark
        _tabMain = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10f),
            Margin = new Padding(0, 10, 0, 10)
        };

        // Tab 1: Processos
        var tabProcs = new TabPage("⚡ Processos Bloqueados")
        {
            BackColor = Color.FromArgb(20, 23, 36),
            Padding = new Padding(12)
        };

        var procTop = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _txtNewProcess = new TextBox
        {
            Font = new Font("Segoe UI", 10f),
            Width = 240,
            Height = 32,
            BackColor = Color.FromArgb(28, 32, 52),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Nome do processo (ex: Teams)"
        };

        var btnAddProc = new ModernButton
        {
            Text = "➕ Adicionar",
            NormalColor = Color.FromArgb(16, 185, 129),
            Size = new Size(110, 34),
            Margin = new Padding(8, 0, 8, 0)
        };
        btnAddProc.Click += (s, e) => AddProcess();

        var btnRemoveProc = new ModernButton
        {
            Text = "🗑️ Remover da Lista Negra",
            NormalColor = Color.FromArgb(239, 68, 68),
            Size = new Size(200, 34),
            Margin = new Padding(0)
        };
        btnRemoveProc.Click += (s, e) => RemoveSelectedProcess();

        procTop.Controls.Add(_txtNewProcess);
        procTop.Controls.Add(btnAddProc);
        procTop.Controls.Add(btnRemoveProc);

        _lvProcesses = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvProcesses.Columns.Add("Processo Bloqueado", 260);
        _lvProcesses.Columns.Add("Ação do Guardião", 240);
        _lvProcesses.Columns.Add("Status Atual", 150);

        tabProcs.Controls.Add(_lvProcesses);
        tabProcs.Controls.Add(procTop);

        // Tab 2: Servicos
        var tabSvcs = new TabPage("🛡️ Serviços Nativos (svchost)")
        {
            BackColor = Color.FromArgb(20, 23, 36),
            Padding = new Padding(12)
        };

        var svcTop = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _txtNewService = new TextBox
        {
            Font = new Font("Segoe UI", 10f),
            Width = 240,
            Height = 32,
            BackColor = Color.FromArgb(28, 32, 52),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Nome do serviço (ex: MapsBroker)"
        };

        var btnAddSvc = new ModernButton
        {
            Text = "➕ Adicionar",
            NormalColor = Color.FromArgb(16, 185, 129),
            Size = new Size(110, 34),
            Margin = new Padding(8, 0, 8, 0)
        };
        btnAddSvc.Click += (s, e) => AddService();

        var btnRemoveSvc = new ModernButton
        {
            Text = "🗑️ Remover & Reativar no Windows",
            NormalColor = Color.FromArgb(245, 158, 11),
            Size = new Size(240, 34),
            Margin = new Padding(0)
        };
        btnRemoveSvc.Click += (s, e) => RemoveSelectedService();

        svcTop.Controls.Add(_txtNewService);
        svcTop.Controls.Add(btnAddSvc);
        svcTop.Controls.Add(btnRemoveSvc);

        _lvServices = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvServices.Columns.Add("Nome do Serviço", 240);
        _lvServices.Columns.Add("Ação do Guardião", 260);
        _lvServices.Columns.Add("Status no Windows", 160);

        tabSvcs.Controls.Add(_lvServices);
        tabSvcs.Controls.Add(svcTop);

        _tabMain.TabPages.Add(tabProcs);
        _tabMain.TabPages.Add(tabSvcs);

        // Bottom Bar
        var bottomBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            BackColor = Color.Transparent
        };

        _lblStatus = new Label
        {
            Text = "Pronto.",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(0, 12)
        };

        var btnClose = new ModernButton
        {
            Text = "Fechar",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(100, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(mainPanel.Width - 140, 6)
        };
        btnClose.Click += (s, e) => Close();

        bottomBar.Controls.Add(_lblStatus);
        bottomBar.Controls.Add(btnClose);

        mainPanel.Controls.Add(_tabMain);
        mainPanel.Controls.Add(headerPanel);
        mainPanel.Controls.Add(bottomBar);

        Controls.Add(mainPanel);
    }

    private void LoadData()
    {
        // 1. Carrega Processos
        _lvProcesses.BeginUpdate();
        _lvProcesses.Items.Clear();
        foreach (var p in AutoStartupService.CurrentConfig.BlacklistedProcesses)
        {
            var lvi = new ListViewItem(p);
            lvi.SubItems.Add("🚫 Finalizar imediatamente");
            bool isRunning = System.Diagnostics.Process.GetProcessesByName(p).Length > 0;
            lvi.SubItems.Add(isRunning ? "⚠️ Em Execução" : "🟢 Suprimido");
            if (isRunning) lvi.ForeColor = Color.FromArgb(248, 113, 113);
            _lvProcesses.Items.Add(lvi);
        }
        _lvProcesses.EndUpdate();

        // 2. Carrega Servicos
        _lvServices.BeginUpdate();
        _lvServices.Items.Clear();
        foreach (var s in AutoStartupService.CurrentConfig.BlacklistedServices)
        {
            var lvi = new ListViewItem(s);
            lvi.SubItems.Add("🚫 Bloqueado (start= disabled)");
            lvi.SubItems.Add("Desativado");
            _lvServices.Items.Add(lvi);
        }
        _lvServices.EndUpdate();

        _lblStatus.Text = $"Total: {AutoStartupService.CurrentConfig.BlacklistedProcesses.Count} processos e {AutoStartupService.CurrentConfig.BlacklistedServices.Count} serviços cadastrados na lista negra.";
    }

    private void AddProcess()
    {
        string name = _txtNewProcess.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Digite o nome do processo a ser bloqueado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AutoStartupService.AddBlacklistedProcess(name);
        _txtNewProcess.Clear();
        LoadData();
        _lblStatus.Text = $"🎉 Processo '{name}' adicionado à lista negra com sucesso!";
    }

    private void RemoveSelectedProcess()
    {
        if (_lvProcesses.SelectedItems.Count == 0)
        {
            MessageBox.Show("Selecione um processo da lista para remover.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string pName = _lvProcesses.SelectedItems[0].Text;
        var confirm = MessageBox.Show(
            $"Deseja remover '{pName}' da lista negra do Guardião? O Guardião deixará de derrubá-lo automaticamente.",
            "Confirmar Remoção",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm != DialogResult.Yes) return;

        AutoStartupService.RemoveBlacklistedProcess(pName);
        LoadData();
        _lblStatus.Text = $"✅ Processo '{pName}' removido da lista negra!";
    }

    private void AddService()
    {
        string name = _txtNewService.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Digite o nome do serviço a ser bloqueado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AutoStartupService.AddBlacklistedService(name);
        _txtNewService.Clear();
        LoadData();
        _lblStatus.Text = $"🎉 Serviço '{name}' adicionado à lista negra com sucesso!";
    }

    private void RemoveSelectedService()
    {
        if (_lvServices.SelectedItems.Count == 0)
        {
            MessageBox.Show("Selecione um serviço da lista para remover.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string sName = _lvServices.SelectedItems[0].Text;
        var confirm = MessageBox.Show(
            $"Deseja remover '{sName}' da lista negra e reativá-lo no Windows (Modo Manual)?",
            "Confirmar Remoção e Reativação",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm != DialogResult.Yes) return;

        AutoStartupService.RemoveBlacklistedService(sName, reenableInWindows: true);
        LoadData();
        _lblStatus.Text = $"✅ Serviço '{sName}' removido da lista negra e reativado no Windows!";
    }
}

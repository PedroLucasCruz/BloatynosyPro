using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;
using BloatynosyPro.Views;

namespace BloatynosyPro.Forms;

public partial class MainForm : Form
{
    private Panel _sidebarPanel = null!;
    private Panel _contentPanel = null!;
    private List<Button> _navButtons = new();

    private DashboardView _dashboardView = null!;
    private PerformanceView _performanceView = null!;
    private ProcessOptimizerView _processOptView = null!;
    private SystemIntegrityView _integrityView = null!;
    private SystemTweaksView _tweaksView = null!;
    private BloatwareView _bloatwareView = null!;
    private DiskCleanerView _cleanerView = null!;
    private StartupManagerView _startupView = null!;
    private SettingsView _settingsView = null!;

    private NotifyIcon _trayIcon = null!;

    public MainForm(bool startMinimized = false)
    {
        InitializeComponent();
        ConfigureForm();
        InitializeViews();
        InitializeTray();
        Loc.LanguageChanged += (_, _) => UpdateLanguage();

        if (startMinimized)
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }
    }

    private void ConfigureForm()
    {
        Text = "BloatynosyPro · Otimizador Gamer & Integridade do Windows";
        Size = new Size(1240, 800);
        MinimumSize = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(15, 17, 26);
        ForeColor = Color.White;
        DoubleBuffered = true;
    }

    private void InitializeTray()
    {
        _trayIcon = new NotifyIcon
        {
            Text = "BloatynosyPro",
            Icon = SystemIcons.Application,
            Visible = true
        };

        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("⚡ Abrir BloatynosyPro", null, (s, e) => RestoreFromTray());
        trayMenu.Items.Add("🧹 Otimizar PC Agora", null, async (s, e) => await ExecuteQuickTrayOptimizeAsync());
        trayMenu.Items.Add("-");
        trayMenu.Items.Add("❌ Sair", null, (s, e) => { _trayIcon.Visible = false; Application.Exit(); });

        _trayIcon.ContextMenuStrip = trayMenu;
        _trayIcon.DoubleClick += (s, e) => RestoreFromTray();
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        ShowInTaskbar = true;
        BringToFront();
    }

    public async Task ExecuteStartupOptimizeAsync()
    {
        _trayIcon.ShowBalloonTip(3000, "BloatynosyPro", "Otimizando inicialização do Windows em segundo plano...", ToolTipIcon.Info);

        await ProcessOptimizerService.OptimizeProcessesAsync();
        await MemoryOptimizer.CleanWorkingSetAsync();

        await Task.Run(() =>
        {
            if (WinRulesEngine.Rules.Count == 0)
                WinRulesEngine.LoadRules();

            WinRulesEngine.CheckAll();
            var preset = AutoStartupService.CurrentConfig.StartupPreset;

            if (preset.Contains("Gamer", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var r in WinRulesEngine.Rules.Where(x => x.Category.Equals("Gaming", StringComparison.OrdinalIgnoreCase) || x.DefaultSelected))
                    WinRulesEngine.ApplyRule(r);
            }
            else if (preset.Contains("Priva", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var r in WinRulesEngine.Rules.Where(x => x.Category.Equals("Privacy", StringComparison.OrdinalIgnoreCase) || x.Category.Equals("Ads", StringComparison.OrdinalIgnoreCase)))
                    WinRulesEngine.ApplyRule(r);
            }
            else
            {
                foreach (var r in WinRulesEngine.Rules.Where(x => x.DefaultSelected && !x.IsApplied))
                    WinRulesEngine.ApplyRule(r);
            }
        });

        _trayIcon.ShowBalloonTip(4000, "BloatynosyPro", "🎉 Computador otimizado com sucesso para inicialização rápida!", ToolTipIcon.Info);
    }

    private async Task ExecuteQuickTrayOptimizeAsync()
    {
        await ProcessOptimizerService.OptimizeProcessesAsync();
        await MemoryOptimizer.CleanWorkingSetAsync();
        _trayIcon.ShowBalloonTip(3000, "BloatynosyPro", "Processos otimizados e memória RAM liberada!", ToolTipIcon.Info);
    }

    private void InitializeViews()
    {
        _sidebarPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 245,
            BackColor = Color.FromArgb(20, 23, 36),
            Padding = new Padding(0)
        };

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 85,
            BackColor = Color.FromArgb(24, 28, 44),
            Padding = new Padding(16, 16, 16, 10)
        };

        var lblLogo = new Label
        {
            Text = "⚡ BloatynosyPro",
            Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(6, 182, 212),
            AutoSize = true,
            Location = new Point(14, 18)
        };

        var lblSub = new Label
        {
            Text = "Windows Optimizer v1.2",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(16, 46)
        };

        headerPanel.Controls.Add(lblLogo);
        headerPanel.Controls.Add(lblSub);

        var navFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(8, 8, 8, 8),
            AutoScroll = true
        };

        var btnDash = CreateNavButton("⚡ Visão Geral", 0);
        var btnPerf = CreateNavButton("📊 Desempenho", 1);
        var btnProcs = CreateNavButton("🔥 Otimizar Processos", 2);
        var btnIntegrity = CreateNavButton("🛡️ Integridade do Windows", 3);
        var btnTweaks = CreateNavButton("⚙️ Otimizações", 4);
        var btnBloat = CreateNavButton("🗑️ Bloatwares & Limpeza", 5);
        var btnClean = CreateNavButton("🧹 Limpeza de Disco", 6);
        var btnStartup = CreateNavButton("🚀 Inicialização", 7);
        var btnSettings = CreateNavButton("🔧 Configurações", 8);

        navFlow.Controls.Add(btnDash);
        navFlow.Controls.Add(btnPerf);
        navFlow.Controls.Add(btnProcs);
        navFlow.Controls.Add(btnIntegrity);
        navFlow.Controls.Add(btnTweaks);
        navFlow.Controls.Add(btnBloat);
        navFlow.Controls.Add(btnClean);
        navFlow.Controls.Add(btnStartup);
        navFlow.Controls.Add(btnSettings);

        _sidebarPanel.Controls.Add(navFlow);
        _sidebarPanel.Controls.Add(headerPanel);

        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 17, 26)
        };

        Controls.Add(_contentPanel);
        Controls.Add(_sidebarPanel);

        _dashboardView = new DashboardView();
        _performanceView = new PerformanceView();
        _processOptView = new ProcessOptimizerView();
        _integrityView = new SystemIntegrityView();
        _tweaksView = new SystemTweaksView();
        _bloatwareView = new BloatwareView();
        _cleanerView = new DiskCleanerView();
        _startupView = new StartupManagerView();
        _settingsView = new SettingsView();

        ShowView(0);
    }

    private Button CreateNavButton(string text, int index)
    {
        var btn = new Button
        {
            Text = "   " + text,
            Tag = index,
            Width = 225,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(203, 213, 225),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 2)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 37, 58);
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 46, 72);

        btn.Click += (s, e) => ShowView((int)btn.Tag!);
        _navButtons.Add(btn);
        return btn;
    }

    private void ShowView(int index)
    {
        for (int i = 0; i < _navButtons.Count; i++)
        {
            if (i == index)
            {
                _navButtons[i].BackColor = Color.FromArgb(99, 102, 241);
                _navButtons[i].ForeColor = Color.White;
            }
            else
            {
                _navButtons[i].BackColor = Color.Transparent;
                _navButtons[i].ForeColor = Color.FromArgb(203, 213, 225);
            }
        }

        _contentPanel.Controls.Clear();
        Control targetView = index switch
        {
            0 => _dashboardView,
            1 => _performanceView,
            2 => _processOptView,
            3 => _integrityView,
            4 => _tweaksView,
            5 => _bloatwareView,
            6 => _cleanerView,
            7 => _startupView,
            _ => _settingsView
        };

        targetView.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(targetView);
    }

    private void UpdateLanguage()
    {
        Text = Loc.Get("App_Title");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        base.OnFormClosing(e);
    }
}

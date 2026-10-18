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
    private SystemTweaksView _tweaksView = null!;
    private BloatwareView _bloatwareView = null!;
    private DiskCleanerView _cleanerView = null!;
    private StartupManagerView _startupView = null!;
    private SettingsView _settingsView = null!;

    public MainForm()
    {
        InitializeComponent();
        ConfigureForm();
        InitializeViews();
        Loc.LanguageChanged += (_, _) => UpdateLanguage();
    }

    private void ConfigureForm()
    {
        Text = "BloatynosyPro · Otimizador Gamer & Privacidade";
        Size = new Size(1180, 760);
        MinimumSize = new Size(980, 640);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(15, 17, 26);
        ForeColor = Color.White;
        DoubleBuffered = true;
    }

    private void InitializeViews()
    {
        // 1. Sidebar Panel
        _sidebarPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 240,
            BackColor = Color.FromArgb(20, 23, 36),
            Padding = new Padding(0)
        };

        // App Header / Logo
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
            Text = "Windows Optimizer v1.0",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(16, 46)
        };

        headerPanel.Controls.Add(lblLogo);
        headerPanel.Controls.Add(lblSub);

        // Navigation Flow
        var navFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(8, 12, 8, 8),
            AutoScroll = true
        };

        var btnDash = CreateNavButton("⚡ Visão Geral", 0);
        var btnPerf = CreateNavButton("📊 Desempenho", 1);
        var btnTweaks = CreateNavButton("🛡️ Otimizações", 2);
        var btnBloat = CreateNavButton("🗑️ Bloatwares", 3);
        var btnClean = CreateNavButton("🧹 Limpeza de Disco", 4);
        var btnStartup = CreateNavButton("🚀 Inicialização", 5);
        var btnSettings = CreateNavButton("⚙️ Configurações", 6);

        navFlow.Controls.Add(btnDash);
        navFlow.Controls.Add(btnPerf);
        navFlow.Controls.Add(btnTweaks);
        navFlow.Controls.Add(btnBloat);
        navFlow.Controls.Add(btnClean);
        navFlow.Controls.Add(btnStartup);
        navFlow.Controls.Add(btnSettings);

        _sidebarPanel.Controls.Add(navFlow);
        _sidebarPanel.Controls.Add(headerPanel);

        // 2. Content Panel
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 17, 26)
        };

        Controls.Add(_contentPanel);
        Controls.Add(_sidebarPanel);

        // 3. Views Instantiation
        _dashboardView = new DashboardView();
        _performanceView = new PerformanceView();
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
            Width = 220,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(203, 213, 225),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 4)
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
                _navButtons[i].BackColor = Color.FromArgb(99, 102, 241); // Accent Purple
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
            2 => _tweaksView,
            3 => _bloatwareView,
            4 => _cleanerView,
            5 => _startupView,
            _ => _settingsView
        };

        targetView.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(targetView);
    }

    private void UpdateLanguage()
    {
        Text = Loc.Get("App_Title");
        if (_navButtons.Count >= 7)
        {
            _navButtons[0].Text = "   ⚡ " + Loc.Get("Nav_Dashboard");
            _navButtons[1].Text = "   📊 " + Loc.Get("Nav_Performance");
            _navButtons[2].Text = "   🛡️ " + Loc.Get("Nav_Tweaks");
            _navButtons[3].Text = "   🗑️ " + Loc.Get("Nav_Bloatware");
            _navButtons[4].Text = "   🧹 " + Loc.Get("Nav_Cleaner");
            _navButtons[5].Text = "   🚀 " + Loc.Get("Nav_Startup");
            _navButtons[6].Text = "   ⚙️ " + Loc.Get("Nav_Settings");
        }
    }
}

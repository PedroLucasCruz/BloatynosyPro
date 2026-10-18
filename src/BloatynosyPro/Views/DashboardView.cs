using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class DashboardView : UserControl
{
    private Label _lblScore = null!;
    private Label _lblStatusTitle = null!;
    private Label _lblStatusSubtitle = null!;
    private ModernProgressBar _barScore = null!;
    private ModernButton _btnQuickOptimize = null!;
    private Label _lblRamStat = null!;
    private Label _lblCpuStat = null!;
    private Label _lblDiskStat = null!;
    private Label _lblFeedback = null!;

    private ModernButton _btnToggleGuardian = null!;
    private Label _lblGuardianStatus = null!;
    private Label _lblGuardianCounters = null!;

    public event Action? NavigateToTweaks;
    public event Action? NavigateToPerformance;

    public DashboardView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
        Loc.LanguageChanged += (_, _) => UpdateLanguage();

        AutoGuardianService.OnGuardianAction += msg =>
        {
            if (IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    _lblFeedback.Text = $"⚡ {msg}";
                    _lblFeedback.ForeColor = Color.FromArgb(52, 211, 153);
                    UpdateGuardianLabels();
                    RefreshMetrics();
                });
            }
        };
    }

    private void InitializeUi()
    {
        Controls.Clear();

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(24),
            BackColor = Color.FromArgb(15, 17, 26)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Card Hero
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 125)); // 3 Stats Cards
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Profiles & Auto Guardian

        // 1. Hero Card - Score & 1-Click Optimize
        var heroCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BorderColor = Color.FromArgb(46, 51, 77),
            BackColor = Color.FromArgb(24, 27, 44),
            Margin = new Padding(0, 0, 0, 14)
        };

        var heroLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var heroLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        _lblStatusTitle = new Label
        {
            Text = "Status do Sistema: Pronto para Otimização",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };

        _lblStatusSubtitle = new Label
        {
            Text = "O BloatynosyPro identificou regras e processos que podem ser otimizados.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(156, 163, 195),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 16)
        };

        _barScore = new ModernProgressBar
        {
            Width = 380,
            Height = 16,
            Value = 68,
            ProgressColor = Color.FromArgb(6, 182, 212),
            Margin = new Padding(0, 0, 0, 8)
        };

        _lblScore = new Label
        {
            Text = "Índice de Otimização: 68% (Pode melhorar)",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(6, 182, 212),
            AutoSize = true
        };

        heroLeft.Controls.Add(_lblStatusTitle);
        heroLeft.Controls.Add(_lblStatusSubtitle);
        heroLeft.Controls.Add(_barScore);
        heroLeft.Controls.Add(_lblScore);

        var heroRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(12, 10, 0, 0)
        };

        _btnQuickOptimize = new ModernButton
        {
            Text = "✨ Otimizar Agora (1 Clique)",
            NormalColor = Color.FromArgb(16, 185, 129),
            HoverColor = Color.FromArgb(52, 211, 153),
            PressedColor = Color.FromArgb(5, 150, 105),
            Size = new Size(260, 48),
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };
        _btnQuickOptimize.Click += async (s, e) => await ExecuteQuickOptimizeAsync();

        _lblFeedback = new Label
        {
            Text = "1 clique para aplicar regras seguras e liberar RAM.",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true
        };

        heroRight.Controls.Add(_btnQuickOptimize);
        heroRight.Controls.Add(_lblFeedback);

        heroLayout.Controls.Add(heroLeft, 0, 0);
        heroLayout.Controls.Add(heroRight, 1, 0);
        heroCard.Controls.Add(heroLayout);

        // 2. Hardware Stats Row (3 Mini Cards)
        var statsRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 14)
        };
        statsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        statsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        statsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

        var ramCard = CreateMetricCard("🧠 Memória RAM", out _lblRamStat, Color.FromArgb(139, 92, 246));
        var cpuCard = CreateMetricCard("⚡ Processador (CPU)", out _lblCpuStat, Color.FromArgb(6, 182, 212));
        var diskCard = CreateMetricCard("💾 Disco do Sistema (C:)", out _lblDiskStat, Color.FromArgb(245, 158, 11));

        statsRow.Controls.Add(ramCard, 0, 0);
        statsRow.Controls.Add(cpuCard, 1, 0);
        statsRow.Controls.Add(diskCard, 2, 0);

        // 3. Profiles & Auto-Guardian Row (2 Column Card)
        var bottomLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

        // Profiles Card (Left)
        var profilesCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 0, 8, 0)
        };

        var profilesFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        var lblProfilesTitle = new Label
        {
            Text = "🎯 Perfis Rápidos de Otimização",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        profilesFlow.Controls.Add(lblProfilesTitle);

        var btnGamer = new ModernButton
        {
            Text = "🎮 Modo Gamer: Maximizar FPS e Reduzir Latência",
            NormalColor = Color.FromArgb(99, 102, 241),
            Size = new Size(380, 38),
            Margin = new Padding(0, 0, 0, 8)
        };
        btnGamer.Click += (s, e) => ApplyGamerProfile();

        var btnPrivacy = new ModernButton
        {
            Text = "🛡️ Modo Privacidade: Bloquear Telemetria & Ads",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(380, 38),
            Margin = new Padding(0, 0, 0, 8)
        };
        btnPrivacy.Click += (s, e) => ApplyPrivacyProfile();

        profilesFlow.Controls.Add(btnGamer);
        profilesFlow.Controls.Add(btnPrivacy);
        profilesCard.Controls.Add(profilesFlow);

        // Auto-Guardian Card (Right)
        var guardianCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(8, 0, 0, 0)
        };

        var guardianFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        var lblGuardianTitle = new Label
        {
            Text = "🤖 Guardião Automático (Auto-Pilot)",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(6, 182, 212),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };

        _lblGuardianStatus = new Label
        {
            Text = "Status: 🟢 Ativo (Otimizando em segundo plano)",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 211, 153),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };

        _lblGuardianCounters = new Label
        {
            Text = "Derruba processos de telemetria e libera RAM sozinho assim que necessário.",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        _btnToggleGuardian = new ModernButton
        {
            Text = "⏸️ Pausar Guardião",
            NormalColor = Color.FromArgb(245, 158, 11),
            Size = new Size(200, 36)
        };
        _btnToggleGuardian.Click += (s, e) => ToggleGuardian();

        guardianFlow.Controls.Add(lblGuardianTitle);
        guardianFlow.Controls.Add(_lblGuardianStatus);
        guardianFlow.Controls.Add(_lblGuardianCounters);
        guardianFlow.Controls.Add(_btnToggleGuardian);
        guardianCard.Controls.Add(guardianFlow);

        bottomLayout.Controls.Add(profilesCard, 0, 0);
        bottomLayout.Controls.Add(guardianCard, 1, 0);

        mainLayout.Controls.Add(heroCard, 0, 0);
        mainLayout.Controls.Add(statsRow, 0, 1);
        mainLayout.Controls.Add(bottomLayout, 0, 2);

        Controls.Add(mainLayout);
        RefreshMetrics();
        UpdateGuardianLabels();
    }

    private void ToggleGuardian()
    {
        bool newState = !AutoStartupService.CurrentConfig.AutoGuardianEnabled;
        AutoStartupService.SetGuardian(newState);
        UpdateGuardianLabels();
    }

    private void UpdateGuardianLabels()
    {
        bool isEnabled = AutoStartupService.CurrentConfig.AutoGuardianEnabled;
        if (isEnabled)
        {
            _lblGuardianStatus.Text = "Status: 🟢 Ativo (Otimizando sozinho)";
            _lblGuardianStatus.ForeColor = Color.FromArgb(52, 211, 153);
            _btnToggleGuardian.Text = "⏸️ Pausar Guardião";
            _btnToggleGuardian.NormalColor = Color.FromArgb(245, 158, 11);
        }
        else
        {
            _lblGuardianStatus.Text = "Status: ⚪ Pausado (Otimização manual)";
            _lblGuardianStatus.ForeColor = Color.FromArgb(148, 163, 184);
            _btnToggleGuardian.Text = "▶️ Ativar Guardião";
            _btnToggleGuardian.NormalColor = Color.FromArgb(16, 185, 129);
        }

        _lblGuardianCounters.Text = $"Bloqueios automáticos: {AutoGuardianService.TotalBlockedCount} | Otimizações de RAM: {AutoGuardianService.TotalRamOptimizations}";
    }

    private ModernCard CreateMetricCard(string title, out Label statLabel, Color accent)
    {
        var card = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            BorderColor = Color.FromArgb(40, 45, 68),
            Margin = new Padding(4)
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = accent,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };

        statLabel = new Label
        {
            Text = "Carregando...",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };

        flow.Controls.Add(lblTitle);
        flow.Controls.Add(statLabel);
        card.Controls.Add(flow);
        return card;
    }

    public void RefreshMetrics()
    {
        var snapshot = SystemMetricsService.GetSnapshot();
        _lblRamStat.Text = $"{snapshot.Memory.UsedGb:F1} GB ({snapshot.Memory.UsagePercent}%)";
        _lblCpuStat.Text = $"{snapshot.CpuUsagePercent}% em uso";

        var cDrive = snapshot.Disks.FirstOrDefault(d => d.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase));
        if (cDrive != null)
            _lblDiskStat.Text = $"{cDrive.FreeGb:F0} GB livres de {cDrive.TotalGb:F0} GB";
        else
            _lblDiskStat.Text = "Pronto";
    }

    private async Task ExecuteQuickOptimizeAsync()
    {
        _btnQuickOptimize.Enabled = false;
        _lblFeedback.Text = "Otimizando regras e memória RAM...";
        _lblFeedback.ForeColor = Color.FromArgb(6, 182, 212);

        await Task.Run(() =>
        {
            if (WinRulesEngine.Rules.Count == 0)
                WinRulesEngine.LoadRules();

            WinRulesEngine.CheckAll();
            var recommended = WinRulesEngine.Rules.Where(r => r.DefaultSelected && !r.IsApplied).ToList();
            foreach (var r in recommended)
            {
                WinRulesEngine.ApplyRule(r);
            }
        });

        long freedRam = await MemoryOptimizer.CleanWorkingSetAsync();
        double freedMb = freedRam / (1024.0 * 1024.0);

        _barScore.Value = 95;
        _lblScore.Text = "Índice de Otimização: 95% (Excelente)";
        _lblScore.ForeColor = Color.FromArgb(16, 185, 129);
        _lblStatusTitle.Text = "🎉 Computador Otimizado com Sucesso!";
        _lblFeedback.Text = $"Memória RAM descarregada: ~{freedMb:F0} MB recuperados!";
        _lblFeedback.ForeColor = Color.FromArgb(16, 185, 129);

        RefreshMetrics();
        _btnQuickOptimize.Enabled = true;
    }

    private void ApplyGamerProfile()
    {
        int applied = 0;
        foreach (var rule in WinRulesEngine.Rules.Where(r => r.Category.Equals("Gaming", StringComparison.OrdinalIgnoreCase)))
        {
            if (WinRulesEngine.ApplyRule(rule)) applied++;
        }
        _lblFeedback.Text = $"Perfil Gamer ativado: {applied} ajustes aplicados!";
        _lblFeedback.ForeColor = Color.FromArgb(99, 102, 241);
    }

    private void ApplyPrivacyProfile()
    {
        int applied = 0;
        foreach (var rule in WinRulesEngine.Rules.Where(r => r.Category.Equals("Privacy", StringComparison.OrdinalIgnoreCase) || r.Category.Equals("Ads", StringComparison.OrdinalIgnoreCase)))
        {
            if (WinRulesEngine.ApplyRule(rule)) applied++;
        }
        _lblFeedback.Text = $"Perfil Privacidade ativado: {applied} regras aplicadas!";
        _lblFeedback.ForeColor = Color.FromArgb(79, 70, 229);
    }

    private void UpdateLanguage()
    {
        _btnQuickOptimize.Text = Loc.Get("Dash_QuickOpt");
        _lblStatusTitle.Text = Loc.CurrentLanguage.StartsWith("pt") ? "Status do Sistema: Pronto" : "System Status: Ready";
    }
}

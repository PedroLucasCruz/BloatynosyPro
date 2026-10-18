using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class SettingsView : UserControl
{
    private ComboBox _cbLanguage = null!;
    private CheckBox _chkAutoStart = null!;
    private ComboBox _cbPreset = null!;
    private Label _lblOsInfo = null!;

    public SettingsView()
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

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        var lblTitle = new Label
        {
            Text = "⚙️ Configurações do Sistema & Inicialização",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };
        flow.Controls.Add(lblTitle);

        // AutoStart & Preset Card
        var cardAutoStart = new ModernCard
        {
            Width = 560,
            Height = 160,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 0, 0, 16)
        };

        var flowAuto = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        flowAuto.Controls.Add(new Label { Text = "🚀 Inicialização Automática com o Windows", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(6, 182, 212), AutoSize = true, Margin = new Padding(0, 0, 0, 8) });

        _chkAutoStart = new CheckBox
        {
            Text = "Disparar otimização do computador automaticamente ao ligar o Windows",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.White,
            AutoSize = true,
            Checked = AutoStartupService.CurrentConfig.AutoStartWithWindows,
            Margin = new Padding(0, 0, 0, 10)
        };
        _chkAutoStart.CheckedChanged += (s, e) =>
        {
            AutoStartupService.SetAutoStart(_chkAutoStart.Checked, _cbPreset.SelectedItem?.ToString());
        };

        var presetPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0) };
        presetPanel.Controls.Add(new Label { Text = "Preset Automático:", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(203, 213, 225), AutoSize = true, Margin = new Padding(0, 4, 10, 0) });

        _cbPreset = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(28, 32, 52),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f),
            Width = 220
        };
        _cbPreset.Items.Add("Modo Equilibrado (Padrão)");
        _cbPreset.Items.Add("Modo Gamer (Máximo FPS)");
        _cbPreset.Items.Add("Modo Privacidade Total");

        string currentPreset = AutoStartupService.CurrentConfig.StartupPreset;
        _cbPreset.SelectedIndex = currentPreset.Contains("Gamer", StringComparison.OrdinalIgnoreCase) ? 1 :
                                 (currentPreset.Contains("Privacy", StringComparison.OrdinalIgnoreCase) || currentPreset.Contains("Privacidade", StringComparison.OrdinalIgnoreCase)) ? 2 : 0;

        _cbPreset.SelectedIndexChanged += (s, e) =>
        {
            AutoStartupService.SetAutoStart(_chkAutoStart.Checked, _cbPreset.SelectedItem?.ToString());
        };

        presetPanel.Controls.Add(_cbPreset);
        flowAuto.Controls.Add(_chkAutoStart);
        flowAuto.Controls.Add(presetPanel);
        cardAutoStart.Controls.Add(flowAuto);
        flow.Controls.Add(cardAutoStart);

        // Auto-Guardian Card
        var cardGuardian = new ModernCard
        {
            Width = 560,
            Height = 295,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 0, 0, 16)
        };

        var flowGuardian = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        flowGuardian.Controls.Add(new Label { Text = "🤖 Guardião Automático (Auto-Pilot Inteligente)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(52, 211, 153), AutoSize = true, Margin = new Padding(0, 0, 0, 8) });

        var chkGuardianMain = new CheckBox
        {
            Text = "Ativar Guardião em segundo plano (Otimização contínua sem intervenção)",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Checked = AutoStartupService.CurrentConfig.AutoGuardianEnabled,
            Margin = new Padding(0, 0, 0, 6)
        };
        chkGuardianMain.CheckedChanged += (s, e) =>
        {
            AutoStartupService.SetGuardian(chkGuardianMain.Checked);
        };

        var chkAutoSvc = new CheckBox
        {
            Text = "Derrubar serviços e hospedeiros svchost indesejados automaticamente",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(203, 213, 225),
            AutoSize = true,
            Checked = AutoStartupService.CurrentConfig.AutoServiceBlockEnabled,
            Margin = new Padding(16, 0, 0, 4)
        };
        chkAutoSvc.CheckedChanged += (s, e) =>
        {
            AutoStartupService.CurrentConfig.AutoServiceBlockEnabled = chkAutoSvc.Checked;
            AutoStartupService.SaveConfig();
        };

        var chkAutoRam = new CheckBox
        {
            Text = "Otimizar memória RAM periodicamente (igual ao botão manual) e por uso",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(203, 213, 225),
            AutoSize = true,
            Checked = AutoStartupService.CurrentConfig.AutoRamCleanEnabled,
            Margin = new Padding(16, 0, 0, 4)
        };
        chkAutoRam.CheckedChanged += (s, e) =>
        {
            AutoStartupService.CurrentConfig.AutoRamCleanEnabled = chkAutoRam.Checked;
            AutoStartupService.SaveConfig();
        };

        var chkAutoDisk = new CheckBox
        {
            Text = "Limpar arquivos temporários padrão automaticamente em segundo plano",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(203, 213, 225),
            AutoSize = true,
            Checked = AutoStartupService.CurrentConfig.AutoDiskCleanEnabled,
            Margin = new Padding(16, 0, 0, 4)
        };
        chkAutoDisk.CheckedChanged += (s, e) =>
        {
            AutoStartupService.CurrentConfig.AutoDiskCleanEnabled = chkAutoDisk.Checked;
            AutoStartupService.SaveConfig();
        };

        var chkAutoIntegrity = new CheckBox
        {
            Text = "Verificar e reparar integridade do Windows automaticamente (DISM/SFC)",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(203, 213, 225),
            AutoSize = true,
            Checked = AutoStartupService.CurrentConfig.AutoIntegrityCheckEnabled,
            Margin = new Padding(16, 0, 0, 4)
        };
        chkAutoIntegrity.CheckedChanged += (s, e) =>
        {
            AutoStartupService.CurrentConfig.AutoIntegrityCheckEnabled = chkAutoIntegrity.Checked;
            AutoStartupService.SaveConfig();
        };

        var btnManageBlacklist = new ModernButton
        {
            Text = "📋 Gerenciar Lista Negra (Ver / Remover Processos e Serviços)",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(520, 34),
            Margin = new Padding(16, 8, 0, 0)
        };
        btnManageBlacklist.Click += (s, e) =>
        {
            using var dlg = new Forms.BlacklistManagerForm();
            dlg.ShowDialog(this);
        };

        flowGuardian.Controls.Add(chkGuardianMain);
        flowGuardian.Controls.Add(chkAutoSvc);
        flowGuardian.Controls.Add(chkAutoRam);
        flowGuardian.Controls.Add(chkAutoDisk);
        flowGuardian.Controls.Add(chkAutoIntegrity);
        flowGuardian.Controls.Add(btnManageBlacklist);
        cardGuardian.Controls.Add(flowGuardian);
        flow.Controls.Add(cardGuardian);

        // Language Card
        var cardLang = new ModernCard
        {
            Width = 560,
            Height = 110,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 0, 0, 16)
        };

        var flowLang = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        flowLang.Controls.Add(new Label { Text = "🌐 Idioma da Aplicação (Language)", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 8) });

        _cbLanguage = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(28, 32, 52),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f),
            Width = 260
        };
        _cbLanguage.Items.Add("Português (Brasil)");
        _cbLanguage.Items.Add("English (US)");
        _cbLanguage.SelectedIndex = Loc.CurrentLanguage.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
        _cbLanguage.SelectedIndexChanged += (s, e) =>
        {
            string newLang = _cbLanguage.SelectedIndex == 0 ? "pt-BR" : "en";
            Loc.SetLanguage(newLang);
        };

        flowLang.Controls.Add(_cbLanguage);
        cardLang.Controls.Add(flowLang);
        flow.Controls.Add(cardLang);

        // System Info Card
        var cardInfo = new ModernCard
        {
            Width = 560,
            Height = 180,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 0, 0, 16)
        };

        var flowInfo = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        flowInfo.Controls.Add(new Label { Text = "💻 Informações do Sistema", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 8) });

        _lblOsInfo = new Label
        {
            Text = $"Sistema Operacional: {Environment.OSVersion}\n" +
                   $"Arquitetura: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}\n" +
                   $"Processadores Lógicos: {Environment.ProcessorCount}\n" +
                   $"Runtime: .NET 9.0 (C# 13)\n" +
                   $"Nome da Máquina: {Environment.MachineName}",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(203, 213, 225),
            AutoSize = true
        };

        flowInfo.Controls.Add(_lblOsInfo);
        cardInfo.Controls.Add(flowInfo);
        flow.Controls.Add(cardInfo);

        // About Card
        var cardAbout = new ModernCard
        {
            Width = 560,
            Height = 140,
            BackColor = Color.FromArgb(20, 23, 36)
        };

        var flowAbout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        flowAbout.Controls.Add(new Label { Text = "⭐ Sobre o BloatynosyPro", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = true, Margin = new Padding(0, 0, 0, 6) });
        flowAbout.Controls.Add(new Label
        {
            Text = "Construído sobre o motor do Bloatynosy com interface Windows Forms moderna, conceito de UX Designer, monitoramento de hardware em tempo real e otimização inteligente em 1 clique.\nDesenvolvido por PedroLucasCruz.",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true
        });

        cardAbout.Controls.Add(flowAbout);
        flow.Controls.Add(cardAbout);

        mainPanel.Controls.Add(flow);
        Controls.Add(mainPanel);
    }
}

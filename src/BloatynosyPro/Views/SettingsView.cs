using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class SettingsView : UserControl
{
    private ComboBox _cbLanguage = null!;
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
            Text = "⚙️ Configurações & Sobre",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };
        flow.Controls.Add(lblTitle);

        // Language Card
        var cardLang = new ModernCard
        {
            Width = 520,
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
            Width = 520,
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
            Width = 520,
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

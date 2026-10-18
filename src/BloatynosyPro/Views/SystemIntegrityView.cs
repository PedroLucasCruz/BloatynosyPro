using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class SystemIntegrityView : UserControl
{
    private ModernButton _btnStartRepair = null!;
    private ModernProgressBar _progressBar = null!;
    private Label _lblStatusTitle = null!;
    private Label _lblStepDesc = null!;
    private TextBox _txtLog = null!;

    private Label _lblDismStatus = null!;
    private Label _lblSfcStatus = null!;
    private Label _lblChkdskStatus = null!;

    public SystemIntegrityView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
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
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // Hero Card
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 105)); // 3 Indicator Cards
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Console Log Card

        // 1. Hero Card
        var heroCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(24, 27, 44),
            Margin = new Padding(0, 0, 0, 14)
        };

        var heroLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        var heroLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        _lblStatusTitle = new Label
        {
            Text = "🛡️ Integridade & Reparo Automático do Windows",
            Font = new Font("Segoe UI", 14.5f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };

        _lblStepDesc = new Label
        {
            Text = "Em apenas 1 clique, verifica e repara componentes do Windows (DISM), arquivos corrompidos (SFC) e sistema de arquivos do HD/SSD (CHKDSK).",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 14)
        };

        _progressBar = new ModernProgressBar
        {
            Width = 440,
            Height = 16,
            Value = 0,
            ProgressColor = Color.FromArgb(16, 185, 129),
            Margin = new Padding(0, 0, 0, 6)
        };

        heroLeft.Controls.Add(_lblStatusTitle);
        heroLeft.Controls.Add(_lblStepDesc);
        heroLeft.Controls.Add(_progressBar);

        var heroRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10, 12, 0, 0)
        };

        _btnStartRepair = new ModernButton
        {
            Text = "🛡️ Iniciar Reparo (1 Clique)",
            NormalColor = Color.FromArgb(16, 185, 129),
            HoverColor = Color.FromArgb(52, 211, 153),
            PressedColor = Color.FromArgb(5, 150, 105),
            Size = new Size(240, 48),
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };
        _btnStartRepair.Click += async (s, e) => await StartIntegrityProcessAsync();

        heroRight.Controls.Add(_btnStartRepair);

        heroLayout.Controls.Add(heroLeft, 0, 0);
        heroLayout.Controls.Add(heroRight, 1, 0);
        heroCard.Controls.Add(heroLayout);

        // 2. 3 Indicator Cards
        var cardsRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 14)
        };
        cardsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        cardsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        cardsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

        var cardDism = CreateIndicatorCard("1. Componentes do Windows", "DISM RestoreHealth", out _lblDismStatus, Color.FromArgb(6, 182, 212));
        var cardSfc = CreateIndicatorCard("2. Arquivos de Sistema", "SFC Scannow", out _lblSfcStatus, Color.FromArgb(139, 92, 246));
        var cardChk = CreateIndicatorCard("3. Sistema de Arquivos SSD/HD", "CHKDSK Online Scan", out _lblChkdskStatus, Color.FromArgb(245, 158, 11));

        cardsRow.Controls.Add(cardDism, 0, 0);
        cardsRow.Controls.Add(cardSfc, 1, 0);
        cardsRow.Controls.Add(cardChk, 2, 0);

        // 3. Log Console Card
        var logCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36)
        };

        var logPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
        var lblLogTitle = new Label
        {
            Text = "📋 Log de Execução e Diagnóstico em Tempo Real",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Top,
            Height = 28
        };

        _txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(13, 15, 23),
            ForeColor = Color.FromArgb(52, 211, 153),
            Font = new Font("Consolas", 9.5f),
            BorderStyle = BorderStyle.None,
            Text = "Pronto para iniciar. Clique em 'Iniciar Reparo' para verificar o sistema.\r\n"
        };

        logPanel.Controls.Add(_txtLog);
        logPanel.Controls.Add(lblLogTitle);
        logCard.Controls.Add(logPanel);

        mainLayout.Controls.Add(heroCard, 0, 0);
        mainLayout.Controls.Add(cardsRow, 0, 1);
        mainLayout.Controls.Add(logCard, 0, 2);

        Controls.Add(mainLayout);
    }

    private ModernCard CreateIndicatorCard(string stepTitle, string techTitle, out Label statusLabel, Color accent)
    {
        var card = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            BorderColor = Color.FromArgb(40, 45, 68),
            Margin = new Padding(4, 0, 4, 0)
        };

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };

        var lblTitle = new Label
        {
            Text = stepTitle,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = accent,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2)
        };

        var lblTech = new Label
        {
            Text = techTitle,
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };

        statusLabel = new Label
        {
            Text = "⚪ Aguardando",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };

        flow.Controls.Add(lblTitle);
        flow.Controls.Add(lblTech);
        flow.Controls.Add(statusLabel);
        card.Controls.Add(flow);
        return card;
    }

    private async Task StartIntegrityProcessAsync()
    {
        _btnStartRepair.Enabled = false;
        _progressBar.Value = 5;
        _txtLog.Clear();
        _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Iniciando Verificação de Integridade Completa...\r\n");

        _lblDismStatus.Text = "⏳ Analisando...";
        _lblDismStatus.ForeColor = Color.FromArgb(6, 182, 212);
        _lblSfcStatus.Text = "⚪ Aguardando";
        _lblSfcStatus.ForeColor = Color.White;
        _lblChkdskStatus.Text = "⚪ Aguardando";
        _lblChkdskStatus.ForeColor = Color.White;

        var progress = new Progress<IntegrityProgressUpdate>(u =>
        {
            _progressBar.Value = u.Percentage;
            _lblStepDesc.Text = u.Message;

            if (u.StepIndex == 1)
            {
                _lblDismStatus.Text = "🔄 Em Execução...";
            }
            else if (u.StepIndex == 2)
            {
                _lblDismStatus.Text = "✅ Concluído";
                _lblDismStatus.ForeColor = Color.FromArgb(16, 185, 129);
                _lblSfcStatus.Text = "🔄 Verificando SFC...";
                _lblSfcStatus.ForeColor = Color.FromArgb(139, 92, 246);
            }
            else if (u.StepIndex == 3)
            {
                _lblSfcStatus.Text = "✅ Concluído";
                _lblSfcStatus.ForeColor = Color.FromArgb(16, 185, 129);
                _lblChkdskStatus.Text = "🔄 Analisando Disco...";
                _lblChkdskStatus.ForeColor = Color.FromArgb(245, 158, 11);
            }

            _txtLog.AppendText(u.LogLine + "\r\n");
            _txtLog.SelectionStart = _txtLog.TextLength;
            _txtLog.ScrollToCaret();
        });

        var result = await SystemIntegrityService.RunFullIntegrityCheckAsync(progress);

        _lblDismStatus.Text = result.DismSuccess ? "✅ Íntegro" : "⚠️ Concluído";
        _lblSfcStatus.Text = result.SfcSuccess ? "✅ Íntegro" : "⚠️ Concluído";
        _lblChkdskStatus.Text = result.ChkdskSuccess ? "✅ Íntegro" : "⚠️ Concluído";

        _lblStatusTitle.Text = "🎉 Sistema Operacional Verificado e Reparado!";
        _txtLog.AppendText("\r\n======================================================\r\n");
        _txtLog.AppendText("RELATÓRIO FINAL:\r\n" + result.Summary + "\r\n");
        _txtLog.AppendText("======================================================\r\n");

        _btnStartRepair.Enabled = true;
    }
}

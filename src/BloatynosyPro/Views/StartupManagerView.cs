using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class StartupManagerView : UserControl
{
    private ListView _lvStartup = null!;
    private ModernButton _btnRefresh = null!;
    private ModernButton _btnDisable = null!;
    private Label _lblStatus = null!;

    public StartupManagerView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
        LoadStartupItems();
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
            Height = 90,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "🚀 Gerenciador de Inicialização (Startup Apps)",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Desative programas que iniciam com o Windows para acelerar o tempo de boot e economizar RAM ao ligar.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(0, 30)
        };

        var toolPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _btnRefresh = new ModernButton
        {
            Text = "🔄 Atualizar",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(130, 36),
            Margin = new Padding(0, 0, 8, 0)
        };
        _btnRefresh.Click += (s, e) => LoadStartupItems();

        _btnDisable = new ModernButton
        {
            Text = "🚫 Desativar da Inicialização",
            NormalColor = Color.FromArgb(239, 68, 68),
            Size = new Size(220, 36),
            Margin = new Padding(0, 0, 8, 0)
        };
        _btnDisable.Click += (s, e) => DisableSelectedItem();

        toolPanel.Controls.Add(_btnRefresh);
        toolPanel.Controls.Add(_btnDisable);

        topPanel.Controls.Add(lblTitle);
        topPanel.Controls.Add(lblSubtitle);
        topPanel.Controls.Add(toolPanel);

        var cardList = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 10, 0, 0)
        };

        _lvStartup = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvStartup.Columns.Add("Nome da Aplicação", 240);
        _lvStartup.Columns.Add("Origem no Registro / Pasta", 160);
        _lvStartup.Columns.Add("Comando Executado", 450);

        cardList.Controls.Add(_lvStartup);

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

    private void LoadStartupItems()
    {
        var items = StartupService.GetStartupItems();
        _lvStartup.BeginUpdate();
        _lvStartup.Items.Clear();

        foreach (var it in items)
        {
            var lvi = new ListViewItem(it.Name) { Tag = it };
            lvi.SubItems.Add(it.Location);
            lvi.SubItems.Add(it.Command);
            _lvStartup.Items.Add(lvi);
        }
        _lvStartup.EndUpdate();
        _lblStatus.Text = $"{items.Count} aplicativos encontrados na inicialização.";
    }

    private void DisableSelectedItem()
    {
        if (_lvStartup.SelectedItems.Count == 0)
        {
            MessageBox.Show("Selecione um programa para desativar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var item = _lvStartup.SelectedItems[0].Tag as StartupItem;
        if (item == null) return;

        var confirm = MessageBox.Show(
            $"Deseja remover '{item.Name}' da inicialização automática?",
            "Confirmar Desativação",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm == DialogResult.Yes)
        {
            StartupService.RemoveStartupItem(item);
            LoadStartupItems();
        }
    }
}

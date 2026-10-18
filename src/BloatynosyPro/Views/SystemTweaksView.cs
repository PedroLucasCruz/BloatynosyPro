using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class SystemTweaksView : UserControl
{
    private ComboBox _cbCategories = null!;
    private ListView _lvRules = null!;
    private ModernButton _btnScan = null!;
    private ModernButton _btnApply = null!;
    private ModernButton _btnRestore = null!;
    private ModernButton _btnSelectRecommended = null!;
    private Label _lblStatus = null!;

    public SystemTweaksView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
        LoadRulesData();
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

        // Header Top
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "🛡️ Otimizações do Windows (WinRules Engine)",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Controle total sobre centenas de políticas, telemetria, anúncios e recursos desnecessários do Windows.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(0, 30)
        };

        // Toolbar
        var toolPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _cbCategories = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f),
            Width = 190,
            Height = 36,
            Margin = new Padding(0, 4, 10, 0)
        };
        _cbCategories.SelectedIndexChanged += (s, e) => FilterRules();

        _btnScan = new ModernButton
        {
            Text = "🔍 Verificar",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(110, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnScan.Click += (s, e) => ScanRules();

        _btnSelectRecommended = new ModernButton
        {
            Text = "✨ Recomendadas",
            NormalColor = Color.FromArgb(59, 130, 246),
            Size = new Size(140, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnSelectRecommended.Click += (s, e) => SelectRecommended();

        _btnApply = new ModernButton
        {
            Text = "⚡ Aplicar",
            NormalColor = Color.FromArgb(16, 185, 129),
            Size = new Size(110, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnApply.Click += async (s, e) => await ApplySelectedAsync();

        _btnRestore = new ModernButton
        {
            Text = "↩ Restaurar",
            NormalColor = Color.FromArgb(239, 68, 68),
            Size = new Size(110, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnRestore.Click += async (s, e) => await RestoreSelectedAsync();

        toolPanel.Controls.Add(_cbCategories);
        toolPanel.Controls.Add(_btnScan);
        toolPanel.Controls.Add(_btnSelectRecommended);
        toolPanel.Controls.Add(_btnApply);
        toolPanel.Controls.Add(_btnRestore);

        topPanel.Controls.Add(lblTitle);
        topPanel.Controls.Add(lblSubtitle);
        topPanel.Controls.Add(toolPanel);

        // Center: List
        var cardList = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 10, 0, 0)
        };

        _lvRules = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvRules.Columns.Add("Regra de Otimização", 320);
        _lvRules.Columns.Add("Categoria", 110);
        _lvRules.Columns.Add("Status Atual", 120);
        _lvRules.Columns.Add("Descrição", 400);

        cardList.Controls.Add(_lvRules);

        // Bottom status
        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Text = "Carregando catálogo de regras..."
        };

        mainPanel.Controls.Add(cardList);
        mainPanel.Controls.Add(topPanel);
        mainPanel.Controls.Add(_lblStatus);

        Controls.Add(mainPanel);
    }

    private void LoadRulesData()
    {
        if (WinRulesEngine.Rules.Count == 0)
            WinRulesEngine.LoadRules();

        var categories = WinRulesEngine.Rules.Select(r => r.Category).Distinct().OrderBy(c => c).ToList();
        _cbCategories.Items.Clear();
        _cbCategories.Items.Add("Todas as Categorias");
        foreach (var c in categories) _cbCategories.Items.Add(c);
        _cbCategories.SelectedIndex = 0;

        ScanRules();
    }

    private void ScanRules()
    {
        _lblStatus.Text = "Analisando estado das chaves do registro...";
        WinRulesEngine.CheckAll();
        FilterRules();
        int appliedCount = WinRulesEngine.Rules.Count(r => r.IsApplied);
        _lblStatus.Text = $"Total: {WinRulesEngine.Rules.Count} regras carregadas ({appliedCount} já aplicadas no seu Windows).";
    }

    private void FilterRules()
    {
        string selectedCat = _cbCategories.SelectedItem?.ToString() ?? "Todas as Categorias";
        _lvRules.BeginUpdate();
        _lvRules.Items.Clear();

        var filtered = selectedCat.Equals("Todas as Categorias", StringComparison.OrdinalIgnoreCase)
            ? WinRulesEngine.Rules
            : WinRulesEngine.Rules.Where(r => r.Category.Equals(selectedCat, StringComparison.OrdinalIgnoreCase));

        foreach (var r in filtered)
        {
            var lvi = new ListViewItem(r.Name)
            {
                Checked = r.Selected,
                Tag = r
            };
            lvi.SubItems.Add(r.Category);
            lvi.SubItems.Add(r.IsApplied ? "✅ Aplicada" : "⚪ Padrão");
            lvi.SubItems.Add(r.Description);

            if (r.IsApplied)
                lvi.ForeColor = Color.FromArgb(52, 211, 153);

            _lvRules.Items.Add(lvi);
        }
        _lvRules.EndUpdate();
    }

    private void SelectRecommended()
    {
        foreach (ListViewItem item in _lvRules.Items)
        {
            if (item.Tag is WinRule r)
            {
                item.Checked = r.DefaultSelected && !r.IsApplied;
            }
        }
    }

    private async Task ApplySelectedAsync()
    {
        _btnApply.Enabled = false;
        _lblStatus.Text = "Aplicando otimizações selecionadas no registro...";

        int count = 0;
        await Task.Run(() =>
        {
            foreach (ListViewItem item in _lvRules.Items)
            {
                if (item.Checked && item.Tag is WinRule r)
                {
                    if (WinRulesEngine.ApplyRule(r)) count++;
                }
            }
        });

        _lblStatus.Text = $"🎉 {count} regras aplicadas com sucesso no Windows!";
        ScanRules();
        _btnApply.Enabled = true;
    }

    private async Task RestoreSelectedAsync()
    {
        _btnRestore.Enabled = false;
        _lblStatus.Text = "Restaurando configurações padrão do Windows...";

        int count = 0;
        await Task.Run(() =>
        {
            foreach (ListViewItem item in _lvRules.Items)
            {
                if (item.Checked && item.Tag is WinRule r)
                {
                    if (WinRulesEngine.RestoreRule(r)) count++;
                }
            }
        });

        _lblStatus.Text = $"↩ {count} regras restauradas para os padrões do sistema.";
        ScanRules();
        _btnRestore.Enabled = true;
    }
}

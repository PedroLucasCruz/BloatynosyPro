using System.Drawing;
using System.Windows.Forms;
using BloatynosyPro.Controls;
using BloatynosyPro.Core;

namespace BloatynosyPro.Views;

public class ProcessOptimizerView : UserControl
{
    private ListView _lvProcesses = null!;
    private ComboBox _cbFilter = null!;
    private ModernButton _btnScan = null!;
    private ModernButton _btnOptimizeNow = null!;
    private ModernButton _btnKillAndBlock = null!;
    private Label _lblStatus = null!;
    private List<DetailedProcessItem> _items = new();
    private List<SvchostGroupItem> _svchostGroups = new();

    public ProcessOptimizerView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(15, 17, 26);
        Dock = DockStyle.Fill;
        InitializeUi();
        LoadProcesses();
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
            Text = "🔥 Otimizador de Processos & Hospedeiros de Serviços (svchost)",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Analise processos ativos e hospedeiros svchost. Identifique e derrube serviços nativos indesejados bloqueando sua reinicialização permanente.",
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

        _cbFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f),
            Width = 270,
            Height = 36,
            Margin = new Padding(0, 4, 10, 0)
        };
        _cbFilter.Items.Add("⚠️ Apenas Prejudiciais / Seguros");
        _cbFilter.Items.Add("🔎 Inspecionar Hospedeiros svchost");
        _cbFilter.Items.Add("Todos os Processos");
        _cbFilter.SelectedIndex = 0;
        _cbFilter.SelectedIndexChanged += (s, e) => FilterList();

        _btnScan = new ModernButton
        {
            Text = "🔄 Atualizar PIDs",
            NormalColor = Color.FromArgb(79, 70, 229),
            Size = new Size(130, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnScan.Click += (s, e) => LoadProcesses();

        _btnOptimizeNow = new ModernButton
        {
            Text = "⚡ Otimizar Processos (1 Clique)",
            NormalColor = Color.FromArgb(16, 185, 129),
            Size = new Size(220, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnOptimizeNow.Click += async (s, e) => await ExecuteOptimizationAsync();

        _btnKillAndBlock = new ModernButton
        {
            Text = "🚫 Derrubar & Bloquear Serviço",
            NormalColor = Color.FromArgb(239, 68, 68),
            Size = new Size(230, 36),
            Margin = new Padding(0, 2, 8, 0)
        };
        _btnKillAndBlock.Click += async (s, e) => await KillAndBlockSelectedAsync();

        toolPanel.Controls.Add(_cbFilter);
        toolPanel.Controls.Add(_btnScan);
        toolPanel.Controls.Add(_btnOptimizeNow);
        toolPanel.Controls.Add(_btnKillAndBlock);

        topPanel.Controls.Add(lblTitle);
        topPanel.Controls.Add(lblSubtitle);
        topPanel.Controls.Add(toolPanel);

        var cardList = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 23, 36),
            Margin = new Padding(0, 10, 0, 0)
        };

        _lvProcesses = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        SetupDefaultColumns();

        cardList.Controls.Add(_lvProcesses);

        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Text = "Carregando processos..."
        };

        mainPanel.Controls.Add(cardList);
        mainPanel.Controls.Add(topPanel);
        mainPanel.Controls.Add(_lblStatus);

        Controls.Add(mainPanel);
    }

    private void SetupDefaultColumns()
    {
        _lvProcesses.Columns.Clear();
        _lvProcesses.Columns.Add("Processo", 220);
        _lvProcesses.Columns.Add("PID", 80);
        _lvProcesses.Columns.Add("Uso de RAM", 120);
        _lvProcesses.Columns.Add("Classificação de Segurança", 220);
        _lvProcesses.Columns.Add("Descrição / Serviços Hospedados", 400);
    }

    private void LoadProcesses()
    {
        _lblStatus.Text = "Examinando tabela de processos e serviços do Windows...";
        _items = ProcessOptimizerService.GetDetailedProcesses();
        _svchostGroups = SvchostInspectorService.InspectSvchostInstances();

        FilterList();
        int harmful = _items.Count(x => x.Safety == ProcessSafetyLevel.SafeToKill);
        int unwantedHosts = _svchostGroups.Count(x => x.HasUnwantedServices);
        _lblStatus.Text = $"Examinados: {_items.Count} processos ({harmful} prejudiciais seguros) | {_svchostGroups.Count} hospedeiros svchost ({unwantedHosts} com serviços indesejados).";
    }

    private void FilterList()
    {
        _lvProcesses.BeginUpdate();
        _lvProcesses.Items.Clear();

        if (_cbFilter.SelectedIndex == 1) // Modo Svchost Inspector
        {
            _btnKillAndBlock.Visible = true;
            _lvProcesses.Columns.Clear();
            _lvProcesses.Columns.Add("Hospedeiro", 120);
            _lvProcesses.Columns.Add("PID", 80);
            _lvProcesses.Columns.Add("RAM", 100);
            _lvProcesses.Columns.Add("Diagnóstico do Hospedeiro", 230);
            _lvProcesses.Columns.Add("Serviços Nativos no Processo", 450);

            foreach (var h in _svchostGroups)
            {
                var lvi = new ListViewItem("svchost.exe") { Tag = h };
                lvi.SubItems.Add(h.Pid.ToString());
                lvi.SubItems.Add($"{h.MemoryMb:F1} MB");

                string diag = h.HasUnwantedServices ? "⚠️ Contém Serviços Indesejados" : (h.HasCriticalServices ? "🛡️ Serviços do Sistema" : "⚪ Vazio / Ocioso");
                lvi.SubItems.Add(diag);

                string svcNames = string.Join(", ", h.Services.Select(s => s.IsUnwanted ? $"[⚠️ {s.Name}]" : s.Name));
                lvi.SubItems.Add(svcNames);

                if (h.HasUnwantedServices)
                    lvi.ForeColor = Color.FromArgb(248, 113, 113);
                else
                    lvi.ForeColor = Color.FromArgb(203, 213, 225);

                _lvProcesses.Items.Add(lvi);
            }
        }
        else
        {
            SetupDefaultColumns();
            _btnKillAndBlock.Visible = false;

            IEnumerable<DetailedProcessItem> filtered = _cbFilter.SelectedIndex switch
            {
                0 => _items.Where(x => x.Safety == ProcessSafetyLevel.SafeToKill),
                _ => _items
            };

            foreach (var item in filtered)
            {
                var lvi = new ListViewItem(item.Name) { Tag = item };
                lvi.SubItems.Add(item.Pid.ToString());
                lvi.SubItems.Add($"{item.MemoryMb:F1} MB");

                string safetyText = item.Safety switch
                {
                    ProcessSafetyLevel.SafeToKill => "⚠️ Prejudicial / Seguro Finalizar",
                    ProcessSafetyLevel.CriticalSystem => "🛡️ Sistema Crítico (Protegido)",
                    ProcessSafetyLevel.SystemSupport => "🧹 Segundo Plano (Otimizar RAM)",
                    _ => "👤 Aplicativo do Usuário"
                };
                lvi.SubItems.Add(safetyText);
                lvi.SubItems.Add(item.Description);

                if (item.Safety == ProcessSafetyLevel.SafeToKill)
                    lvi.ForeColor = Color.FromArgb(248, 113, 113);
                else if (item.Safety == ProcessSafetyLevel.CriticalSystem)
                    lvi.ForeColor = Color.FromArgb(148, 163, 184);
                else
                    lvi.ForeColor = Color.FromArgb(226, 232, 240);

                _lvProcesses.Items.Add(lvi);
            }
        }

        _lvProcesses.EndUpdate();
    }

    private async Task ExecuteOptimizationAsync()
    {
        _btnOptimizeNow.Enabled = false;
        _lblStatus.Text = "Otimizando processos e finalizando telemetria/atualizadores ociosos...";
        _lblStatus.ForeColor = Color.FromArgb(6, 182, 212);

        var result = await ProcessOptimizerService.OptimizeProcessesAsync();

        _lblStatus.Text = $"🎉 Sucesso! {result.TerminatedCount} processos prejudiciais finalizados e {result.TrimmedCount} processos otimizados. ~{result.FreedMemoryMb:F0} MB recuperados!";
        _lblStatus.ForeColor = Color.FromArgb(16, 185, 129);

        LoadProcesses();
        _btnOptimizeNow.Enabled = true;
    }

    private async Task KillAndBlockSelectedAsync()
    {
        if (_lvProcesses.SelectedItems.Count == 0)
        {
            MessageBox.Show("Selecione um hospedeiro svchost da lista para derrubar e desativar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selected = _lvProcesses.SelectedItems[0].Tag as SvchostGroupItem;
        if (selected == null) return;

        var unwanted = selected.Services.Where(s => s.IsUnwanted).ToList();
        string serviceListStr = unwanted.Count > 0
            ? string.Join(", ", unwanted.Select(u => u.Name))
            : string.Join(", ", selected.Services.Select(s => s.Name));

        var confirm = MessageBox.Show(
            $"Deseja realmente derrubar o processo PID {selected.Pid} e desativar permanentemente o(s) serviço(s) '{serviceListStr}' no Windows para nunca mais iniciarem?",
            "Confirmar Bloqueio Permanente",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (confirm != DialogResult.Yes) return;

        _lblStatus.Text = "Derrubando hospedeiro e desativando serviços no Windows...";
        foreach (var svc in selected.Services)
        {
            await SvchostInspectorService.KillAndDisableServiceAsync(selected.Pid, svc.Name);
        }

        _lblStatus.Text = $"🎉 Hospedeiro PID {selected.Pid} finalizado e serviços desativados com sucesso!";
        LoadProcesses();
    }
}

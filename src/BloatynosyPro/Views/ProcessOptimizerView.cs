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
    private ContextMenuStrip _contextMenu = null!;
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
            Text = "Analise processos e instâncias svchost. Finalize e bloqueie processos e serviços indesejados no Guardião com 1 clique.",
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
            Text = "🚫 Finalizar & Bloquear no Guardião",
            NormalColor = Color.FromArgb(239, 68, 68),
            Size = new Size(260, 36),
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

        // Context Menu para clique direito
        _contextMenu = new ContextMenuStrip();
        var menuKill = new ToolStripMenuItem("🚫 Finalizar e Adicionar ao Guardião Automático");
        menuKill.Click += async (s, e) => await KillAndBlockSelectedAsync();
        _contextMenu.Items.Add(menuKill);

        _lvProcesses = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            ContextMenuStrip = _contextMenu,
            BackColor = Color.FromArgb(24, 27, 44),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        _lvProcesses.SelectedIndexChanged += (s, e) => UpdateKillButtonLabel();
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

    private void UpdateKillButtonLabel()
    {
        if (_lvProcesses.SelectedItems.Count == 0)
        {
            _btnKillAndBlock.Text = "🚫 Finalizar & Bloquear no Guardião";
            return;
        }

        var tag = _lvProcesses.SelectedItems[0].Tag;
        if (tag is SvchostGroupItem)
        {
            _btnKillAndBlock.Text = "🚫 Derrubar svchost & Bloquear";
        }
        else if (tag is DetailedProcessItem proc)
        {
            if (proc.Name.Equals("svchost", StringComparison.OrdinalIgnoreCase))
                _btnKillAndBlock.Text = "🚫 Derrubar svchost & Bloquear";
            else
                _btnKillAndBlock.Text = $"🚫 Finalizar {proc.Name} & Bloquear";
        }
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
        UpdateKillButtonLabel();
    }

    private async Task ExecuteOptimizationAsync()
    {
        _btnOptimizeNow.Enabled = false;
        _lblStatus.Text = "Otimizando processos e finalizando telemetria/atualizadores ociosos...";
        _lblStatus.ForeColor = Color.FromArgb(6, 182, 212);

        var result = await ProcessOptimizerService.OptimizeProcessesAsync();

        // Tambem faz enforcement automatico de svchost indesejados
        int svchostKilled = await SvchostInspectorService.AutoDetectAndKillUnwantedSvchostAsync();

        _lblStatus.Text = $"🎉 Sucesso! {result.TerminatedCount} processos e {svchostKilled} hospedeiros svchost finalizados. ~{result.FreedMemoryMb:F0} MB recuperados!";
        _lblStatus.ForeColor = Color.FromArgb(16, 185, 129);

        LoadProcesses();
        _btnOptimizeNow.Enabled = true;
    }

    private async Task KillAndBlockSelectedAsync()
    {
        if (_lvProcesses.SelectedItems.Count == 0)
        {
            MessageBox.Show("Selecione um processo ou hospedeiro svchost da lista para finalizar e bloquear.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var tag = _lvProcesses.SelectedItems[0].Tag;

        // Caso 1: Item selecionado no modo Svchost Inspector
        if (tag is SvchostGroupItem svchostGroup)
        {
            var unwanted = svchostGroup.Services.Where(s => s.IsUnwanted).ToList();
            string serviceListStr = unwanted.Count > 0
                ? string.Join(", ", unwanted.Select(u => u.Name))
                : string.Join(", ", svchostGroup.Services.Select(s => s.Name));

            var confirm = MessageBox.Show(
                $"Deseja realmente derrubar o processo svchost PID {svchostGroup.Pid} e desativar permanentemente o(s) serviço(s) '{serviceListStr}' no Windows para serem bloqueados pelo Guardião?",
                "Confirmar Bloqueio Permanente",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes) return;

            _lblStatus.Text = "Derrubando hospedeiro e adicionando serviços ao Guardião...";
            await SvchostInspectorService.KillAndBlacklistSvchostAsync(svchostGroup.Pid);

            _lblStatus.Text = $"🎉 Hospedeiro PID {svchostGroup.Pid} finalizado e adicionado ao Guardião com sucesso!";
            LoadProcesses();
            return;
        }

        // Caso 2: Item selecionado no modo de Processos Gerais (DetailedProcessItem)
        if (tag is DetailedProcessItem procItem)
        {
            if (procItem.Name.Equals("svchost", StringComparison.OrdinalIgnoreCase))
            {
                var services = SvchostInspectorService.GetServicesForPid(procItem.Pid);
                string svcNames = services.Count > 0 ? string.Join(", ", services.Select(s => s.Name)) : "Serviço nativo";

                var confirm = MessageBox.Show(
                    $"Deseja derrubar a instância svchost (PID {procItem.Pid}) contendo '{svcNames}' e adicionar à lista negra do Guardião?",
                    "Confirmar Derrubada de Hospedeiro",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirm != DialogResult.Yes) return;

                _lblStatus.Text = $"Derrubando svchost PID {procItem.Pid}...";
                await SvchostInspectorService.KillAndBlacklistSvchostAsync(procItem.Pid);
                _lblStatus.Text = $"🎉 Hospedeiro svchost PID {procItem.Pid} derrubado e registrado no Guardião com sucesso!";
            }
            else
            {
                var confirm = MessageBox.Show(
                    $"Deseja finalizar o processo '{procItem.Name}' (PID {procItem.Pid}) e adicioná-lo à lista negra do Guardião para ser derrubado automaticamente sempre que iniciar?",
                    "Confirmar Finalização & Bloqueio no Guardião",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirm != DialogResult.Yes) return;

                _lblStatus.Text = $"Finalizando {procItem.Name}...";
                await ProcessOptimizerService.KillAndBlacklistProcessAsync(procItem.Pid, procItem.Name);
                _lblStatus.Text = $"🎉 Processo '{procItem.Name}' finalizado e adicionado ao Guardião com sucesso!";
            }

            LoadProcesses();
        }
    }
}

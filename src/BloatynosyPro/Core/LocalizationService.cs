using System.Text.Json;

namespace BloatynosyPro.Core;

public static class Loc
{
    private static readonly Dictionary<string, string> _translations = new(StringComparer.OrdinalIgnoreCase);
    public static string CurrentLanguage { get; private set; } = "pt-BR";

    public static event EventHandler? LanguageChanged;

    static Loc()
    {
        Load("pt-BR");
    }

    public static void SetLanguage(string langCode)
    {
        if (CurrentLanguage.Equals(langCode, StringComparison.OrdinalIgnoreCase))
            return;

        Load(langCode);
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void Load(string langCode)
    {
        CurrentLanguage = langCode;
        _translations.Clear();

        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(appDir, "Localization", $"{langCode}.json");

        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(appDir, "Localization", "pt-BR.json");
        }

        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                using var doc = JsonDocument.Parse(json);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    _translations[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }
            }
            catch
            {
                // Fallback para defaults
            }
        }

        ApplyDefaultFallback();
    }

    public static string Get(string key, string? defaultValue = null)
    {
        if (_translations.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
            return val;

        return defaultValue ?? key;
    }

    public static string Format(string key, params object[] args)
    {
        string raw = Get(key);
        try
        {
            return string.Format(raw, args);
        }
        catch
        {
            return raw;
        }
    }

    private static void ApplyDefaultFallback()
    {
        bool isPt = CurrentLanguage.StartsWith("pt", StringComparison.OrdinalIgnoreCase);

        SetDefault("App_Title", "BloatynosyPro · Otimizador Gamer & Privacidade");
        SetDefault("Nav_Dashboard", isPt ? "Visão Geral" : "Dashboard");
        SetDefault("Nav_Performance", isPt ? "Desempenho em Tempo Real" : "Realtime Performance");
        SetDefault("Nav_Tweaks", isPt ? "Otimizações do Windows" : "Windows Tweaks");
        SetDefault("Nav_Bloatware", isPt ? "Desinstalador de Bloatware" : "Bloatware Remover");
        SetDefault("Nav_Cleaner", isPt ? "Limpeza de Disco" : "Disk Cleaner");
        SetDefault("Nav_Startup", isPt ? "Inicialização Rápida" : "Startup Apps");
        SetDefault("Nav_Settings", isPt ? "Configurações" : "Settings");

        SetDefault("Dash_HealthTitle", isPt ? "Índice de Otimização do Sistema" : "System Health Score");
        SetDefault("Dash_QuickOpt", isPt ? "✨ Otimização Inteligente Recomendada (1 Clique)" : "✨ Recommended Smart Optimize (1 Click)");
        SetDefault("Dash_QuickOptSub", isPt ? "Desativa telemetria invasiva, bloqueia anúncios e libera memória RAM com segurança." : "Safely disables telemetry, blocks ads and flushes RAM.");
        SetDefault("Dash_Profile_Gamer", isPt ? "🎮 Modo Gamer (Máximo Desempenho)" : "🎮 Gamer Mode (Max Performance)");
        SetDefault("Dash_Profile_Privacy", isPt ? "🛡️ Modo Privacidade Total" : "🛡️ Total Privacy Mode");
        SetDefault("Dash_Profile_Balanced", isPt ? "⚡ Modo Equilibrado" : "⚡ Balanced Mode");

        SetDefault("Perf_Ram", isPt ? "Memória RAM" : "RAM Memory");
        SetDefault("Perf_Cpu", isPt ? "Processador (CPU)" : "Processor (CPU)");
        SetDefault("Perf_Disk", isPt ? "Armazenamento (Disco C:)" : "Storage (Drive C:)");
        SetDefault("Perf_CleanRam", isPt ? "🧹 Liberar RAM Agora" : "🧹 Clean RAM Now");
        SetDefault("Perf_RamCleaned", isPt ? "Memória liberada com sucesso!" : "RAM memory successfully cleaned!");

        SetDefault("Tweaks_Scan", isPt ? "🔍 Verificar Regras do Windows" : "🔍 Scan Windows Rules");
        SetDefault("Tweaks_Apply", isPt ? "⚡ Aplicar Selecionados" : "⚡ Apply Selected");
        SetDefault("Tweaks_Restore", isPt ? "↩ Restaurar Padrões" : "↩ Restore Defaults");
        SetDefault("Tweaks_FilterAll", isPt ? "Todas as Categorias" : "All Categories");

        SetDefault("Bloat_Scan", isPt ? "🔍 Buscar Aplicativos Instalados" : "🔍 Scan Installed Apps");
        SetDefault("Bloat_Remove", isPt ? "🗑️ Desinstalar Selecionados" : "🗑️ Uninstall Selected");
        SetDefault("Bloat_SafeOnly", isPt ? "Selecionar Seguros para Remoção" : "Select Safe for Removal");

        SetDefault("Clean_Analyze", isPt ? "🔍 Analisar Lixo do Sistema" : "🔍 Analyze System Junk");
        SetDefault("Clean_Execute", isPt ? "🧹 Limpar Arquivos Temporários" : "🧹 Clean Temp Files");

        SetDefault("Lang_Label", isPt ? "Idioma da Aplicação" : "Application Language");
        SetDefault("Lang_PT", "Português (Brasil)");
        SetDefault("Lang_EN", "English (US)");
    }

    private static void SetDefault(string key, string value)
    {
        if (!_translations.ContainsKey(key))
            _translations[key] = value;
    }
}

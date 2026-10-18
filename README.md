# ⚡ BloatynosyPro

> **Otimizador Inteligente de Performance, Desempenho e Privacidade para Windows 10 e 11**  
> Construído em C# com **.NET 9.0 (Windows Forms Moderno)** baseado na engenharia reversa do ecossistema Bloatynosy.

---

## ⚡ Assistente Instalador e Execução Rápida

Na raiz do projeto existem scripts práticos para você e para o usuário final:

- 📦 **`Instalar-BloatynosyPro.bat`**: **Assistente de Instalação Autônomo**. Não exige que o usuário tenha .NET SDK ou runtime instalado. Ele instala no diretório `%LocalAppData%\Programs\BloatynosyPro`, cria o atalho oficial na **Área de Trabalho** e no **Menu Iniciar**!
- 🛠️ **`Gerar-Instalador.bat`**: Compila a distribuição **Self-Contained (Zero SDK)** onde todo o runtime .NET 9 fica embutido no `.exe`.
- 🚀 **`Gerar-Executavel.bat`**: Compila e publica a versão **Release de alta performance (sem debugger)** diretamente na pasta `Executavel\BloatynosyPro.exe`.
- 🟢 **`Iniciar-BloatynosyPro.bat`** (ou **`run.bat`**): Inicia a aplicação instantaneamente. Se ainda não tiver sido compilada, ele gera e abre automaticamente.
  - *Dica:* Para aplicar regras que afetam todo o sistema (`HKLM`) e remover bloatwares nativos com permissão total, clique com o botão direito e selecione **"Executar como Administrador"**.

---

## 🚀 Como Subir e Compilar o Projeto Manualmente

### Pré-requisitos
- Windows 10 ou 11 (64-bit)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) instalado

### Pelo Terminal (PowerShell / CMD)
```bash
# 1. Clonar o repositório
git clone https://github.com/PedroLucasCruz/BloatynosyPro.git
cd BloatynosyPro

# 2. Restaurar dependências e compilar em Release
dotnet build -c Release

# 3. Executar o projeto
dotnet run --project src/BloatynosyPro/BloatynosyPro.csproj -c Release
```

### Pelo Visual Studio 2022+ / VS Code
1. Abra o arquivo de solução **`BloatynosyPro.sln`**.
2. Selecione a configuração **Release** e plataforma **x64**.
3. Pressione **F5** ou **Ctrl+F5** para executar.

---

## 🧠 Como o BloatynosyPro Funciona

O **BloatynosyPro** foi desenvolvido para resolver o maior problema dos otimizadores tradicionais: **excesso de complexidade técnica que confunde o usuário**. Ele analisa a máquina e aplica as melhores ações de forma automática e segura, sem travamentos.

```
┌────────────────────────────────────────────────────────────────────────┐
│                          BloatynosyPro (WinForms)                      │
│                                                                        │
│   ┌─────────────────┐   ┌───────────────────┐   ┌──────────────────┐   │
│   │  WinRules.ini   │   │   AppRules.ini    │   │  Métricas Win32  │   │
│   │ (1.200+ Regras) │   │  (Classificação)  │   │ (RAM/CPU/GPU/IO) │   │
│   └────────┬────────┘   └─────────┬─────────┘   └────────┬─────────┘   │
│            ▼                      ▼                      ▼             │
│   ┌─────────────────┐   ┌───────────────────┐   ┌──────────────────┐   │
│   │ WinRulesEngine  │   │   AppxDebloater   │   │  SystemMetrics   │   │
│   └────────┬────────┘   └─────────┬─────────┘   └────────┬─────────┘   │
│            └──────────────────────┼──────────────────────┘             │
│                                   ▼                                    │
│             ✨ OTIMIZAÇÃO INTELIGENTE EM 1 CLIQUE (Dashboard)         │
└────────────────────────────────────────────────────────────────────────┘
```

---

## ✨ Módulos e Funcionalidades em Detalhes

### 1. ⚡ Dashboard & Otimização em 1-Clique ("Zero Esforço Mental")
- **Score de Saúde (0 a 100%)**: Calcula o nível de otimização atual com base no uso de RAM, arquivos temporários acumulados e status das políticas do Windows.
- **Botão "Otimizar Agora (1 Clique)"**: Aplica automaticamente as configurações recomendadas seguras (bloqueia anúncios no Windows Explorer e Menu Iniciar, desativa telemetria invasiva e descarrega a memória RAM).
- **Perfis Especializados**:
  - 🎮 **Modo Gamer**: Desativa o GameDVR em segundo plano, otimiza throttling de energia e reduz animações desnecessárias para garantir mais FPS e zero engasgos (stuttering).
  - 🛡️ **Modo Privacidade Total**: Desativa o envio de dados diagnósticos, telemetria e identificadores de anúncios da Microsoft.
  - ⚡ **Modo Equilibrado**: Otimizações do dia a dia mantendo 100% dos recursos funcionais.

### 2. 🤖 Guardião Automático em Tempo Real (Auto-Pilot / Zero Esforço)
- **Otimização Contínua e Silenciosa**: O usuário não precisa ficar abrindo o programa e clicando em "Otimizar". O Guardião opera em segundo plano na bandeja do sistema (System Tray).
- **Intercepção de Bloqueio Imediato**: Sempre que um serviço ou processo indesejado da sua lista negra tentar subir no Windows, o Guardião derruba na hora.
- **Auto-Descarregamento de Memória RAM**: Executa a liberação e descarregamento contínuo de páginas de memória RAM automaticamente (idêntico ao botão manual), mantendo o Windows sempre com memória livre sem exigir intervenção humana.
- **Limpeza de Disco Silenciosa**: Limpa periodicamente em segundo plano os itens padrão de lixo do sistema (`%TEMP%`, `Windows\Temp`, `Prefetch` e relatórios de erro `CrashDumps`).
- **Verificação & Reparo Automático de Integridade**: Periodicamente verifica se o sistema operacional possui inconsistências ou arquivos corrompidos (via DISM CheckHealth) e, caso detecte avarias, aciona automaticamente o reparo com DISM RestoreHealth e SFC Scannow sem incomodar o usuário.
- **Controle com 1 Clique**: Um interruptor destacado no Dashboard permite ativar ou pausar o Guardião instantaneamente, com 4 contadores em tempo real (Bloqueios, RAM, Disco e Reparos).

### 3. 🔥 Otimizador de Processos & Inspetor de Hospedeiros (`svchost.exe`)
- **Raio-X de Hospedeiros de Serviços Nativos**: O sistema inspeciona dentro de cada PID de `svchost.exe` para listar quais serviços do Windows estão rodando dentro dele (ex: `DiagTrack`, `dmwappushservice`, `MapsBroker`, `PcaSvc`, `WerSvc`, `RetailDemo`).
- **Classificação Inteligente**: Separa hospedeiros que contêm serviços indesejados daqueles que contêm serviços vitais do sistema (Rede, Áudio, RPC).
- **Finalização & Bloqueio com 1 Clique (ou Botão Direito)**:
  - Disponível em **todas as abas e filtros** da tabela de processos.
  - Ao derrubar um hospedeiro `svchost.exe` indesejado, o sistema não apenas encerra o PID atual, mas **desativa permanentemente a inicialização do serviço no Windows (`sc config <serviço> start= disabled`)** e registra na lista negra do Guardião para ser derrubado preventivamente.
  - Ao finalizar qualquer outro processo indesejado pelo usuário, o nome é automaticamente registrado no Guardião para nunca mais consumir recursos da máquina.
  - Lista negra persistente sincronizada automaticamente no `Config.json`.

### 4. 🚀 Inicialização com o Windows & Preset Configurável
- **Auto-Otimização no Boot**: Opção de ativar o BloatynosyPro para iniciar com o Windows em segundo plano (minimizando na bandeja/System Tray).
- **Preset Selecionável**: O usuário escolhe se no boot deve aplicar o *Modo Equilibrado*, *Modo Gamer* ou *Modo Privacidade*.
- **Ativação/Desativação com 1 Clique**: Configuração simples e reversível a qualquer momento na aba de Configurações.

### 4. 🛡️ Integridade & Reparo Automático do Windows (1 Clique)
- **Diagnóstico e Reparação Completa em 1 Toque**:
  - **1. DISM (`/Online /Cleanup-Image /RestoreHealth`)**: Corrige o repositório de componentes e imagens base do Windows que impedem ferramentas de reparo de funcionar.
  - **2. SFC (`/scannow`)**: Varre e repara automaticamente todos os arquivos essenciais e bibliotecas DLL corrompidas do sistema operacional.
  - **3. CHKDSK Online (`chkdsk C: /scan`)**: Analisa o sistema de arquivos no HD ou SSD em busca de falhas e inconsistências lógicas que causam travamentos e lentidão, sem exigir que o computador seja reiniciado!
- **Feedback Visual com Console em Tempo Real**: Barra de progresso animada com 3 fases e exibição dos logs oficiais gerados pelo Windows.

### 5. 🗑️ Desinstalador com Desinstalação Profunda (100% Livre de Vestígios)
- **Eliminação Total de Rastros**:
  - Além de desinstalar o pacote AppX, o sistema vasculha o disco e o registro apagando pastas de cache em `%LocalAppData%\Packages\<PackageFullName>`, diretórios residuais em `%AppData%`, `%LocalAppData%`, `C:\ProgramData` e chaves órfãs no registro do Windows (`HKCU\Software\...`).
  - Seu computador fica **100% livre** de qualquer informação ou resíduo deixado pelo programa desinstalado.
- Classificação inteligente baseada no `AppRules.ini`:
  - 🛡️ **Protegido do Sistema**: Apps vitais (Loja, Instalador de Pacotes) são bloqueados contra remoção acidental.
  - 🗑️ **Seguro para Remoção**: Bloatwares patrocinados e dispensáveis (BingWeather, Solitaire, TikTok, Clipchamp, etc.).

### 6. 📊 Monitoramento de Hardware em Tempo Real
- **Memória RAM**: Exibe o consumo exato em GBs e percentual, com botão de **Limpeza Imediata de Memória (`EmptyWorkingSet` via Win32 API)**, liberando centenas de megabytes de RAM inativa instantaneamente.
- **Processador (CPU)**: Acompanha a carga de uso geral e exibe a tabela com os **processos que mais consomem processamento e memória**.
- **Disco do Sistema (C:)**: Mostra o percentual de ocupação e espaço livre em gigabytes.
- **Placas de Vídeo (GPU)**: Identifica automaticamente adaptadores gráficos dedicados (NVIDIA, AMD) e integrados (Intel).
- **Tráfego de Rede**: Medição contínua de taxas de Download e Upload em KB/s.

### 7. ⚙️ Otimizações do Windows (Motor WinRules)
- Catálogo com mais de **1.200 regras** divididas em categorias:
  - `Ads` (Propagandas no Explorer, Tela de Bloqueio e Configurações).
  - `AI` (Desativação de recursos invasivos de IA, Copilot e Recall).
  - `Gaming` (Visual FX, GameDVR, PowerThrottling).
  - `Privacy` (Telemetria, Diagnósticos, Localização).
  - `System` (Menu de contexto clássico, serviços redundantes).
  - `Taskbar` (Widgets, Bing Search no Iniciar, Chat).
- Permite checar o estado atual de cada regra, aplicar as alterações ou restaurar os padrões do Windows a qualquer momento (*Rollback*).

### 8. 🧹 Limpeza de Disco & Arquivos Temporários
- Analisa diretórios com acúmulo de lixo:
  - Arquivos temporários do usuário (`%TEMP%`)
  - Temporários do sistema (`Windows\Temp`)
  - Cache de pré-inicialização (`Windows\Prefetch`)
  - Relatórios de falha e despejos de memória (`CrashDumps`)
- Liberação segura de espaço em disco com 1 clique.

### 9. 🚀 Gerenciador de Inicialização (Startup)
- Lista programas registrados em `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, `HKLM` e na pasta de Inicializar.
- Permite desativar programas desnecessários para acelerar o tempo de boot do computador.

### 10. 🌐 Internacionalização Dinâmica (i18n)
- Suporte nativo completo a **Português do Brasil (PT-BR)** como idioma padrão.
- Seletor imediato para **Inglês (EN-US)** na aba de Configurações, trocando todos os textos da interface sem necessidade de reiniciar.

---

## 🎨 Conceito de UX Designer em Windows Forms

Diferente das interfaces tradicionais de WinForms cinzas e estáticas, o **BloatynosyPro** adota padrões modernos de design:
- **Paleta Dark Cyber / Fluent**: Fundo escuro com alto contraste (`#0f111a`, `#181b2a`, `#242844`), textos nítidos e acentos vibrantes Neon Cyan (`#06b6d4`), Roxo (`#8b5cf6`) e Verde Esmeralda (`#10b981`).
- **Controles GDI+ Personalizados**: Painéis com cantos arredondados, botões modernos com estados de Hover e Click, e barras de progresso animadas.
- **Duplo Buffer Ativado**: Renderização suave sem cintilação (*flickering*) na tela.
- **Navegação Lateral Intuitiva**: Sidebar com ícones e transições instantâneas entre abas.

---

## 📁 Estrutura do Repositório

```
BloatynosyPro/
├── Iniciar-BloatynosyPro.bat    # Executável batch de 1 clique
├── run.bat                     # Atalho de execução rápida
├── BloatynosyPro.sln           # Solução .NET 9
├── README.md                   # Documentação completa
└── src/
    └── BloatynosyPro/
        ├── BloatynosyPro.csproj
        ├── Program.cs          # Ponto de entrada (HighDpiMode PerMonitorV2)
        ├── app.manifest        # Manifesto com compatibilidade Win 10/11
        ├── Forms/
        │   └── MainForm.cs     # Shell principal com Sidebar e navegação
        ├── Views/
        │   ├── DashboardView.cs
        │   ├── PerformanceView.cs
        │   ├── ProcessOptimizerView.cs
        │   ├── SystemIntegrityView.cs
        │   ├── SystemTweaksView.cs
        │   ├── BloatwareView.cs
        │   ├── DiskCleanerView.cs
        │   ├── StartupManagerView.cs
        │   └── SettingsView.cs
        ├── Controls/
        │   ├── ModernCard.cs
        │   ├── ModernButton.cs
        │   └── ModernProgressBar.cs
        ├── Core/
        │   ├── WinRulesEngine.cs
        │   ├── AppxDebloater.cs
        │   ├── DeepUninstallService.cs
        │   ├── SystemIntegrityService.cs
        │   ├── SvchostInspectorService.cs
        │   ├── AutoGuardianService.cs
        │   ├── AutoStartupService.cs
        │   ├── MemoryOptimizer.cs
        │   ├── ProcessOptimizerService.cs
        │   ├── SystemMetricsService.cs
        │   ├── DiskCleanerService.cs
        │   ├── StartupService.cs
        │   └── LocalizationService.cs
        ├── Data/
        │   ├── WinRules.ini    # 1.277 linhas de regras
        │   └── AppRules.ini    # Regras de segurança de pacotes
        └── Localization/
            ├── pt-BR.json      # Tradução PT-BR
            └── en.json         # Tradução EN-US
```

---

## 👤 Autor
Desenvolvido por **PedroLucasCruz**  
Projeto baseado na engenharia reversa e estudos do ecossistema Bloatynosy.

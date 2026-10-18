# ⚡ BloatynosyPro

> **Otimizador Inteligente de Performance, Desempenho e Privacidade para Windows 10 e 11**  
> Desenvolvido em C# com **.NET 9.0 (Windows Forms Moderno)** com base no motor do Bloatynosy.

---

## 🚀 Visão Geral

**BloatynosyPro** une o catálogo de mais de 1.200 regras de otimização e desinstalação de bloatwares do **Bloatynosy** com uma **interface gráfica moderna (UX Designer Dark/Gamer)** e uma suíte completa de **diagnóstico de hardware e desempenho em tempo real**.

O objetivo é proporcionar máxima velocidade e estabilidade ao computador com **zero esforço mental** para o usuário através da **Otimização Inteligente em 1 Clique**.

---

## ✨ Principais Funcionalidades

### 1. ⚡ Dashboard & Otimização em 1-Clique
- **Índice de Saúde do Sistema**: score em tempo real de 0% a 100%.
- **Botão Mágico "Otimizar Agora (1 Clique)"**: aplica automaticamente as melhores práticas recomendadas de sistema, bloqueia anúncios e descarrega a memória RAM.
- **Perfis Especializados**:
  - 🎮 **Modo Gamer**: desativa GameDVR em segundo plano, otimiza throttling e efeitos visuais para ganho de FPS.
  - 🛡️ **Modo Privacidade**: bloqueia telemetria, biometria e anúncios diagnósticos.
  - ⚡ **Modo Equilibrado**: focado em estabilidade e rapidez para uso diário.

### 2. 📊 Monitoramento de Hardware em Tempo Real
- **Memória RAM**: monitoramento contínuo de GBs usados/livres e botão de **Limpeza Imediata de Working Set (`EmptyWorkingSet` via Win32 API)**.
- **Processador (CPU)**: taxa de utilização do processador e ranking dos processos mais pesados.
- **Armazenamento (Disco C:)**: espaço livre vs total e detecção de sobrecarga.
- **Placas de Vídeo (GPU)**: identificação de adaptadores gráficos instalados.
- **Tráfego de Rede**: taxas de Download e Upload em KB/s.

### 3. 🛡️ Otimizações do Windows (WinRules Engine)
- Mais de 1.200 regras categorizadas: *Ads*, *AI/Copilot*, *Appearance*, *Edge*, *Gaming*, *Privacy*, *System*, *Taskbar*, *Updates*.
- Leitura, aplicação e restauração de chaves do Registro (`HKCU` e `HKLM`).
- Filtro por categorias e seleção automática de itens recomendados.

### 4. 🗑️ Desinstalador de Bloatwares (AppX)
- Varredura de aplicativos nativos do Windows classificados como **Seguros para Remoção**, **Protegidos do Sistema** e **Pessoais**.
- Desinstalação em lote via PowerShell.

### 5. 🧹 Limpeza de Disco & Arquivos Temporários
- Scanner de resíduos do sistema (`%TEMP%`, `Windows\Temp`, `Prefetch` e `CrashDumps`).
- Liberação imediata de centenas de MBs ou GBs de espaço em disco.

### 6. 🚀 Gerenciador de Inicialização (Startup)
- Lista de aplicativos configurados no registro de inicialização (`Run` e `Startup Folder`).
- Desativação rápida de inicializadores desnecessários para acelerar o boot.

### 7. 🌐 Internacionalização (i18n)
- Idioma padrão: **Português do Brasil (PT-BR)**.
- Alternador instantâneo para **Inglês (US)** sem necessidade de reiniciar.

---

## 🛠️ Tecnologias Utilizadas
- **Linguagem:** C# 13
- **Framework:** .NET 9.0-windows (Windows Forms)
- **APIs:** Win32 Native (`kernel32.dll`, `psapi.dll`), `System.Management` (WMI), `System.Net.NetworkInformation`
- **Padrão Visual:** Dark Cyber / Fluent (sem flickering, duplo buffer ativado)

---

## 📦 Como Compilar e Executar

### Pré-requisitos:
- Windows 10 ou 11 (64-bit)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Compilação:
```bash
git clone https://github.com/PedroLucasCruz/BloatynosyPro.git
cd BloatynosyPro
dotnet build -c Release
```

### Executar:
```bash
./src/BloatynosyPro/bin/Release/net9.0-windows/BloatynosyPro.exe
```

---

## 👤 Autor
Desenvolvido por **PedroLucasCruz**  
Projeto baseado na engenharia reversa e estudos do ecossistema Bloatynosy.

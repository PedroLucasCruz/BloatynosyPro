@echo off
title BloatynosyPro - Gerador de Pacote Autonomo (Self-Contained)
color 0b
cd /d "%~dp0"

echo ======================================================================
echo        ? GERADOR DO PACOTE DE INSTALACAO AUTONOMO (ZERO SDK)
echo ======================================================================
echo.

echo [1/4] Fechando instancias ativas do BloatynosyPro (se houver)...
taskkill /f /im BloatynosyPro.exe >nul 2>&1
ping 127.0.0.1 -n 2 >nul

echo [2/4] Limpando arquivos anteriores do instalador...
if exist "Instalador\Arquivos" rmdir /s /q "Instalador\Arquivos" >nul 2>&1

echo [3/4] Compilando versao Self-Contained (Runtime .NET 9 embutido no .exe)...
echo      O usuario final NAO precisara instalar nenhum SDK nem pacote!
echo.
dotnet publish src\BloatynosyPro\BloatynosyPro.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "Instalador\Arquivos"

if %errorlevel% neq 0 (
    color 0c
    echo.
    echo [ERRO] Falha ao publicar o pacote autonomo.
    pause
    exit /b 1
)

echo.
echo [4/4] Copiando regras e dados de traducao para o pacote...
if not exist "Instalador\Arquivos\Data" mkdir "Instalador\Arquivos\Data"
if not exist "Instalador\Arquivos\Localization" mkdir "Instalador\Arquivos\Localization"
copy /y "src\BloatynosyPro\Data\*.ini" "Instalador\Arquivos\Data\" >nul 2>&1
copy /y "src\BloatynosyPro\Localization\*.json" "Instalador\Arquivos\Localization\" >nul 2>&1

color 0a
echo.
echo ======================================================================
echo    ?? PACOTE DE INSTALACAO GERADO COM SUCESSO!
echo    Pasta: %~dp0Instalador\Arquivos\
echo    Executavel Autonomo: BloatynosyPro.exe (100%% independente de SDK)
echo.
echo    O usuario final so precisa executar:
echo    ?? Instalar-BloatynosyPro.bat
echo ======================================================================
echo.
pause

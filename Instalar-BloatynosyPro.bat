@echo off
title Assistente de Instalacao - BloatynosyPro
color 0b
cd /d "%~dp0"

echo ======================================================================
echo                ? ASSISTENTE DE INSTALACAO - BLOATYNOSYPRO
echo ======================================================================
echo.
echo  Bem-vindo ao Assistente de Instalacao do BloatynosyPro!
echo  Este pacote e 100%% autonomo (Self-Contained) e NAO requer
echo  instalacao de .NET SDK ou qualquer ferramenta de desenvolvedor.
echo.
echo ======================================================================
echo.

set TARGET_DIR=%LocalAppData%\Programs\BloatynosyPro

echo [1/4] Destino da instalacao: %TARGET_DIR%
echo.
set /p CONTINUAR="Deseja prosseguir com a instalacao agora? (S/N): "
if /i not "%CONTINUAR%"=="S" (
    echo Instalacao cancelada pelo usuario.
    pause
    exit /b 0
)

echo.
echo [2/4] Criando diretorios de instalacao...
if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"
if not exist "%TARGET_DIR%\Data" mkdir "%TARGET_DIR%\Data"
if not exist "%TARGET_DIR%\Localization" mkdir "%TARGET_DIR%\Localization"

echo [3/4] Copiando arquivos do aplicativo...
if exist "Instalador\Arquivos\BloatynosyPro.exe" (
    copy /y "Instalador\Arquivos\BloatynosyPro.exe" "%TARGET_DIR%\" >nul
    copy /y "Instalador\Arquivos\Data\*.ini" "%TARGET_DIR%\Data\" >nul
    copy /y "Instalador\Arquivos\Localization\*.json" "%TARGET_DIR%\Localization\" >nul
) else if exist "Executavel\BloatynosyPro.exe" (
    copy /y "Executavel\BloatynosyPro.exe" "%TARGET_DIR%\" >nul
    copy /y "Executavel\*.dll" "%TARGET_DIR%\" >nul 2>&1
    copy /y "Executavel\Data\*.ini" "%TARGET_DIR%\Data\" >nul
    copy /y "Executavel\Localization\*.json" "%TARGET_DIR%\Localization\" >nul
) else (
    echo [ERRO] Arquivos de instalacao nao encontrados.
    pause
    exit /b 1
)

echo [4/4] Criando atalhos na Area de Trabalho e Menu Iniciar...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\BloatynosyPro.lnk'); $s.TargetPath = '%TARGET_DIR%\BloatynosyPro.exe'; $s.WorkingDirectory = '%TARGET_DIR%'; $s.Save()"
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut([Environment]::GetFolderPath('StartMenu') + '\Programs\BloatynosyPro.lnk'); $s.TargetPath = '%TARGET_DIR%\BloatynosyPro.exe'; $s.WorkingDirectory = '%TARGET_DIR%'; $s.Save()"

color 0a
echo.
echo ======================================================================
echo    ?? INSTALACAO CONCLUIDA COM SUCESSO!
echo    - Atalho criado na sua Area de Trabalho
echo    - Atalho criado no Menu Iniciar
echo    - Nenhuma ferramenta externa ou SDK e necessaria!
echo ======================================================================
echo.

set /p INICIAR="Deseja iniciar o BloatynosyPro agora? (S/N): "
if /i "%INICIAR%"=="S" (
    start "" "%TARGET_DIR%\BloatynosyPro.exe"
)

exit /b 0

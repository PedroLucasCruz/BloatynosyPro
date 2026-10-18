@echo off
title BloatynosyPro - Iniciando...
cd /d "%~dp0"

echo ========================================================
echo        ? BloatynosyPro - Iniciar Aplicacao
echo ========================================================
echo.

:: Verifica privilegios de Administrador
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [AVISO] O BloatynosyPro funciona melhor como Administrador
    echo         para aplicar regras de sistema (HKLM) e debloater.
    echo.
)

:: 1. Tenta abrir a versao publicada de alta performance
if exist "Executavel\BloatynosyPro.exe" (
    echo [OK] Abrindo versao otimizada (Release)...
    start "" "Executavel\BloatynosyPro.exe"
    exit /b 0
)

:: 2. Tenta abrir a versao compilada em bin
if exist "src\BloatynosyPro\bin\Release\net9.0-windows\BloatynosyPro.exe" (
    echo [OK] Abrindo executavel...
    start "" "src\BloatynosyPro\bin\Release\net9.0-windows\BloatynosyPro.exe"
    exit /b 0
)

:: 3. Se nao existir nenhum, chama o gerador
echo [INFO] Executavel ainda nao gerado. Compilando agora...
call Gerar-Executavel.bat

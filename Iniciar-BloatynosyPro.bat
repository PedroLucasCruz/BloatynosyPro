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

set EXE_PATH=src\BloatynosyPro\bin\Release\net9.0-windows\BloatynosyPro.exe

if exist "%EXE_PATH%" (
    echo [OK] Executavel encontrado! Abrindo BloatynosyPro...
    start "" "%EXE_PATH%"
    exit /b 0
)

echo [INFO] Compilando o projeto pela primeira vez (Release)...
dotnet build src\BloatynosyPro\BloatynosyPro.csproj -c Release

if %errorlevel% equ 0 (
    echo [OK] Compilacao concluida com sucesso!
    start "" "%EXE_PATH%"
) else (
    echo [ERRO] Falha ao compilar. Verifique se o .NET 9 SDK esta instalado.
    pause
)

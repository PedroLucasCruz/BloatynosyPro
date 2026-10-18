@echo off
title BloatynosyPro - Gerador de Executavel Otimizado
color 0b
cd /d "%~dp0"

echo ======================================================================
echo          ? BLOATYNOSYPRO - GERADOR DE EXECUTAVEL DE ALTA PERFORMANCE
echo ======================================================================
echo.
echo [1/3] Limpando compilacoes anteriores...
if exist "Executavel" rmdir /s /q "Executavel" >nul 2>&1

echo [2/3] Compilando e publicando em modo Release otimizado (x64)...
dotnet publish src\BloatynosyPro\BloatynosyPro.csproj -c Release -r win-x64 -o "Executavel" --self-contained false

if %errorlevel% neq 0 (
    color 0c
    echo.
    echo [ERRO] Ocorreu uma falha na compilacao. Verifique se o .NET 9 SDK esta instalado.
    echo.
    pause
    exit /b 1
)

echo [3/3] Garantindo pastas de regras e idiomas...
if not exist "Executavel\Data" mkdir "Executavel\Data"
if not exist "Executavel\Localization" mkdir "Executavel\Localization"
copy /y "src\BloatynosyPro\Data\*.ini" "Executavel\Data\" >nul 2>&1
copy /y "src\BloatynosyPro\Localization\*.json" "Executavel\Localization\" >nul 2>&1

color 0a
echo.
echo ======================================================================
echo    ?? SUCESSO! O executavel de alta performance foi gerado em:
echo    Pasta: %~dp0Executavel
echo    Arquivo: BloatynosyPro.exe
echo ======================================================================
echo.

set /p ABRIR="Deseja abrir a aplicacao agora? (S/N): "
if /i "%ABRIR%"=="S" (
    echo Abrindo BloatynosyPro...
    start "" "Executavel\BloatynosyPro.exe"
) else (
    echo Abrindo a pasta do executavel...
    explorer "Executavel"
)

exit /b 0

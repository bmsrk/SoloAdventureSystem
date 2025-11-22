@echo off
REM Quick Start Script for KoboldCpp + AI World Generator
REM This script helps you get started with local AI generation

echo ================================
echo KoboldCpp AI World Generator
echo Quick Start Helper
echo ================================
echo.

REM Check if KoboldCpp is running
echo Checking for KoboldCpp...
curl -s http://localhost:5001/api/v1/model >nul 2>&1
if %errorlevel% == 0 (
    echo [OK] KoboldCpp is running at localhost:5001
    echo.
    goto :run_generator
) else (
    echo [!] KoboldCpp is NOT running
    echo.
    echo Please start KoboldCpp first!
    echo.
    echo Quick Start:
    echo   1. Download KoboldCpp: https://github.com/LostRuins/koboldcpp/releases
    echo   2. Download a model (e.g., Phi-3-mini.gguf)
    echo   3. Run: koboldcpp.exe --model Phi-3-mini.gguf --port 5001
    echo.
    echo See KOBOLDCPP_SETUP.md for detailed instructions.
    echo.
    pause
    exit /b 1
)

:run_generator
echo Starting AI World Generator...
echo.
dotnet run --ui

pause

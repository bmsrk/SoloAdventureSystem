@echo off
echo ========================================
echo  SoloAdventureSystem Web UI Launcher
echo ========================================
echo.

cd /d "%~dp0"

echo Checking .NET installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 10 SDK not found!
    echo Please install from: https://dotnet.microsoft.com/download/dotnet/10.0
    pause
    exit /b 1
)

echo.
echo Starting Web UI...
echo.
echo The application will open at:
echo   https://localhost:5001
echo   http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo.

cd SoloAdventureSystem.Web.UI
dotnet run

pause

@echo off
echo ========================================
echo  Model Download Fix Tool
echo ========================================
echo.

echo Cleaning up corrupted model files...
echo.

:: Delete model cache
set MODEL_CACHE=%APPDATA%\SoloAdventureSystem\models
if exist "%MODEL_CACHE%" (
    echo [1/3] Deleting model cache...
    rmdir /s /q "%MODEL_CACHE%"
    echo       Done!
) else (
    echo [1/3] No model cache found (already clean^)
)

:: Delete AI cache
set AI_CACHE=%~dp0SoloAdventureSystem.Web.UI\.aicache
if exist "%AI_CACHE%" (
    echo [2/3] Deleting AI cache...
    rmdir /s /q "%AI_CACHE%"
    echo       Done!
) else (
    echo [2/3] No AI cache found (already clean^)
)

:: Delete temp files
echo [3/3] Cleaning temp files...
del /q "%TEMP%\*gguf*" 2>nul
echo       Done!

echo.
echo ========================================
echo  Cleanup Complete!
echo ========================================
echo.
echo Your system is ready for a fresh download.
echo.
echo Next Steps:
echo   1. Restart the Web UI (run start-webui.bat^)
echo   2. Click "Initialize AI"
echo   3. Wait for complete download (don't cancel!^)
echo.
echo Tip: Use TinyLlama for faster download
echo      Edit appsettings.json: LLamaModelKey = "tinyllama-q4"
echo.
pause

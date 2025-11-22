@echo off
echo ========================================
echo Worlds Directory Diagnostic Tool
echo ========================================
echo.

echo Current Directory:
cd
echo.

echo Looking for solution file...
set FOUND=0

rem Check for .sln files
for /r .. %%i in (*.sln) do (
    echo Found .sln: %%i
    set SOLUTION_DIR=%%~dpi
    set FOUND=1
    goto :found
)

rem Check for .slnx files
for /r .. %%i in (*.slnx) do (
    echo Found .slnx: %%i
    set SOLUTION_DIR=%%~dpi
    set FOUND=1
    goto :found
)

:found
if %FOUND%==1 (
    echo.
    echo Solution Directory: %SOLUTION_DIR%
    echo.
    echo Expected Worlds Path: %SOLUTION_DIR%content\worlds
    echo.
    
    if exist "%SOLUTION_DIR%content\worlds" (
        echo ? Directory exists
        echo.
        echo World files:
        dir /b "%SOLUTION_DIR%content\worlds\*.zip" 2>nul
        if errorlevel 1 (
            echo   ^(none found^)
        )
    ) else (
        echo ? Directory does not exist
        echo.
        echo Creating directory...
        mkdir "%SOLUTION_DIR%content\worlds"
        echo ? Directory created: %SOLUTION_DIR%content\worlds
    )
) else (
    echo ? Could not find solution file (.sln or .slnx)
)

echo.
pause

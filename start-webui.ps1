# SoloAdventureSystem Web UI Launcher
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " SoloAdventureSystem Web UI Launcher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET installation
Write-Host "Checking .NET installation..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "? .NET SDK $dotnetVersion found" -ForegroundColor Green
} catch {
    Write-Host "? ERROR: .NET 10 SDK not found!" -ForegroundColor Red
    Write-Host "Please install from: https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Check CUDA (optional)
Write-Host ""
Write-Host "Checking CUDA installation (optional)..." -ForegroundColor Yellow
try {
    $cudaCheck = nvidia-smi 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? NVIDIA GPU detected - CUDA acceleration available" -ForegroundColor Green
    }
} catch {
    Write-Host "? NVIDIA GPU not detected - will use CPU mode" -ForegroundColor Yellow
}

# Navigate to Web UI project
Write-Host ""
Write-Host "Starting Web UI..." -ForegroundColor Yellow
Set-Location -Path "$PSScriptRoot\SoloAdventureSystem.Web.UI"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " Application will open at:" -ForegroundColor Green
Write-Host "   https://localhost:5001" -ForegroundColor White
Write-Host "   http://localhost:5000" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
Write-Host ""

# Run the application
dotnet run

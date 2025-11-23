# Fix Interrupted Model Download
# This script cleans up corrupted model files and caches

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Model Download Fix Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$fixed = $false

# 1. Check and delete model cache
$modelCache = "$env:APPDATA\SoloAdventureSystem\models"
Write-Host "Checking model cache..." -ForegroundColor Yellow
Write-Host "  Location: $modelCache" -ForegroundColor Gray

if (Test-Path $modelCache) {
    Write-Host "  Status: Found corrupted cache" -ForegroundColor Red
    
    try {
        Remove-Item -Path $modelCache -Recurse -Force -ErrorAction Stop
        Write-Host "  ? Deleted model cache" -ForegroundColor Green
        $fixed = $true
    } catch {
        Write-Host "  ? Failed to delete: $_" -ForegroundColor Red
    }
} else {
    Write-Host "  ? No cache found (already clean)" -ForegroundColor Gray
}

Write-Host ""

# 2. Check and delete AI cache
$aiCache = "$PSScriptRoot\SoloAdventureSystem.Web.UI\.aicache"
Write-Host "Checking AI cache..." -ForegroundColor Yellow
Write-Host "  Location: $aiCache" -ForegroundColor Gray

if (Test-Path $aiCache) {
    Write-Host "  Status: Found AI cache" -ForegroundColor Red
    
    try {
        Remove-Item -Path $aiCache -Recurse -Force -ErrorAction Stop
        Write-Host "  ? Deleted AI cache" -ForegroundColor Green
        $fixed = $true
    } catch {
        Write-Host "  ? Failed to delete: $_" -ForegroundColor Red
    }
} else {
    Write-Host "  ? No AI cache found (already clean)" -ForegroundColor Gray
}

Write-Host ""

# 3. Check and clean temp files
$tempFiles = Get-ChildItem -Path $env:TEMP -Filter "*gguf*" -ErrorAction SilentlyContinue
Write-Host "Checking temp files..." -ForegroundColor Yellow
Write-Host "  Location: $env:TEMP" -ForegroundColor Gray

if ($tempFiles.Count -gt 0) {
    Write-Host "  Status: Found $($tempFiles.Count) temp file(s)" -ForegroundColor Red
    
    try {
        $tempFiles | Remove-Item -Force -ErrorAction Stop
        Write-Host "  ? Deleted temp files" -ForegroundColor Green
        $fixed = $true
    } catch {
        Write-Host "  ? Failed to delete some files: $_" -ForegroundColor Red
    }
} else {
    Write-Host "  ? No temp files found (already clean)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

if ($fixed) {
    Write-Host " ? Cleanup Complete!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your system is now ready for a fresh model download." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Restart the Web UI application" -ForegroundColor White
    Write-Host "  2. Navigate to AI World Generator" -ForegroundColor White
    Write-Host "  3. Click 'Initialize AI'" -ForegroundColor White
    Write-Host "  4. Wait for complete download (don't cancel!)" -ForegroundColor White
    Write-Host ""
    Write-Host "Tip: Start with TinyLlama (~700MB) for faster download" -ForegroundColor Cyan
    Write-Host "     Edit appsettings.json and set: LLamaModelKey = 'tinyllama-q4'" -ForegroundColor Gray
} else {
    Write-Host " ??  No Cleanup Needed" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "No corrupted files found. If you're still having issues:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Check disk space (need 3-5GB free):" -ForegroundColor White
    Write-Host "   Get-PSDrive C | Select-Object Used,Free" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Check internet connection:" -ForegroundColor White
    Write-Host "   Test-NetConnection huggingface.co -Port 443" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. Try a smaller model (TinyLlama)" -ForegroundColor White
    Write-Host ""
    Write-Host "4. Check firewall/antivirus settings" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

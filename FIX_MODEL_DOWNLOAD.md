# ?? Fix Interrupted Model Download

## The Problem
You canceled the model download mid-process, which likely left a corrupted or partial model file.

## The Solution

### Option 1: PowerShell Script (Easiest)
Run this script to clean up and reset:

```powershell
# Navigate to solution directory
cd C:\Users\bruno\source\repos\SoloAdventureSystem

# Delete corrupted model cache
$modelCache = "$env:APPDATA\SoloAdventureSystem\models"
if (Test-Path $modelCache) {
    Write-Host "Deleting corrupted model cache..." -ForegroundColor Yellow
    Remove-Item -Path $modelCache -Recurse -Force
    Write-Host "? Cache cleared!" -ForegroundColor Green
} else {
    Write-Host "No cache found to clear" -ForegroundColor Yellow
}

# Delete temporary AI cache
$aiCache = "SoloAdventureSystem.Web.UI\.aicache"
if (Test-Path $aiCache) {
    Write-Host "Deleting AI cache..." -ForegroundColor Yellow
    Remove-Item -Path $aiCache -Recurse -Force
    Write-Host "? AI cache cleared!" -ForegroundColor Green
}

Write-Host ""
Write-Host "? All caches cleared!" -ForegroundColor Green
Write-Host "You can now restart the Web UI and re-download the model." -ForegroundColor Cyan
```

Save this as `fix-model-cache.ps1` and run it.

### Option 2: Manual Cleanup

**Step 1: Delete Model Cache**
```
Location: %APPDATA%\SoloAdventureSystem\models
Full path: C:\Users\bruno\AppData\Roaming\SoloAdventureSystem\models
```

Delete this entire folder.

**Step 2: Delete AI Cache**
```
Location: SoloAdventureSystem.Web.UI\.aicache
```

Delete this folder if it exists.

**Step 3: Clear Temp Files (Optional)**
```
Location: %TEMP%
```

Delete any files starting with "SoloAdventureSystem" or model names.

### Option 3: Command Line

```powershell
# Delete model cache
Remove-Item -Path "$env:APPDATA\SoloAdventureSystem\models" -Recurse -Force

# Delete AI cache
Remove-Item -Path "C:\Users\bruno\source\repos\SoloAdventureSystem\SoloAdventureSystem.Web.UI\.aicache" -Recurse -Force

# Clear temp files (optional)
Remove-Item -Path "$env:TEMP\*SoloAdventureSystem*" -Recurse -Force
Remove-Item -Path "$env:TEMP\*gguf*" -Recurse -Force
```

## After Cleanup

### Restart the Web UI

1. **Stop the application** if it's running (Ctrl+C)
2. **Start fresh:**
   ```powershell
   cd SoloAdventureSystem.Web.UI
   dotnet run
   ```
3. **Open browser:** https://localhost:5001
4. **Click "Initialize AI"** again
5. **Wait for complete download** (don't cancel this time!)

### Download Tips

**Expected Download Sizes:**
- `phi-3-mini-q4`: ~2.3GB (5-10 minutes)
- `tinyllama-q4`: ~700MB (2-5 minutes)
- `llama-3.2-1b-q4`: ~2GB (4-8 minutes)

**Progress Indicators:**
- You'll see MB/s download speed
- Percentage complete
- ETA (estimated time remaining)

**What to Watch:**
- Progress should steadily increase
- Speed should be 1-20 MB/s (depending on internet)
- Don't close browser or stop app until 100%

### Use Faster Model

If download is too slow, switch to TinyLlama (smallest/fastest):

**Edit:** `SoloAdventureSystem.Web.UI\appsettings.json`
```json
{
  "AI": {
    "LLamaModelKey": "tinyllama-q4",  // Changed from phi-3-mini-q4
    "UseGPU": true
  }
}
```

Restart the app after changing this.

## Verify It's Fixed

After cleanup and restart:

1. ? No error messages on app startup
2. ? "Initialize AI" button appears
3. ? Download starts from 0%
4. ? Progress increases steadily
5. ? Completes at 100%
6. ? Shows "AI Model Ready!"

## Still Having Issues?

### Check Disk Space
```powershell
Get-PSDrive C | Select-Object Used,Free
```

Need at least 3-5GB free.

### Check Internet Connection
```powershell
Test-NetConnection -ComputerName huggingface.co -Port 443
```

Should show "TcpTestSucceeded : True"

### Check Firewall
Temporarily disable firewall/antivirus to test if they're blocking the download.

### Try Different Model
Start with smallest model first:
1. TinyLlama (~700MB) - easiest to download
2. Llama-3.2 (~2GB) - medium size
3. Phi-3 (~2.3GB) - largest, best quality

## Prevention

To avoid this in the future:
- ? Don't cancel mid-download
- ? Ensure stable internet
- ? Have enough disk space
- ? Let it complete to 100%
- ? Wait for "AI Model Ready!" message

## Quick Fix Command

Run this single command to fix everything:

```powershell
Remove-Item -Path "$env:APPDATA\SoloAdventureSystem" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item -Path "C:\Users\bruno\source\repos\SoloAdventureSystem\SoloAdventureSystem.Web.UI\.aicache" -Recurse -Force -ErrorAction SilentlyContinue; Write-Host "? Caches cleared! Restart the app." -ForegroundColor Green
```

---

## Ready to Try Again?

After running the fix:

```powershell
cd C:\Users\bruno\source\repos\SoloAdventureSystem\SoloAdventureSystem.Web.UI
dotnet run
```

Then navigate to https://localhost:5001 and click "Initialize AI" again!

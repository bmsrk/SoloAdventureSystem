# ?? IMMEDIATE FIX - Run This Now!

## Quick Fix (Copy & Paste)

Open PowerShell as Administrator and run:

```powershell
cd C:\Users\bruno\source\repos\SoloAdventureSystem
.\fix-model-cache.ps1
```

This will automatically:
- ? Delete corrupted model files
- ? Clear AI cache
- ? Remove temp files
- ? Tell you what was fixed

## After Running the Fix

### 1. Restart the Web UI
```powershell
cd SoloAdventureSystem.Web.UI
dotnet run
```

### 2. Open Browser
Navigate to: **https://localhost:5001**

### 3. Initialize AI Again
- Click **"AI World Generator"**
- Click **"?? Initialize AI"**
- **DON'T CANCEL THIS TIME!**
- Wait for 100% complete

## Alternative: Use Smaller Model

If downloads keep failing, switch to the smallest model:

### Edit `SoloAdventureSystem.Web.UI\appsettings.json`

Find this section:
```json
{
  "AI": {
    "LLamaModelKey": "phi-3-mini-q4",  // ? Change this line
    "UseGPU": true
  }
}
```

Change to:
```json
{
  "AI": {
    "LLamaModelKey": "tinyllama-q4",  // ? Smallest/fastest model
    "UseGPU": true
  }
}
```

**Model Comparison:**
- `tinyllama-q4` - **~700MB** (2-5 min download) - **Use this if having issues**
- `llama-3.2-1b-q4` - ~2GB (4-8 min download)
- `phi-3-mini-q4` - ~2.3GB (5-10 min download) - Best quality

## Still Not Working?

### Check Disk Space
```powershell
Get-PSDrive C
```

Need at least **3-5GB free**.

### Check Internet
```powershell
Test-NetConnection huggingface.co -Port 443
```

Should show: `TcpTestSucceeded : True`

### Manual Cache Delete

If the script doesn't work, manually delete these folders:

1. **Model Cache:**
   ```
   C:\Users\bruno\AppData\Roaming\SoloAdventureSystem\models
   ```

2. **AI Cache:**
   ```
   C:\Users\bruno\source\repos\SoloAdventureSystem\SoloAdventureSystem.Web.UI\.aicache
   ```

3. **Temp Files:**
   - Open: `%TEMP%`
   - Delete files containing "gguf" or "SoloAdventureSystem"

## Watch Download Progress

When it's working correctly, you'll see:

```
Downloading model: 150/700 MB (21%) - 5.2 MB/s - ETA: 1m 45s
Downloading model: 350/700 MB (50%) - 5.8 MB/s - ETA: 1m 00s
Downloading model: 700/700 MB (100%) - 6.1 MB/s - ETA: 0s
? AI initialized successfully!
```

**Don't cancel** until you see "AI initialized successfully!"

## Prevention Tips

? **DO:**
- Wait for complete download
- Keep app running until "AI Model Ready!"
- Ensure stable internet
- Have 5GB+ free disk space

? **DON'T:**
- Cancel mid-download
- Close browser during download
- Close app during download
- Switch models mid-download

## Emergency Contact

If nothing works, you can:

1. **Use CPU-only mode** (slower but works):
   ```json
   "UseGPU": false
   ```

2. **Try different times** (maybe internet is congested)

3. **Use mobile hotspot** (if home internet is blocking)

---

## TL;DR - Just Fix It!

```powershell
# 1. Run fix script
cd C:\Users\bruno\source\repos\SoloAdventureSystem
.\fix-model-cache.ps1

# 2. Restart app
cd SoloAdventureSystem.Web.UI
dotnet run

# 3. Open https://localhost:5001
# 4. Click "Initialize AI"
# 5. Wait for 100% - DON'T CANCEL!
```

Done! ??

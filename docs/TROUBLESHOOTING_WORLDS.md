# Troubleshooting: Worlds Not Showing Up

If generated worlds aren't appearing in the game UI, follow these steps:

## Quick Diagnosis

1. **Run the diagnostic tool:**
   ```bash
   check-worlds.bat
   ```
   This will show you exactly where the system is looking for worlds.

2. **Check both projects are using the same path:**
   - Generator saves to: `{solution-root}/content/worlds/`
   - Game loads from: `{solution-root}/content/worlds/`

## Step-by-Step Troubleshooting

### Step 1: Verify World Generation

1. Run the AI World Generator:
   ```bash
   cd SoloAdventureSystem.AIWorldGenerator
   dotnet run
   ```

2. Generate a test world (use default settings and click "Generate")

3. **Look at the log output** - it will show the exact path where the file was saved:
   ```
   ? Saved: C:\...\SoloAdventureSystem\content\worlds\World_MyWorld_12345.zip
   ```

4. **Verify the file exists** at that exact path using File Explorer

### Step 2: Verify Game Loading

1. Run the Terminal GUI:
   ```bash
   cd SoloAdventureSystem.TerminalGUI.UI
   dotnet run
   ```

2. **Look at the startup output** - it shows:
   - Current directory
   - Solution root found
   - Worlds path being searched
   - Files found

3. The output should look like:
   ```
   ?? Current directory: C:\...\SoloAdventureSystem\SoloAdventureSystem.TerminalGUI.UI\bin\Debug\net10.0
      Checking: C:\...\SoloAdventureSystem\SoloAdventureSystem.TerminalGUI.UI\bin\Debug\net10.0
      Checking: C:\...\SoloAdventureSystem\SoloAdventureSystem.TerminalGUI.UI\bin\Debug
      Checking: C:\...\SoloAdventureSystem\SoloAdventureSystem.TerminalGUI.UI\bin
      Checking: C:\...\SoloAdventureSystem\SoloAdventureSystem.TerminalGUI.UI
      Checking: C:\...\SoloAdventureSystem
      ? Found solution file: SoloAdventureSystem.slnx
   ? Found solution root: C:\...\SoloAdventureSystem
   ?? Worlds path: C:\...\SoloAdventureSystem\content\worlds
   ? Directory exists
   ?? Files found:
      - World_MyWorld_12345.zip
   ```

### Step 3: Common Issues

#### Issue: "Could not find solution root"

**Cause:** The program can't find the `.sln` or `.slnx` file

**Solutions:**
- Make sure you're running from within the solution directory structure
- Check that `SoloAdventureSystem.sln` or `SoloAdventureSystem.slnx` exists in the root
- Try running from the project directories directly (not from bin/Debug)
- **Note:** Both old `.sln` and new `.slnx` solution formats are supported

#### Issue: "Directory exists but no .zip files found"

**Cause:** Worlds directory exists but is empty

**Solutions:**
- Generate worlds using the AI World Generator first
- Check that generation completed successfully (look for "? Saved:" message)
- Verify file permissions on the content/worlds directory

#### Issue: Paths don't match

**Cause:** Generator and game are looking in different locations

**Check:**
- Generator log shows: `Saved: C:\path\to\content\worlds\World_X.zip`
- Game log shows: `Worlds path: C:\path\to\content\worlds`
- **These should be identical!**

**Solution:** If they're different, one of the projects isn't finding the solution root correctly. Run from the project directories, not from bin/Debug.

### Step 4: Manual Verification

If automatic detection fails, you can manually verify:

1. Open File Explorer
2. Navigate to your solution root (where `SoloAdventureSystem.sln` or `SoloAdventureSystem.slnx` is)
3. Look for `content\worlds\` folder
4. Check if `.zip` files are there
5. Note the full path

Compare this with what the programs are reporting.

## Advanced: Running from Different Directories

The system searches up to 5 levels to find the solution root (looking for .sln or .slnx files):

```
bin/Debug/net10.0/      ? run from here (3 levels up to project)
  ?
bin/Debug/               ? or here (2 levels)
  ?
bin/                     ? or here (1 level)
  ?
SoloAdventureSystem.XXX/ ? or here (project directory)
  ?
SoloAdventureSystem/     ? solution root (target - contains .sln or .slnx)
```

**Recommendation:** Always run from the project directory for best results:
```bash
cd SoloAdventureSystem.AIWorldGenerator
dotnet run
```

## Still Not Working?

1. Check the console output for both programs - they now show detailed debugging info
2. Run `check-worlds.bat` to see the diagnostic output
3. Look at the file paths carefully - even small differences matter
4. Try generating a world and immediately checking if the file appears in File Explorer
5. Report the issue with:
   - Output from generator showing save path
   - Output from game showing search path
   - Screenshot of the content/worlds directory in File Explorer

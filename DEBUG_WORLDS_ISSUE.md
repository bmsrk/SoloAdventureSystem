# Debugging Steps - Worlds Not Appearing

## What I've Added

I've enhanced both the World Generator and Game UI with detailed debugging output to help identify why worlds aren't showing up.

## How to Diagnose the Issue

### 1. Generate a World (with debugging)

```bash
cd SoloAdventureSystem.AIWorldGenerator
dotnet run
```

When you generate a world, you should see in the log:
```
Saving to shared worlds directory: C:\...\content\worlds
? Saved: C:\...\content\worlds\World_MyWorld_12345.zip
? Location: C:\...\content\worlds
```

**Copy this path** - this is where the file was actually saved.

### 2. Try to Load the World (with debugging)

```bash
cd SoloAdventureSystem.TerminalGUI.UI
dotnet run
```

You should see detailed output like:
```
?? Current directory: C:\...\bin\Debug\net10.0
   Checking: C:\...\bin\Debug\net10.0
   Checking: C:\...\bin\Debug
   Checking: C:\...\bin
? Found solution root: C:\Users\bruno\source\repos\SoloAdventureSystem
?? Worlds path: C:\Users\bruno\source\repos\SoloAdventureSystem\content\worlds
? Directory exists
?? Files found:
   - World_MyWorld_12345.zip
? Found 1 world(s)
```

### 3. In the World Selector UI

The UI now shows:
- The path it's searching: `Searching: C:\...\content\worlds`
- Number of worlds found
- Better error messages if directory doesn't exist or is empty

### 4. Quick Diagnostic

Run this batch file to check the directory:
```bash
check-worlds.bat
```

## What to Check

Compare these three paths - **they must all be the same**:

1. **Where generator saved:** (from generator log output)
   ```
   ? Saved: ___________________________________________
   ```

2. **Where game is looking:** (from game startup output)
   ```
   ?? Worlds path: ___________________________________________
   ```

3. **What actually exists:** (from File Explorer)
   ```
   Navigate to this path and verify .zip files exist: ___________________________________________
   ```

## Next Steps

Please run both programs and share:

1. The full console output from the Generator when you create a world
2. The full console output from the Game when you try to load worlds
3. A screenshot or listing of what's in your `content\worlds` folder

This will help me identify exactly where the disconnect is happening!

## Common Issues

- **Different paths**: If the generator saves to one location and the game looks in another, we have a path detection issue
- **Files exist but not detected**: Permission or file extension issue
- **No solution root found**: Running from wrong directory or .sln file not found

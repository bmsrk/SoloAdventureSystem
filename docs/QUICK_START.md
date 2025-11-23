# Quick Start Guide

## How to Use Your Fixed Application

### 1. Launch the Application
- Press F5 in Visual Studio, or
- Run `SoloAdventureSystem.Terminal.UI.exe`

---

### 2. Generate Your First World

**From Main Menu ? Select "Generate New World"**

You'll see:
```
?? AI World Generator ?????????????????
?                                      ?
?  Name: [MyWorld        ]             ?
?  Seed: [12345          ]             ?
?                                      ?
?  Provider:                           ?
?    (•) Stub (fast)                   ?
?    ( ) LLamaSharp (AI)               ?
?                                      ?
?  Model: Stub (deterministic)         ?
?                                      ?
?  [Progress Bar]                      ?
?  [Log Window]                        ?
?                                      ?
?  [ > Generate ]  [ < Cancel ]        ?
????????????????????????????????????????
```

**Quick Generation (Recommended for Testing):**
1. Name: Enter any world name (e.g., "TestWorld")
2. Seed: Enter any number (e.g., 42)
3. Provider: Leave on "Stub (fast)"
4. Click "Generate"

**Result:** 
- World generated in ~2 seconds
- Saved to: `content/worlds/World_[Name]_[Seed].zip`
- Contains 5 rooms, 5 NPCs, 1 faction

---

### 3. Play Your World

**From Main Menu ? Select "Play Adventure"**

You'll see a list of generated worlds:
```
?? Select World ???????????????????????
?                                      ?
?  Choose Your Adventure               ?
?                                      ?
?  • World_Bruno_666.zip               ?
?  • World_MyWorld_12345.zip           ?
?  • World_TestWorld_42.zip            ?
?                                      ?
?  [ > Select ]  [ < Cancel ]          ?
????????????????????????????????????????
```

Select a world and press Enter.

---

### 4. Playing the Game

**Game Screen Layout:**
```
?? [World Name] ???????????????????????????????????????
? @ Chrome Node                        Turn: 0 ? Items: 0 ?
?                                                       ?
? ?? Description ?????????????? ?? Actions ?????????? ?
? ? Room description for...   ? ? • Go east         ? ?
? ?                           ? ? • Talk to Marcus  ? ?
? ? = People here:            ? ?                   ? ?
? ?   * Marcus Chen - ...     ? ?                   ? ?
? ?                           ? ? Up/Down - Enter   ? ?
? ????????????????????????????? ????????????????????? ?
?                                                       ?
? ?? Command ????????????????????????????????????????? ?
? ? [ I ] Inventory  [ L ] Look  [ Q ] Quit          ? ?
? ???????????????????????????????????????????????????? ?
?????????????????????????????????????????????????????????
```

**Controls:**
- **Arrow Keys** - Navigate actions list
- **Enter** - Select action
- **I** - View inventory
- **L** - Look around current room
- **Q** - Quit game (with confirmation)

**Actions:**
- **Go [direction]** - Move to adjacent room
- **Talk to [NPC]** - Converse with character
- **Take [item]** - Add item to inventory

---

### 5. World Generation Options

#### **Stub Provider (Fast - Recommended for Testing)**
- ? Generates in ~2 seconds
- ? Deterministic (same seed = same world)
- ? Great for testing/development
- ? Uses placeholder text instead of AI

#### **LLamaSharp Provider (Slow - Real AI)**
- ? Real AI-generated descriptions
- ? Atmospheric and immersive
- ? Unique content every time
- ? First run downloads 2GB model
- ? Generates in ~2-5 minutes
- ? Requires decent CPU/GPU

---

### 6. Understanding Seeds

**Seed = Random Number Generator Initialization**

Same seed + same settings = **identical world**

Examples:
- Seed `42` ? "Plasma Vault", "Cyber Node", NPC "Jin 'Volt' Park"
- Seed `666` ? "Chrome Node", "Neural Hub", NPC "Johnny 'Spike' Wong"
- Seed `42` (again) ? Same as first seed 42

**Use Cases:**
- **Share worlds**: "Try my world with seed 12345!"
- **Reproduce bugs**: "Seed 999 crashes at room 3"
- **Consistent testing**: Same seed for regression tests

---

### 7. World Location

Generated worlds are saved to:
```
<Solution Root>/content/worlds/
```

Example:
```
C:\Users\bruno\source\repos\SoloAdventureSystem\content\worlds\
  ?? World_Bruno_666.zip
  ?? World_MyWorld_12345.zip
  ?? World_TestWorld_42.zip
```

**Each ZIP contains:**
```
World_[Name]_[Seed].zip
  ?? world.json           (metadata)
  ?? rooms/               (room descriptions)
  ?   ?? room1.json
  ?   ?? room2.json
  ?   ?? ...
  ?? npcs/                (characters)
  ?   ?? npc1.json
  ?   ?? ...
  ?? factions/            (organizations)
  ?   ?? faction1.json
  ?? story/               (narrative nodes)
  ?   ?? story1.yaml
  ?? system/              (generation info)
      ?? seed.txt
      ?? generatorVersion.txt
```

---

### 8. Troubleshooting

#### World Generator Doesn't Open
? **FIXED** - Should work now!

#### Game Won't Start
? **FIXED** - Should work now!

#### "No worlds found"
- You need to generate at least one world first
- Go to Main Menu ? Generate New World

#### Slow Generation (LLamaSharp)
- First run downloads 2GB model (one time only)
- Subsequent runs use cached model
- Switch to "Stub" provider for instant generation

#### Can't See Text Clearly
- Terminal window might be too small
- Maximize the window for best experience
- Or adjust terminal font size

---

### 9. Quick Test Sequence

**5-Minute Test:**
1. ? Launch app (F5)
2. ? Main Menu appears
3. ? Select "Generate New World"
4. ? Leave defaults, click "Generate"
5. ? Wait ~2 seconds
6. ? See "World generated!" success message
7. ? Click OK
8. ? Select "Play Adventure"
9. ? Select your new world
10. ? Game starts, explore a few rooms
11. ? Try talking to an NPC
12. ? Press Q to quit

**If all 12 steps work ? Everything is perfect!** ?

---

### 10. Advanced: Using LLamaSharp AI

**For Real AI-Generated Content:**

1. Generate World ? Select "LLamaSharp (AI)"
2. Choose a model:
   - **Phi-3-mini Q4** (recommended, 2GB)
   - TinyLlama Q4 (smaller, 600MB)
   - Llama-3.2-1B Q4 (800MB)

3. Click Generate
4. **First Time Only**: Downloads model (~5-10 min)
5. Generation takes ~2-5 minutes
6. Result: Beautiful AI-written descriptions!

**Example AI Output:**
```
The data vault hums with cooling fans, bathed in flickering 
blue server lights. Rows of black terminals stretch into 
shadows, screens casting ghostly glows on polished floors. 
Ozone mingles with stale air. A red-blinking security 
terminal guards the entrance, surrounded by scattered 
maintenance tools.
```

vs **Stub Output:**
```
Room description for 'Generate a room description for:
Name: Data Vault
Theme: Cyberpunk
...'
```

---

## You're All Set!

The application is:
? Fully functional  
? Significantly improved  
? Ready to use  

Generate some worlds and explore! ???

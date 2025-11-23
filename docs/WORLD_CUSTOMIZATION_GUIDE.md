# ?? World Customization Guide

## Overview

The SoloAdventureSystem now supports **extensive world customization** through new parameters that allow you to create diverse atmospheres, settings, and narratives. These parameters guide the AI to generate cohesive, themed content.

## ?? New Customization Parameters

### 1. **Flavor** 
*Atmospheric mood of the world*

The emotional tone and atmosphere that permeates every aspect of your world.

**Examples:**
- `"Dark and gritty"` - Film noir, oppressive atmosphere
- `"Hopeful rebellion"` - Optimistic resistance against tyranny
- `"Neon-soaked mystery"` - Vibrant, enigmatic cyberpunk
- `"Decaying elegance"` - Post-apocalyptic grandeur
- `"Chaotic energy"` - Frenetic, unpredictable world
- `"Melancholic isolation"` - Lonely, introspective mood

**Impact:**
- Room descriptions emphasize this mood
- NPC personalities reflect the atmosphere
- Lore entries reinforce the feeling

---

### 2. **Description**
*Brief description of the world setting*

A concise summary of your world's core concept and setting.

**Examples:**
- `"A sprawling megacity where corporations rule"`
- `"Post-apocalyptic wasteland with scattered settlements"`
- `"Orbital space station teetering on the edge of civil war"`
- `"Underground city carved from ancient ruins"`
- `"Floating sky islands connected by cable cars"`
- `"Virtual reality prison where minds are trapped"`

**Impact:**
- Establishes the physical and social environment
- Guides room generation to fit the setting
- Informs NPC backgrounds and roles

---

### 3. **Main Plot Point**
*Central conflict or quest*

The primary narrative thread that connects all elements of your world.

**Examples:**
- `"Uncover the conspiracy behind recent disappearances"`
- `"A rogue AI threatens to destabilize the city"`
- `"Search for the legendary artifact that can save humanity"`
- `"Stop the rival factions from triggering a war"`
- `"Escape the simulation before your mind is deleted"`
- `"Prevent corporate assassins from eliminating witnesses"`

**Impact:**
- NPCs may reference or be involved in the plot
- Lore entries provide backstory and context
- Story nodes tie into the main conflict
- Rooms may contain plot-relevant details

---

### 4. **Time Period**
*When the story takes place*

The temporal setting that influences technology, culture, and aesthetics.

**Examples:**
- `"Near future (2077)"` - Recognizable but advanced
- `"Far future (2400s)"` - Alien and speculative
- `"Alternative 1980s"` - Retro-futuristic aesthetic
- `"Post-collapse (Year 47)"` - After societal breakdown
- `"Pre-singularity"` - On the cusp of AI transcendence
- `"Eternal present"` - Time is meaningless or cyclical

**Impact:**
- Technology levels in descriptions
- Cultural references and slang
- Visual aesthetics and architecture
- NPC backgrounds and memories

---

### 5. **Power Structure**
*Dominant factions and hierarchies*

The political and social forces that shape your world.

**Examples:**
- `"Megacorps vs. Street Gangs"`
- `"AI Overlords and Human Rebels"`
- `"Noble houses vying for control"`
- `"Hacker collectives vs. Government"`
- `"Religious orders and secular scientists"`
- `"Corporate oligarchy with underground resistance"`

**Impact:**
- Faction generation aligns with power dynamics
- NPCs represent different power groups
- Conflicts and tensions feel organic
- World lore explains the power balance

---

## ?? Example Configurations

### Example 1: Classic Cyberpunk
```
Name: "Neon Nights"
Seed: 42069
Flavor: "Dark and gritty with neon highlights"
Description: "A sprawling megacity where corporations own everything"
Main Plot: "Expose the corporation covering up illegal AI experiments"
Time Period: "2084"
Power Structure: "Megacorporations, hackers, and street gangs"
Regions: 10
```

**Result:** A noir cyberpunk world with corporate intrigue, underground hackers, and neon-lit streets.

---

### Example 2: Post-Apocalyptic Hope
```
Name: "Last Light"
Seed: 12345
Flavor: "Melancholic but hopeful"
Description: "Scattered communities rebuilding after the collapse"
Main Plot: "Unite the settlements against the warlord threat"
Time Period: "47 years after the Fall"
Power Structure: "Tribal councils and roaming warlords"
Regions: 8
```

**Result:** A post-apocalyptic world focused on survival and community, with echoes of the old world.

---

### Example 3: Space Station Thriller
```
Name: "Station Omega"
Seed: 99999
Flavor: "Claustrophobic paranoia"
Description: "Isolated space station far from Earth"
Main Plot: "Find the saboteur before life support fails"
Time Period: "2156 - Deep space era"
Power Structure: "Station administration and rival departments"
Regions: 6
```

**Result:** A tense, confined setting where every room matters and trust is scarce.

---

### Example 4: Virtual Reality Prison
```
Name: "The Construct"
Seed: 77777
Flavor: "Surreal and disorienting"
Description: "Digital prison where minds are trapped"
Main Plot: "Discover the glitch that reveals the truth"
Time Period: "Timeless virtual space"
Power Structure: "AI wardens and rebel hackers"
Regions: 12
```

**Result:** A mind-bending digital world where reality is malleable.

---

### Example 5: Retro-Futuristic Mystery
```
Name: "Chrome City 1985"
Seed: 80085
Flavor: "Neon-soaked noir mystery"
Description: "Alternative 1980s where AI emerged early"
Main Plot: "Solve the murder of a famous scientist"
Time Period: "Alternative timeline 1985"
Power Structure: "Tech corporations and detective agencies"
Regions: 7
```

**Result:** An 80s aesthetic with futuristic AI, blending retro and sci-fi.

---

## ?? Tips for Best Results

### 1. **Be Specific**
- ? "A city" ? ? "A neon-lit megacity built on ancient ruins"
- ? "Find something" ? ? "Recover the stolen neural interface prototype"

### 2. **Stay Consistent**
All parameters should support the same vision:
- If Flavor is "hopeful," avoid "dystopian nightmare" in Description
- Match Time Period to your technology level
- Align Power Structure with your plot

### 3. **Use Evocative Language**
- "Neon-soaked" is better than "bright"
- "Crumbling" is better than "old"
- "Teetering on collapse" is better than "unstable"

### 4. **Think About Contrast**
Create interesting tensions:
- Hopeful flavor in a dark setting
- High-tech world with low social progress
- Beautiful architecture with corrupt power

### 5. **Let the AI Surprise You**
- Don't over-specify every detail
- Give the AI room to interpret creatively
- The same parameters + different seeds = different stories

---

## ?? How It Works

### Prompt Integration
All your custom parameters are woven into the AI prompts:

**Room Generation:**
```
Name: Neural Nexus
Setting: A sprawling megacity where corporations rule
Flavor: Dark and gritty with neon highlights
Time Period: 2084
Context: Expose the corporation covering up illegal AI experiments
```

**NPC Generation:**
```
Name: Marcus Chen
Setting: Cyberpunk - A sprawling megacity where corporations rule
Faction: Chrome Syndicate
Flavor: Dark and gritty with neon highlights
Power Structure: Megacorporations, hackers, and street gangs
Plot Context: Expose the corporation covering up illegal AI experiments
```

The AI uses these parameters to create **cohesive, themed content** that feels like it belongs in the same world.

---

## ?? Using in the UI

### Terminal UI
1. Launch the World Generator
2. Fill in the basic fields (Name, Seed)
3. **Customize the new fields:**
   - Flavor
   - Setting
   - Main Plot
   - Time Period
   - Regions
4. Select your AI model
5. Click Generate

### CLI
```bash
# Coming soon: CLI support for custom parameters
dotnet run -- generate \
  --name "MyWorld" \
  --flavor "Dark and mysterious" \
  --description "Cyberpunk megacity" \
  --plot "Uncover the conspiracy" \
  --time-period "2077"
```

---

## ?? Parameter Impact Matrix

| Parameter | Rooms | NPCs | Factions | Lore | Story Nodes |
|-----------|-------|------|----------|------|-------------|
| **Flavor** | ??? | ?? | ?? | ?? | ? |
| **Description** | ??? | ??? | ??? | ?? | ?? |
| **Main Plot** | ? | ??? | ?? | ??? | ??? |
| **Time Period** | ?? | ?? | ? | ??? | ? |
| **Power Structure** | ? | ??? | ??? | ?? | ? |

? = Minor influence  
?? = Moderate influence  
??? = Major influence

---

## ?? Genre Examples

### Cyberpunk
- **Flavor:** "Neon-soaked and dystopian"
- **Description:** "Megacity ruled by AI corporations"
- **Plot:** "Stop the rogue AI uprising"
- **Time:** "2089"
- **Power:** "Megacorps vs. hackers"

### Fantasy
- **Flavor:** "Epic and mythical"
- **Description:** "Kingdom on the brink of war"
- **Plot:** "Find the legendary sword"
- **Time:** "Age of Dragons"
- **Power:** "Noble houses and wizards"

### Horror
- **Flavor:** "Oppressive dread"
- **Description:** "Abandoned space station"
- **Plot:** "Survive the unknown threat"
- **Time:** "Far future isolation"
- **Power:** "Survivors vs. the unknown"

### Post-Apocalyptic
- **Flavor:** "Desperate but hopeful"
- **Description:** "Wasteland with scattered settlements"
- **Plot:** "Unite against the raiders"
- **Time:** "50 years after collapse"
- **Power:** "Tribal councils and warlords"

### Noir
- **Flavor:** "Dark and morally gray"
- **Description:** "Crime-ridden city of secrets"
- **Plot:** "Solve the murder conspiracy"
- **Time:** "1940s but with AI"
- **Power:** "Mob families and corrupt police"

---

## ?? Next Steps

1. **Experiment!** Try different combinations
2. **Compare Results:** Same parameters, different seeds
3. **Iterate:** Refine based on what you like
4. **Share:** Post your best configurations!
5. **Mix Genres:** Create unique hybrid worlds

---

## ?? Notes

- All parameters are **optional** - defaults are provided
- Parameters guide the AI but don't strictly control it
- Different models may interpret parameters differently
- Longer generations (more regions) give more opportunities for the theme to shine
- The **seed** ensures reproducibility - same params + seed = same world

---

**Happy World Building! ???**

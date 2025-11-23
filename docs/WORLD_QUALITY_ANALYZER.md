# ?? World Quality Analyzer - User Guide

## Overview

The ValidationTool now includes a **World Quality Analyzer** that tests already-generated worlds to validate the quality of AI-generated content. This helps you identify which worlds are high quality and which may need regeneration.

---

## ?? Quick Start

### Analyze All Worlds
```bash
cd SoloAdventureSystem.ValidationTool
dotnet run -- analyze
```

### Analyze Specific World
```bash
dotnet run -- analyze World_MyWorld_12345.zip
```

### Analyze World by Path
```bash
dotnet run -- analyze "C:\path\to\world.zip"
```

---

## ?? What It Checks

The analyzer validates quality metrics for each content type:

### ?? Room Descriptions
- ? **Length**: 100-500 characters
- ? **Sentence count**: 2-5 sentences (target: 3)
- ? **No vague language**: Checks for "some", "maybe", "things"
- ? **Has colors**: Blue, red, neon, etc.
- ? **Has sensory details**: Sounds, smells, textures
- ? **Not empty**: Actually has content

### ?? NPC Biographies
- ? **Length**: 80-400 characters
- ? **Sentence count**: 1-4 sentences (target: 2)
- ? **No vague language**: Specific details only
- ? **Has trait/quirk**: Secrets, implants, scars, etc.
- ? **Has role**: Background and motivation
- ? **Not empty**: Actually has content

### ?? Faction Descriptions
- ? **Length**: 100-500 characters
- ? **Sentence count**: 2-5 sentences (target: 3)
- ? **No vague language**: Concrete details
- ? **Has goals**: Fight, control, protect, etc.
- ? **Has conflict**: Enemies, wars, battles
- ? **Not empty**: Actually has content

---

## ?? Scoring System

### Individual Content Scores
Each room/NPC/faction gets a score out of 100 based on how many quality checks it passes:

```
Score = (Checks Passed / Total Checks) × 100
```

### Overall World Score
The world score is the average of all content category scores:

```
World Score = Average(Room Score, NPC Score, Faction Score)
```

### Grading Scale
- **A (90-100)**: Excellent quality
- **B (80-89)**: Good quality
- **C (70-79)**: Acceptable quality
- **D (60-69)**: Poor quality
- **F (<60)**: Failed - should regenerate

---

## ?? Example Output

### Single World Analysis
```
????????????????????????????????????????????????????????????
?  ?? World Quality Analyzer                               ?
????????????????????????????????????????????????????????????

?? Analyzing: World_NeonCity_42069.zip
??????????????????????????????????????????????????????????

?? World: NeonCity
   Description: A sprawling megacity ruled by AI corporations
   Created: 2025-01-22 14:30

?? Analyzing 8 rooms...

   Average Score: 87.5/100
   Average Length: 245 chars
   Average Sentences: 3.2
   Quality: 8/8 passed (?70 score)

?? Analyzing 8 NPCs...

   Average Score: 82.3/100
   Average Length: 189 chars
   Average Sentences: 2.1
   Quality: 7/8 passed (?70 score)

   ??  Lowest: Random Citizen (65/100)
      Issues: HasTrait

??  Analyzing 1 factions...

   Average Score: 91.7/100
   Average Length: 267 chars
   Average Sentences: 3.0
   Quality: 1/1 passed (?70 score)

????????????????????????????????????????????????????????????
? Overall Quality Summary                                  ?
????????????????????????????????????????????????????????????

?? Overall Score: 87.2/100
   Rooms:    87.5/100 (8 items)
   NPCs:     82.3/100 (8 items)
   Factions: 91.7/100 (1 items)

   Grade: B (Good)

   ? This world meets high quality standards!
```

### Multiple Worlds Comparison
```
????????????????????????????????????????????????????????????
? Comparison Summary                                       ?
????????????????????????????????????????????????????????????

Worlds ranked by quality:

?? 1. World_NeonCity_42069
      Score: 87.2/100 (Grade: B)
      Rooms: 88 | NPCs: 82 | Factions: 92

?? 2. World_TestWorld_12345
      Score: 78.5/100 (Grade: C)
      Rooms: 75 | NPCs: 80 | Factions: 81

?? 3. World_OldWorld_99999
      Score: 65.3/100 (Grade: D)
      Rooms: 62 | NPCs: 68 | Factions: 66

? Best World: World_NeonCity_42069 (87.2/100)
?? Score Range: 21.9 points difference
?? Average Quality: 77.0/100
```

---

## ?? Use Cases

### 1. Quality Assurance
Before sharing or playing a world:
```bash
dotnet run -- analyze World_MyWorld_12345.zip
```

If score ? 80, it's high quality!

### 2. Compare Generations
Test different prompts/models:
```bash
# Generate with TinyLlama
dotnet run -- analyze

# Generate with Phi-3
dotnet run -- analyze

# Compare all worlds
```

See which model produces better quality.

### 3. Identify Issues
If a world scores low (<70):
- Check which content type is weak (rooms, NPCs, factions)
- Review the "Lowest" item to see what's wrong
- Regenerate with better prompts or different model

### 4. Batch Validation
After generating multiple worlds:
```bash
dotnet run -- analyze
```

Quickly see which worlds are keepers!

---

## ?? Interpreting Results

### High Room Score (?85)
? Room descriptions are:
- Specific with colors and details
- Include sensory information
- Well-structured (3 sentences)
- No vague language

### High NPC Score (?85)
? NPC bios are:
- Clear role and background
- Memorable traits or secrets
- Concise (2 sentences)
- Specific and engaging

### High Faction Score (?85)
? Faction descriptions are:
- Clear goals and ideology
- Defined conflicts
- Territory/influence specified
- Well-structured (3 sentences)

### Low Scores (<70)
?? Common issues:
- Too short or too long
- Vague language ("some", "maybe")
- Missing key elements (traits, colors, conflicts)
- Empty or generic content

**Solution:** Regenerate with better prompts or try different model

---

## ?? Advanced Usage

### Analyze Specific Categories
The tool analyzes all categories automatically, but you can focus on specific issues:

1. **If rooms are weak**: Check for colors and sensory details
2. **If NPCs are weak**: Ensure they have traits and roles
3. **If factions are weak**: Look for goals and conflicts

### Compare Models
Generate same world with different models, then compare:

```bash
# With TinyLlama
dotnet run -- analyze World_Test_TinyLlama.zip

# With Phi-3
dotnet run -- analyze World_Test_Phi3.zip
```

Which model scores higher?

### Track Quality Over Time
Analyze after each major change:
- New prompt templates
- Different parameters
- Model updates

---

## ?? Tips for Better Scores

### To Improve Room Scores:
- Use **flavor** field: "Dark and gritty with neon highlights"
- Specify **time period**: "2089"
- Include **setting** details: "Megacity ruled by AI"

### To Improve NPC Scores:
- Define **power structure**: "Corporations vs. Hackers"
- Set **main plot**: "Uncover the conspiracy..."
- Use **specific factions**: Not just "Faction1"

### To Improve Faction Scores:
- Clear **power dynamics**: "Control tech black markets"
- Define **conflicts**: "War with corporate security"
- Specify **territory**: "Operates from the Undercity"

---

## ?? Troubleshooting

### No Worlds Found
```
? No world files found in: C:\...\content\worlds
```

**Solution:** Generate a world first using the main UI

### World File Not Found
```
? World file not found: MyWorld.zip
```

**Solution:** Use full path or just filename if in worlds directory

### Extraction Error
```
? Error analyzing world: Invalid ZIP format
```

**Solution:** World file may be corrupted, try regenerating

---

## ?? File Structure

### World ZIP Contains:
```
World_Name_Seed.zip
??? world.json         # World metadata
??? rooms/
?   ??? room1.json
?   ??? room2.json
?   ??? ...
??? npcs/
?   ??? npc1.json
?   ??? npc2.json
?   ??? ...
??? factions/
    ??? faction1.json
```

All JSON files are analyzed automatically.

---

## ?? Quality Metrics Explained

### Why These Checks Matter

**Length Checks:**
- Too short = Probably failed to generate properly
- Too long = May be rambling or unfocused

**Sentence Counts:**
- Follows prompt template structure
- Consistent formatting

**Vague Language:**
- "Some things" vs "Black cables"
- Specificity = Quality

**Colors/Sensory:**
- Makes descriptions vivid
- Engages reader imagination

**Traits/Quirks:**
- Makes NPCs memorable
- Adds depth to characters

**Goals/Conflicts:**
- Gives factions purpose
- Creates narrative tension

---

## ?? Integration Ideas

### CI/CD Pipeline
```bash
# Generate world
dotnet run generate --name TestWorld

# Validate quality
dotnet run -- analyze World_TestWorld_*.zip
if [ $? -ne 0 ]; then
  echo "Quality check failed!"
  exit 1
fi
```

### Automated Testing
```bash
# Test all models
for model in tinyllama-q4 phi-3-mini-q4 llama-3.2-1b-q4; do
  dotnet run generate --model $model --name Test_$model
  dotnet run -- analyze World_Test_$model_*.zip
done
```

### Quality Reports
Redirect output to file for later review:
```bash
dotnet run -- analyze > quality_report.txt
```

---

## ? Conclusion

The World Quality Analyzer helps you:
- ? Validate world quality before playing
- ? Compare different generation approaches
- ? Identify and fix quality issues
- ? Ensure consistent output quality
- ? Track improvements over time

**Use it every time you generate a world to ensure high quality!**

---

## ?? Related Documentation

- [PROMPT_TESTING.md](PROMPT_TESTING.md) - Test prompts before generating
- [AI_QUALITY_IMPROVEMENTS.md](../../AI_QUALITY_IMPROVEMENTS.md) - Prompt optimization details
- [WORLD_CUSTOMIZATION_GUIDE.md](../../WORLD_CUSTOMIZATION_GUIDE.md) - Create better worlds

---

**Status:** ? **READY TO USE**

**Usage:**
```bash
dotnet run -- analyze                    # All worlds
dotnet run -- analyze MyWorld.zip        # Specific world
```

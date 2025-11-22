# DAEMON / MUDVision — AI World Generator (SoloAdventureSystem.ContentGenerator)

## Purpose
Create a .NET 10 console application named `SoloAdventureSystem.ContentGenerator` that produces fully pre-generated world packages for the MUDVision game. Each package is a `.zip` that the game (Blazor MUD) can drop into `/content/worlds` and immediately load.

The generator must:
- produce deterministic structure from a seed,
- use a local SLM adapter **only for flavor text**,
- optionally produce image prompt strings (and optionally render images if configured),
- output a zipped world package following the exact folder layout below.

---

## Output ZIP structure (mandatory)
SoloAdventureWorld_<name>_<seed>.zip
/world.json
/map/map.png (placeholder or generated)
/rooms/{roomId}.json
/factions/{factionId}.json
/npcs/{npcId}.json
/story/{nodeId}.yaml
/assets/images/{...}.png (optional)
/system/seed.txt
/system/generatorVersion.txt


Each `room` JSON must include:
- id, title, baseDescription, exits (direction -> roomId), items (ids), npcs (ids), visualPrompt (string), uiPosition {x,y}.

Each `story` node is YAML:
- id, title, text (multi-line `|`), choices[] { label, next, effects[] }.

---

## Requirements & constraints
1. **Language/Platform**: C# (.NET 10). Console app.
2. **Deterministic**: All structural generation (layout, ids, exits, region placement) must be deterministic and seeded. Provide `--seed` flag.
3. **SLM usage**: Implement a `LocalS lmAdapter` interface. The generator must call it for **flavor text only** (room descriptions, npc biographies, faction flavor, lore). The adapter is pluggable; provide a simple stub that returns deterministic placeholder text if no real model is configured.
4. **Image prompts**: for each room generate a `visualPrompt` string. Optionally call a local image generator adapter if `--render-images` flag is passed. If using images, place them in `/assets/images`.
5. **Validation**: after generation, verify presence of `world.json`, at least 3 rooms, at least 1 faction, and at least 1 story node. Fail with clear error if validation fails.
6. **Export**: Compress the output folder into `content/worlds/SoloAdventureWorld_<name>_<seed>.zip` using `System.IO.Compression.ZipFile`.
7. **CLI**: Support interactive wizard and non-interactive flags: `--name`, `--seed`, `--theme`, `--regions`, `--npc-density`, `--render-images`.
8. **No runtime SLM calls**: the generator runs offline (dev machine). The game runtime does not call SLM.
9. **Coding style**: modular, small classes, clear comments. Separate responsibilities (Generator, SLM Adapter, Image Adapter, Exporter, Validator).
10. **Sample asset**: include usage of this sample image as an example to copy into assets if rendering is disabled: `sandbox:/mnt/data/A_roadmap_infographic_for_the_"Daemon_RPG"_is_disp.png`
11. **Deliverables**:
    - Full project `SoloAdventureSystem.ContentGenerator` in the solution.
    - `Program.cs` with CLI wizard.
    - `IWorldGenerator` and an implementation `SeededWorldGenerator`.
    - `ILocalSLMAdapter` with a `StubSLMAdapter` implementation.
    - `IImageAdapter` with a `StubImageAdapter`.
    - Exporter/validator classes.
    - Example command lines in README.
    - Generate one sample world zip `SoloAdventureWorld_Test_12345.zip` and place it in `content/worlds/`.

---

## Behavior details (generator algorithm)
- Step 1: seed-based map grid generation (tile map or node graph). Use seed to place N rooms, ensure connectivity.
- Step 2: create factions and assign influence to regions deterministically.
- Step 3: place NPCs in rooms based on npc-density.
- Step 4: call SLM adapter to create:
  - room `baseDescription`
  - npc short bio (1–2 lines)
  - faction flavor paragraph
  - 3–5 lore entries (markdown)
- Step 5: generate story skeleton (start node + branching choices) and save as YAML story nodes. Choices should link to other nodes or to room actions.
- Step 6: create `visualPrompt` strings for rooms (concise prompts).
- Step 7: validate and zip.

---

## Example usage (non-interactive)
dotnet run -- --name "Cidade Cinza" --seed 12345 --theme "Urban Horror" --regions 8 --npc-density "medium" --render-images false


---

## Notes to implementer (strict)
- The SLM adapter must never decide game mechanics or modify generation seeds — only returns textual content based on the passed deterministic context.
- Provide unit-testable methods for core generation steps.
- Keep external dependencies minimal. The only allowed external package is `YamlDotNet` for YAML serialization.
- Make sure the output zip loads in the existing `WorldLoader` of MUDVision (match the fields previously defined for rooms, story nodes and world.json).

---

## Final output from agent
- All source files for `SoloAdventureSystem.ContentGenerator`
- A README showing how to run generator and how to set a real SLM adapter later
- One generated sample zip: `content/worlds/SoloAdventureWorld_Test_12345.zip`
- A short log of generation run showing seed and counts (rooms, npcs, factions, story nodes)

End of prompt.




using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.Engine.Rules;
using SoloAdventureSystem.ContentGenerator.Adapters;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Generates pregenerated dialogue/story nodes with skill checks.
/// This generator can call the LLM during generation to create richer choices
/// based on NPC personality and provided skill list. The runtime uses pregenerated
/// results only (no LLM calls at runtime).
/// </summary>
public class DialogueGenerator
{
    private readonly WorldGenerationContext _context;
    private readonly ILogger<DialogueGenerator>? _logger;
    private readonly ILocalSLMAdapter? _slm;

    public DialogueGenerator(WorldGenerationContext context, ILogger<DialogueGenerator>? logger = null, ILocalSLMAdapter? slm = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
        _slm = slm; // optional - allow deterministic fallback when null
    }

    /// <summary>
    /// Skill list influences what kinds of social options are requested from the LLM.
    /// </summary>
    public List<StoryNodeModel> Generate(Func<ContentGenerator.Models.NpcModel, List<Skill>?>? skillSelector = null)
    {
        var nodes = new List<StoryNodeModel>();

        if (_context.Npcs == null || _context.Npcs.Count == 0)
        {
            _logger?.LogInformation("No NPCs available for dialogue generation");
            return nodes;
        }

        // Use per-run randomness via context.Random; deterministic seed exposure removed
        var seedBase = _context.Random.Next();

        for (int i = 0; i < _context.Npcs.Count; i++)
        {
            var npc = _context.Npcs[i];
            var room = _context.Rooms.Count > i ? _context.Rooms[i] : (_context.Rooms.Count > 0 ? _context.Rooms[0] : null);
            var faction = _context.Factions.Count > 0 ? _context.Factions[0] : null;

            var seed = seedBase + i;
            var rand = new Random(seed);

            var npcName = string.IsNullOrWhiteSpace(npc.Name) ? $"npc{i + 1}" : npc.Name;
            var factionName = faction?.Name ?? "Unknown faction";
            var roomName = room?.Title ?? "the place";

            // Determine skill list for this NPC using selector (if provided)
            var npcSkills = skillSelector?.Invoke(npc);

            // If an SLM adapter is available, ask it to produce dialogue choices tailored to NPC
            List<StoryChoice>? generatedChoices = null;
            string rawJson = string.Empty;
            if (_slm != null)
            {
                try
                {
                    var prompt = BuildDialoguePrompt(npc, npcName, factionName, roomName, npcSkills);
                    var json = _slm.GenerateDialogue(prompt, seed);
                    rawJson = json ?? string.Empty;
                    // Expect JSON array of choices with label, nextSuffix, skillCheck (optional), effects
                    generatedChoices = ParseChoicesFromJson(json, npc.Id);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "SLM failed to generate dialogue for {Npc}, falling back to deterministic choices", npcName);
                    generatedChoices = null;
                }

                // If parsing failed (empty/malformed output), attempt retries with simpler prompts
                if (generatedChoices == null)
                {
                    const int maxRetries = 3;
                    for (int attempt = 1; attempt <= maxRetries && generatedChoices == null; attempt++)
                    {
                        try
                        {
                            var altPrompt = BuildShortDialoguePrompt(npcName, npc.Description, factionName, roomName, npcSkills, attempt);
                            var altJson = _slm.GenerateDialogue(altPrompt, seed + attempt);
                            // append attempt output to rawJson for debugging
                            rawJson += "\n---retry-" + attempt + "---\n" + (altJson ?? string.Empty);
                            generatedChoices = ParseChoicesFromJson(altJson, npc.Id);
                            if (generatedChoices != null)
                            {
                                _logger?.LogInformation("SLM produced valid dialogue on retry {Attempt} for {Npc}", attempt, npcName);
                                break;
                            }
                        }
                        catch (Exception rex)
                        {
                            _logger?.LogWarning(rex, "Retry {Attempt} failed for {Npc}", attempt, npcName);
                        }
                    }
                }
            }

            // Fallback deterministic choices if no generated choices
            if (generatedChoices == null)
            {
                generatedChoices = new List<StoryChoice>
                {
                    new StoryChoice
                    {
                        Label = "Try to persuade them (appeal to empathy)",
                        Next = $"dialogue_{npc.Id}_reveal",
                        Effects = new List<string> { $"Daemon:{npc.Id}|Loyalty:1" },
                        SkillCheck = new SkillCheckModel
                        {
                            Attribute = GameAttribute.Presence,
                            Skill = Skill.Social,
                            TargetNumber = 12,
                            OpponentNpcId = npc.Id
                        }
                    },
                    new StoryChoice
                    {
                        Label = "Threaten them (intimidate)",
                        Next = $"dialogue_{npc.Id}_hostile",
                        Effects = new List<string> { $"Daemon:{npc.Id}|Rage:2" },
                        SkillCheck = new SkillCheckModel
                        {
                            Attribute = GameAttribute.Body,
                            Skill = Skill.Combat,
                            TargetNumber = 11,
                            OpponentNpcId = npc.Id
                        }
                    },
                    new StoryChoice
                    {
                        Label = "Leave them alone",
                        Next = $"dialogue_{npc.Id}_leave",
                        Effects = new List<string>()
                    }
                };
            }

            // If generated choices exist, ensure the first choice does not immediately end the dialogue when possible
            if (generatedChoices != null && generatedChoices.Count > 1)
            {
                bool firstIsEnd = generatedChoices[0].Next != null && generatedChoices[0].Next.EndsWith("_end", StringComparison.OrdinalIgnoreCase);
                if (firstIsEnd)
                {
                    // find first non-end choice and move it to front
                    var idx = generatedChoices.FindIndex(1, c => !(c.Next != null && c.Next.EndsWith("_end", StringComparison.OrdinalIgnoreCase)));
                    if (idx >= 1)
                    {
                        var item = generatedChoices[idx];
                        generatedChoices.RemoveAt(idx);
                        generatedChoices.Insert(0, item);
                        _logger?.LogDebug("Reordered dialogue choices for NPC {Npc} to avoid immediate end on first option", npcName);
                    }
                }
            }

            // Build nodes using choices (guarded by npc id)
            var intro = new StoryNodeModel
            {
                Id = $"dialogue_{npc.Id}_intro",
                Title = "A Tense Meeting",
                Text = $"You encounter {npcName}, a representative of {factionName}, in {roomName}. They seem guarded but willing to speak about recent events: {_context.Options.MainPlotPoint}",
                OwnerNpcId = npc.Id,
                Choices = generatedChoices,
                RawGeneratorJson = rawJson
            };

            nodes.Add(intro);

            // Reveal/hostile/leave/end as before, using npc-specific ids
            var reveal = new StoryNodeModel
            {
                Id = $"dialogue_{npc.Id}_reveal",
                Title = "They Open Up",
                Text = $"{npcName} relaxes and shares a hint: 'There's a meeting at the old docks tonight. Someone from {factionName} will be there.'",
                OwnerNpcId = npc.Id,
                Choices = new List<StoryChoice>
                {
                    new StoryChoice { Label = "Thank them and leave", Next = $"dialogue_{npc.Id}_end", Effects = new List<string>{ "Flag:revealed_meeting:1" } },
                    new StoryChoice { Label = "Ask for more details", Next = $"dialogue_{npc.Id}_end", Effects = new List<string>{ "Flag:asked_more:1" } }
                }
            };
            nodes.Add(reveal);

            var hostile = new StoryNodeModel
            {
                Id = $"dialogue_{npc.Id}_hostile",
                Title = "Threat Escalates",
                Text = $"{npcName} narrows their eyes. The conversation ends badly; guards are alerted and you must flee.",
                OwnerNpcId = npc.Id,
                Choices = new List<StoryChoice>
                {
                    new StoryChoice { Label = "Flee", Next = $"dialogue_{npc.Id}_end", Effects = new List<string>{ "Flag:chased_by_guards:1" } }
                }
            };
            nodes.Add(hostile);

            var leave = new StoryNodeModel
            {
                Id = $"dialogue_{npc.Id}_leave",
                Title = "You Walk Away",
                Text = $"You decide not to press the issue. {npcName} watches you go, unreadable.",
                OwnerNpcId = npc.Id,
                Choices = new List<StoryChoice>
                {
                    new StoryChoice { Label = "Continue", Next = $"dialogue_{npc.Id}_end", Effects = new List<string>() }
                }
            };
            nodes.Add(leave);

            var end = new StoryNodeModel
            {
                Id = $"dialogue_{npc.Id}_end",
                Title = "End",
                Text = "The conversation ends.",
                OwnerNpcId = npc.Id,
                Choices = new List<StoryChoice>()
            };
            nodes.Add(end);
        }

        _logger?.LogInformation("Generated {Count} dialogue nodes for {NpcCount} NPCs", nodes.Count, _context.Npcs.Count);
        return nodes;
    }

    private string BuildDialoguePrompt(ContentGenerator.Models.NpcModel npc, string npcName, string factionName, string roomName, List<Skill>? skills)
    {
        // Create a compact JSON-style instruction so LLM outputs structured JSON
        var skillList = skills != null && skills.Count > 0 ? string.Join(",", skills) : "Social,Knowledge,Awareness,Will";

        var prompt = $@"You are a creative writer specialized in interactive NPC dialogue for a text-adventure game.\n"
            + $"NPC_NAME: {npcName}\n"
            + $"NPC_BIO: {npc.Description}\n"
            + $"FACTION: {factionName}\n"
            + $"LOCATION: {roomName}\n"
            + $"MOOD: {_context.Options.Flavor}\n"
            + $"PLOT: {_context.Options.MainPlotPoint}\n"
            + $"SKILLS: {skillList}\n"
            + "\nProduce a JSON array of 3 choices. Each choice must be an object with fields: label (string), nextSuffix (string, short id suffix), skill_check (optional object with attribute, skill, target_number), and effects (array of short effect strings).\n"
            + "Example output:\n[ { \"label\": \"Appeal to empathy\", \"nextSuffix\": \"reveal\", \"skill_check\": { \"attribute\": \"Presence\", \"skill\": \"Social\", \"target_number\": 12 }, \"effects\": [\"Daemon:npc1|Loyalty:1\"] } ]\n"
            + "Return ONLY the JSON array.";

        return prompt;
    }

    private string BuildShortDialoguePrompt(string npcName, string npcBio, string factionName, string roomName, List<Skill>? skills, int attempt = 1)
    {
        var skillList = skills != null && skills.Count > 0 ? string.Join(",", skills) : "Social";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Produce a JSON array of 2 dialogue choices. Return ONLY the JSON array. No explanation.");
        sb.AppendLine($"NPC_NAME: {npcName}");
        if (!string.IsNullOrWhiteSpace(npcBio))
        {
            var bio = npcBio.Length > 150 ? npcBio.Substring(0, 150) + "..." : npcBio;
            sb.AppendLine($"NPC_BIO: {bio}");
        }
        sb.AppendLine($"LOCATION: {roomName}");
        sb.AppendLine($"FACTION: {factionName}");
        sb.AppendLine($"SKILLS: {skillList}");
        sb.AppendLine("Format exactly like this example (two choices):");
        sb.AppendLine("[ { \"label\": \"Ask politely\", \"nextSuffix\": \"reveal\", \"skill_check\": { \"attribute\": \"Presence\", \"skill\": \"Social\", \"target_number\": 12 }, \"effects\": [\"Daemon:npc1|Loyalty:1\"] }, { \"label\": \"Leave\", \"nextSuffix\": \"leave\", \"effects\": [] } ]");
        sb.AppendLine("Be brief and output strict JSON only. Use exactly the fields shown.");
        if (attempt > 1) sb.AppendLine("If you cannot produce 2 choices, produce 1 valid choice only.");
        return sb.ToString();
    }

    private List<StoryChoice>? ParseChoicesFromJson(string json, string npcId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            // Pre-clean common artifacts before attempting JSON extraction
            var cleanedJsonText = json ?? string.Empty;
            try
            {
                cleanedJsonText = System.Text.RegularExpressions.Regex.Replace(cleanedJsonText, "(?i)\\b#?TOON\\b", "");
                cleanedJsonText = System.Text.RegularExpressions.Regex.Replace(cleanedJsonText, "(?i)\\b#?ENDTOON\\b", "");
                cleanedJsonText = System.Text.RegularExpressions.Regex.Replace(cleanedJsonText, "#\\w+", "");
            }
            catch { }

            var trimmed = cleanedJsonText.Trim();
            var startIdx = trimmed.IndexOf('[');
            var endIdx = trimmed.LastIndexOf(']');
            if (startIdx >= 0 && endIdx > startIdx)
            {
                json = trimmed.Substring(startIdx, endIdx - startIdx + 1);
            }
            else
            {
                // Some models return a single object; wrap it to normalize to array
                if (trimmed.StartsWith("{") && !trimmed.StartsWith("["))
                {
                    json = "[" + trimmed + "]";
                }
            }

            // Try to deserialize; allow for fields named 'label' or 'text' or 'option'
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var choices = System.Text.Json.JsonSerializer.Deserialize<List<ChoiceDto>>(json, options);

            if (choices == null || choices.Count == 0) return null;

            var result = new List<StoryChoice>();
            foreach (var c in choices)
            {
                // Trim and validate label (accept several possible fields)
                var label = (c.label ?? c.text ?? c.option)?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(label))
                {
                    // skip empty labels
                    continue;
                }

                // Sanitize next suffix (allow alphanum and underscore)
                var nextCandidate = (c.nextSuffix ?? c.next ?? string.Empty).Trim();
                string next;
                if (string.IsNullOrEmpty(nextCandidate))
                {
                    next = $"dialogue_{npcId}_end";
                }
                else
                {
                    // If candidate looks like a full id (starts with dialogue_) use as-is
                    if (nextCandidate.StartsWith("dialogue_", StringComparison.OrdinalIgnoreCase))
                    {
                        next = System.Text.RegularExpressions.Regex.Replace(nextCandidate, "[^a-zA-Z0-9_\\-]", "").Trim();
                    }
                    else
                    {
                        // sanitize short suffix and prefix with npc id
                        var suffix = System.Text.RegularExpressions.Regex.Replace(nextCandidate, "[^a-zA-Z0-9_\\-]", "").Trim();
                        next = string.IsNullOrEmpty(suffix) ? $"dialogue_{npcId}_end" : $"dialogue_{npcId}_" + suffix;
                    }
                }

                // Skill check mapping with safety bounds
                SkillCheckModel? sc = null;
                if (c.skill_check != null)
                {
                    var attrStr = c.skill_check.attribute ?? "Soul";
                    var skillStr = c.skill_check.skill ?? "Social";
                    var tn = c.skill_check.target_number;
                    if (tn <= 0) tn = 10;
                    // clamp reasonable target numbers
                    tn = Math.Max(3, Math.Min(20, tn));

                    if (Enum.TryParse<GameAttribute>(attrStr, true, out var attr))
                    {
                        if (Enum.TryParse<Skill>(skillStr, true, out var sk))
                        {
                            sc = new SkillCheckModel
                            {
                                Attribute = attr,
                                Skill = sk,
                                TargetNumber = tn,
                                OpponentNpcId = npcId
                            };
                        }
                    }
                }

                var effects = c.effects?.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToList() ?? new List<string>();

                result.Add(new StoryChoice
                {
                    Label = label,
                    Next = next,
                    Effects = effects,
                    SkillCheck = sc
                });
            }

            // If parsed result contains no valid choices, return null so caller falls back
            if (result.Count == 0) return null;

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse dialogue JSON");
            return null;
        }
    }

    private class ChoiceDto
    {
        // accept multiple possible field names that model might output
        public string? label { get; set; }
        public string? text { get; set; }
        public string? option { get; set; }
        public string? next { get; set; }
        public string? nextSuffix { get; set; }
        public SkillCheckDto? skill_check { get; set; }
        public List<string>? effects { get; set; }
    }

    private class SkillCheckDto
    {
        public string? attribute { get; set; }
        public string? skill { get; set; }
        public int target_number { get; set; }
    }
}

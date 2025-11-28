using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation
{
    /// <summary>
    /// V2 world generator producing a WorldV2 DTO.
    /// Uses an ILocalSLMAdapter for richer generated text and embraces LLM nondeterminism.
    /// </summary>
    public class WorldGeneratorV2
    {
        private readonly ILocalSLMAdapter _slm;
        private readonly ILogger<WorldGeneratorV2>? _logger;

        public WorldGeneratorV2(ILocalSLMAdapter slm, ILogger<WorldGeneratorV2>? logger = null)
        {
            _slm = slm ?? throw new ArgumentNullException(nameof(slm));
            _logger = logger;
        }

        public WorldV2 Generate(WorldGenerationOptions options, List<string> skills, int locationsCount = 3, int npcCount = 3)
        {
            if (skills == null || skills.Count == 0) throw new ArgumentException("skills must be provided and non-empty", nameof(skills));
            if (locationsCount < 1) locationsCount = 3;
            if (npcCount < 1) npcCount = 3;

            // Non-deterministic RNG to embrace LLM variation
            var rand = new Random();

            var world = new WorldV2();
            world.Meta.GeneratedAt = DateTime.UtcNow;
            world.Meta.Deterministic = false;
            world.Skills = new List<string>(skills);
            world.World.Name = options.Name;
            world.World.Theme = options.Theme;
            world.World.Tone = options.Flavor;
            world.World.Description = options.Description;

            // Create locations
            for (int i = 0; i < locationsCount; i++)
            {
                var lid = $"loc{i + 1}";
                var name = ProceduralNames.GenerateRoomName(rand.Next());

                // Use LLM to generate a room description when available
                string desc;
                try
                {
                    var prompt = $"Describe a location named '{name}' for a world called '{options.Name}'. Theme: {options.Theme}. Tone: {options.Flavor}. Provide 1-2 sentences.";
                    desc = _slm.GenerateRoomDescription(prompt) ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(desc)) desc = ProceduralNames.GenerateAtmosphere(rand.Next());
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "SLM GenerateRoomDescription failed; falling back to procedural atmosphere");
                    desc = ProceduralNames.GenerateAtmosphere(rand.Next());
                }

                world.Locations.Add(new LocationV2
                {
                    Id = lid,
                    Name = name,
                    Description = desc
                });
            }

            // Create simple factions (one or two)
            var factionCount = Math.Max(1, locationsCount / 2);
            for (int f = 0; f < factionCount; f++)
            {
                var fid = $"fac{f + 1}";
                string flavor;
                try
                {
                    var prompt = $"Provide a one-sentence faction flavor for a faction in world '{options.Name}' with theme '{options.Theme}' and tone '{options.Flavor}'.";
                    flavor = _slm.GenerateFactionFlavor(prompt) ?? ProceduralNames.GenerateFactionName(rand.Next());
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "SLM GenerateFactionFlavor failed; falling back to procedural name");
                    flavor = ProceduralNames.GenerateFactionName(rand.Next());
                }

                world.Factions.Add(new FactionV2
                {
                    Id = fid,
                    Name = ProceduralNames.GenerateFactionName(rand.Next()),
                    Description = flavor
                });
            }

            // Create secrets pool
            for (int s = 0; s < 5; s++)
            {
                var sid = $"sec{s + 1}";
                string text;
                try
                {
                    var prompt = $"Generate a short in-world secret or clue for '{options.Name}'. Keep it concise (one sentence). Index:{s}";
                    text = _slm.GenerateRaw(prompt) ?? (s % 2 == 0
                        ? "A hidden ledger lists illicit transfers between a corporation and a shadow group."
                        : "An identity chip contains false credentials tied to a missing person.");
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "SLM GenerateRaw failed; using fallback secret text");
                    text = s % 2 == 0
                        ? "A hidden ledger lists illicit transfers between a corporation and a shadow group."
                        : "An identity chip contains false credentials tied to a missing person.";
                }

                world.Secrets.Add(new SecretV2
                {
                    Id = sid,
                    Text = text
                });
            }

            // Create NPCs
            for (int n = 0; n < npcCount; n++)
            {
                var nid = $"npc{n + 1}";
                var name = ProceduralNames.GenerateNpcName(rand.Next());
                var traits = new List<string>();
                // trait selection now random
                var traitPool = new[] { "curious", "guarded", "talkative", "suspicious", "helpful", "pragmatic" };
                traits.Add(traitPool[rand.Next(traitPool.Length)]);
                traits.Add(traitPool[rand.Next(traitPool.Length)]);

                var relation = rand.Next(3) switch { 0 => "ally", 1 => "neutral", 2 => "rival", _ => "neutral" };
                var trust = rand.Next(0, 11);

                // assign a secret to some NPCs randomly
                var secrets = new List<string>();
                if (rand.Next(2) == 0 && world.Secrets.Count > 0)
                {
                    secrets.Add(world.Secrets[rand.Next(world.Secrets.Count)].Id);
                }

                var knowledgeAreas = new List<string>();
                if (rand.Next(2) == 0)
                {
                    knowledgeAreas.Add("local_history");
                }

                // Use LLM to generate a short bio
                string bio = string.Empty;
                try
                {
                    var prompt = $"Produce a 1-2 sentence bio for a character named '{name}' in world '{options.Name}'. Mention one notable trait and role.\nReturn only the bio.";
                    bio = _slm.GenerateNpcBio(prompt) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "SLM GenerateNpcBio failed; falling back to default bio");
                    bio = "A local resident with a complicated past.";
                }

                if (string.IsNullOrWhiteSpace(bio)) bio = "A local resident with a complicated past.";

                world.Npcs.Add(new NpcV2
                {
                    Id = nid,
                    Name = name,
                    PersonalityTraits = traits,
                    RelationToPlayer = relation,
                    Trust = trust,
                    Secrets = secrets,
                    KnowledgeAreas = knowledgeAreas
                });
            }

            // Create scenes (at least 3)
            for (int sc = 0; sc < 3; sc++)
            {
                var sid = $"scene{sc + 1}";
                var loc = world.Locations.Count > 0 ? world.Locations[rand.Next(world.Locations.Count)] : new LocationV2 { Id = "loc1", Name = "Unknown", Description = "" };
                string sceneDesc;
                try
                {
                    var prompt = $"Write a short scene description set at '{loc.Name}' in world '{options.Name}'. One-two sentences, hint at tension and possible choices.";
                    sceneDesc = _slm.GenerateDialogue(prompt) ?? $"At {loc.Name}: {loc.Description} A tense encounter awaits.";
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "SLM GenerateDialogue failed; falling back to templated scene description");
                    sceneDesc = $"At {loc.Name}: {loc.Description} A tense encounter awaits.";
                }

                var scene = new SceneV2
                {
                    Id = sid,
                    Description = sceneDesc
                };

                // Add 2 actions per scene
                for (int a = 0; a < 2; a++)
                {
                    var skillChoice = world.Skills[rand.Next(world.Skills.Count)];
                    var actionType = "interaction";
                    if (skillChoice.ToLowerInvariant().Contains("teach") || skillChoice.ToLowerInvariant().Contains("math") || skillChoice.ToLowerInvariant().Contains("history"))
                        actionType = "educational_check";
                    else
                        actionType = "social_check";

                    var successUnlocks = new List<string>();
                    if (rand.Next(3) == 0 && world.Secrets.Count > 0)
                    {
                        successUnlocks.Add(world.Secrets[rand.Next(world.Secrets.Count)].Id);
                    }

                    var actorNpc = world.Npcs.Count > 0 ? world.Npcs[rand.Next(world.Npcs.Count)] : new NpcV2 { Id = "npc1", Name = "Unknown" };

                    // Use LLM to craft success/failure flavor when possible
                    string successText;
                    string failureText;
                    try
                    {
                        var sPrompt = $"Write a short success outcome for using skill '{skillChoice}' on NPC '{actorNpc.Name}' in world '{options.Name}'. One sentence.";
                        successText = _slm.GenerateRaw(sPrompt) ?? $"You succeed using {skillChoice}, {actorNpc.Name} responds favorably.";

                        var fPrompt = $"Write a short failure outcome for failing skill '{skillChoice}' against NPC '{actorNpc.Name}' in world '{options.Name}'. One sentence.";
                        failureText = _slm.GenerateRaw(fPrompt) ?? $"You fail the {skillChoice} check and {actorNpc.Name} is unimpressed.";
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "SLM GenerateRaw for outcomes failed; using templated outcomes");
                        successText = $"You succeed using {skillChoice}, {actorNpc.Name} responds favorably.";
                        failureText = $"You fail the {skillChoice} check and {actorNpc.Name} is unimpressed.";
                    }

                    var action = new ActionV2
                    {
                        Type = actionType,
                        Skill = skillChoice,
                        Difficulty = 5 + rand.Next(5),
                        SuccessOutcome = new OutcomeV2
                        {
                            Text = successText,
                            ReputationChange = 1,
                            TrustChange = 1,
                            UnlockSecrets = successUnlocks
                        },
                        FailureOutcome = new OutcomeV2
                        {
                            Text = failureText,
                            ReputationChange = -1,
                            TrustChange = -1
                        }
                    };

                    scene.Actions.Add(action);
                }

                // Scene-level twist references
                if (rand.Next(2) == 0 && world.Secrets.Count > 0)
                {
                    scene.SecretsAndTwists.Add(world.Secrets[rand.Next(world.Secrets.Count)].Id);
                }

                world.Scenes.Add(scene);
            }

            // Create endings (simple presets)
            world.Endings.Add(new EndingV2
            {
                Name = "Expose Conspiracy",
                Conditions = new EndingConditions
                {
                    ReputationMin = 3,
                    RequiredAllies = world.Npcs.Where(n => n.RelationToPlayer == "ally").Select(n => n.Id).ToList(),
                    SecretsRequired = new List<string> { world.Secrets.FirstOrDefault()?.Id ?? string.Empty }
                },
                Text = "You reveal the ledger and topple the corrupt directors, becoming a folk hero."
            });

            world.Endings.Add(new EndingV2
            {
                Name = "Cover Up",
                Conditions = new EndingConditions
                {
                    ReputationMin = -10,
                    RequiredAllies = new List<string>(),
                    SecretsRequired = new List<string>()
                },
                Text = "You keep the secret and prosper quietly in the shadows."
            });

            return world;
        }
    }
}

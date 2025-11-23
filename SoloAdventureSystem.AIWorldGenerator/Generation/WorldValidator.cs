using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;

namespace SoloAdventureSystem.ContentGenerator
{
    /// <summary>
    /// Validation result for world quality checks.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public QualityMetrics Metrics { get; set; } = new();
    }

    /// <summary>
    /// Quality metrics for generated content.
    /// </summary>
    public class QualityMetrics
    {
        public int RoomQualityScore { get; set; }
        public int NpcQualityScore { get; set; }
        public int FactionQualityScore { get; set; }
        public int ConsistencyScore { get; set; }
        public int OverallScore { get; set; }
    }

    public class WorldValidator
    {
        private readonly ILocalSLMAdapter? _slm;
        private readonly ILogger<WorldValidator>? _logger;

        public WorldValidator(ILocalSLMAdapter? slm = null, ILogger<WorldValidator>? logger = null)
        {
            _slm = slm;
            _logger = logger;
        }

        /// <summary>
        /// Basic structural validation (always runs).
        /// Throws exception on critical structural issues.
        /// </summary>
        public void Validate(WorldGenerationResult result)
        {
            _logger?.LogInformation("?? Starting world validation...");

            if (result.World == null)
                throw new Exception("Missing world.json");
            if (result.Rooms == null || result.Rooms.Count < 3)
                throw new Exception("At least 3 rooms are required.");
            if (result.Factions == null || result.Factions.Count < 1)
                throw new Exception("At least 1 faction is required.");
            if (result.StoryNodes == null || result.StoryNodes.Count < 1)
                throw new Exception("At least 1 story node is required.");

            _logger?.LogInformation("? Basic structural validation passed");
        }

        /// <summary>
        /// Deep quality validation using LLM prompts (optional).
        /// Returns detailed validation results with scores and warnings.
        /// </summary>
        public ValidationResult ValidateQuality(WorldGenerationResult result, string theme)
        {
            _logger?.LogInformation("?? Starting LLM-based quality validation...");

            var validationResult = new ValidationResult { IsValid = true };

            try
            {
                // Run basic validation first
                Validate(result);

                // If no SLM adapter available, skip quality checks
                if (_slm == null)
                {
                    _logger?.LogWarning("?? No SLM adapter provided, skipping quality validation");
                    validationResult.Warnings.Add("Quality validation skipped (no LLM adapter available)");
                    return validationResult;
                }

                // Validate rooms
                _logger?.LogInformation("?? Validating room descriptions...");
                var roomScore = ValidateRooms(result.Rooms, theme, validationResult);
                validationResult.Metrics.RoomQualityScore = roomScore;

                // Validate NPCs
                _logger?.LogInformation("?? Validating NPC biographies...");
                var npcScore = ValidateNpcs(result.Npcs, theme, validationResult);
                validationResult.Metrics.NpcQualityScore = npcScore;

                // Validate Factions
                _logger?.LogInformation("??? Validating faction lore...");
                var factionScore = ValidateFactions(result.Factions, theme, validationResult);
                validationResult.Metrics.FactionQualityScore = factionScore;

                // Validate consistency
                _logger?.LogInformation("?? Validating world consistency...");
                var consistencyScore = ValidateConsistency(result, theme, validationResult);
                validationResult.Metrics.ConsistencyScore = consistencyScore;

                // Calculate overall score
                validationResult.Metrics.OverallScore = 
                    (roomScore + npcScore + factionScore + consistencyScore) / 4;

                _logger?.LogInformation("?? Quality Scores: Rooms={RoomScore}, NPCs={NpcScore}, Factions={FactionScore}, Consistency={ConsistencyScore}, Overall={OverallScore}",
                    roomScore, npcScore, factionScore, consistencyScore, validationResult.Metrics.OverallScore);

                // Mark as invalid if overall score is too low
                if (validationResult.Metrics.OverallScore < 50)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add($"Overall quality score too low: {validationResult.Metrics.OverallScore}/100");
                    _logger?.LogError("? World quality validation FAILED: Score too low");
                }
                else
                {
                    _logger?.LogInformation("? World quality validation PASSED");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "?? Quality validation failed with exception");
                validationResult.IsValid = false;
                validationResult.Errors.Add($"Validation exception: {ex.Message}");
            }

            return validationResult;
        }

        private int ValidateRooms(List<RoomModel> rooms, string theme, ValidationResult result)
        {
            if (_slm == null) return 100;

            int totalScore = 0;
            int validatedCount = 0;

            foreach (var room in rooms.Take(3)) // Validate first 3 rooms to save time
            {
                try
                {
                    var prompt = BuildRoomValidationPrompt(room.Title, room.BaseDescription, theme);
                    var response = _slm.GenerateRoomDescription(prompt, 1); // Use seed=1 for deterministic validation

                    var score = ParseValidationScore(response);
                    totalScore += score;
                    validatedCount++;

                    if (score < 60)
                    {
                        result.Warnings.Add($"Room '{room.Title}' has low quality score: {score}/100");
                        _logger?.LogWarning("?? Room '{RoomTitle}' quality issue: {Score}/100", room.Title, score);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "?? Failed to validate room '{RoomTitle}'", room.Title);
                    result.Warnings.Add($"Could not validate room '{room.Title}': {ex.Message}");
                }
            }

            return validatedCount > 0 ? totalScore / validatedCount : 100;
        }

        private int ValidateNpcs(List<NpcModel> npcs, string theme, ValidationResult result)
        {
            if (_slm == null) return 100;

            int totalScore = 0;
            int validatedCount = 0;

            foreach (var npc in npcs.Take(3)) // Validate first 3 NPCs
            {
                try
                {
                    var prompt = BuildNpcValidationPrompt(npc.Name, npc.Description, theme);
                    var response = _slm.GenerateNpcBio(prompt, 1);

                    var score = ParseValidationScore(response);
                    totalScore += score;
                    validatedCount++;

                    if (score < 60)
                    {
                        result.Warnings.Add($"NPC '{npc.Name}' has low quality score: {score}/100");
                        _logger?.LogWarning("?? NPC '{NpcName}' quality issue: {Score}/100", npc.Name, score);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "?? Failed to validate NPC '{NpcName}'", npc.Name);
                    result.Warnings.Add($"Could not validate NPC '{npc.Name}': {ex.Message}");
                }
            }

            return validatedCount > 0 ? totalScore / validatedCount : 100;
        }

        private int ValidateFactions(List<FactionModel> factions, string theme, ValidationResult result)
        {
            if (_slm == null) return 100;

            int totalScore = 0;
            int validatedCount = 0;

            foreach (var faction in factions)
            {
                try
                {
                    var prompt = BuildFactionValidationPrompt(faction.Name, faction.Description, theme);
                    var response = _slm.GenerateFactionFlavor(prompt, 1);

                    var score = ParseValidationScore(response);
                    totalScore += score;
                    validatedCount++;

                    if (score < 60)
                    {
                        result.Warnings.Add($"Faction '{faction.Name}' has low quality score: {score}/100");
                        _logger?.LogWarning("?? Faction '{FactionName}' quality issue: {Score}/100", faction.Name, score);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "?? Failed to validate faction '{FactionName}'", faction.Name);
                    result.Warnings.Add($"Could not validate faction '{faction.Name}': {ex.Message}");
                }
            }

            return validatedCount > 0 ? totalScore / validatedCount : 100;
        }

        private int ValidateConsistency(WorldGenerationResult result, string theme, ValidationResult result2)
        {
            // Check for basic consistency issues without LLM
            int score = 100;

            // Check room connectivity
            var disconnectedRooms = result.Rooms.Where(r => r.Exits == null || !r.Exits.Any()).ToList();
            if (disconnectedRooms.Any())
            {
                score -= 20;
                result2.Warnings.Add($"Found {disconnectedRooms.Count} disconnected rooms");
                _logger?.LogWarning("?? {Count} disconnected rooms found", disconnectedRooms.Count);
            }

            // Check NPC-faction references
            var invalidNpcs = result.Npcs.Where(n => 
                !result.Factions.Any(f => f.Id == n.FactionId)).ToList();
            if (invalidNpcs.Any())
            {
                score -= 15;
                result2.Warnings.Add($"Found {invalidNpcs.Count} NPCs with invalid faction references");
                _logger?.LogWarning("?? {Count} NPCs with invalid faction references", invalidNpcs.Count);
            }

            // Check room-NPC references
            foreach (var room in result.Rooms)
            {
                var missingNpcs = room.Npcs.Where(npcId => 
                    !result.Npcs.Any(n => n.Id == npcId)).ToList();
                if (missingNpcs.Any())
                {
                    score -= 10;
                    result2.Warnings.Add($"Room '{room.Title}' references {missingNpcs.Count} missing NPCs");
                    _logger?.LogWarning("?? Room '{RoomTitle}' references missing NPCs", room.Title);
                }
            }

            return Math.Max(0, score);
        }

        // Validation prompt templates
        private string BuildRoomValidationPrompt(string roomName, string description, string theme)
        {
            return $@"Rate this room description on a scale of 0-100 based on quality criteria:
- Is it vivid and immersive? (30 points)
- Does it match the {theme} theme? (25 points)
- Does it include specific sensory details? (25 points)
- Is it well-written and engaging? (20 points)

Room Name: {roomName}
Description: {description}

Respond with ONLY a number 0-100, nothing else:";
        }

        private string BuildNpcValidationPrompt(string npcName, string bio, string theme)
        {
            return $@"Rate this NPC biography on a scale of 0-100 based on quality criteria:
- Does it establish a clear personality? (30 points)
- Does it match the {theme} theme? (25 points)
- Does it provide interesting backstory? (25 points)
- Is it memorable and well-written? (20 points)

NPC Name: {npcName}
Biography: {bio}

Respond with ONLY a number 0-100, nothing else:";
        }

        private string BuildFactionValidationPrompt(string factionName, string description, string theme)
        {
            return $@"Rate this faction lore on a scale of 0-100 based on quality criteria:
- Does it establish clear goals/ideology? (30 points)
- Does it match the {theme} theme? (25 points)
- Does it create interesting conflicts? (25 points)
- Is it coherent and well-written? (20 points)

Faction Name: {factionName}
Lore: {description}

Respond with ONLY a number 0-100, nothing else:";
        }

        private int ParseValidationScore(string response)
        {
            // Extract number from response
            var cleaned = new string(response.Where(char.IsDigit).ToArray());
            if (int.TryParse(cleaned, out var score))
            {
                return Math.Clamp(score, 0, 100);
            }

            _logger?.LogWarning("?? Failed to parse validation score from: {Response}", response);
            return 70; // Default to "acceptable" if parsing fails
        }
    }
}

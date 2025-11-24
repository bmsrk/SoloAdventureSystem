using System.Collections.Generic;
using SoloAdventureSystem.Engine.Rules;

namespace SoloAdventureSystem.Engine.Models;

/// <summary>
/// Represents the player character with attributes, skills, and equipment
/// </summary>
public class PlayerCharacter
{
    public string Name { get; set; } = "Adventurer";
    public Dictionary<GameAttribute, int> Attributes { get; set; } = new();
    public Dictionary<Skill, int> Skills { get; set; } = new();
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public List<string> Equipment { get; set; } = new();
    public List<string> ActiveQuests { get; set; } = new();
    public Dictionary<string, object> StatusEffects { get; set; } = new();

    public PlayerCharacter()
    {
        // Initialize default attributes (roll 3d6 for each)
        Attributes = new Dictionary<GameAttribute, int>
        {
            [GameAttribute.Body] = Roll3d6(),
            [GameAttribute.Mind] = Roll3d6(),
            [GameAttribute.Soul] = Roll3d6(),
            [GameAttribute.Presence] = Roll3d6()
        };

        // Initialize skills (starting at 0, can be trained)
        Skills = new Dictionary<Skill, int>
        {
            [Skill.Combat] = 0,
            [Skill.Stealth] = 0,
            [Skill.Knowledge] = 0,
            [Skill.Awareness] = 0,
            [Skill.Social] = 0,
            [Skill.Will] = 0,
            [Skill.Occult] = 0
        };

        MaxHP = CharacterStats.CalculateHP(Attributes);
        HP = MaxHP;
    }

    private static int Roll3d6()
    {
        return Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7);
    }

    public CharacterStats ToCharacterStats()
    {
        return new CharacterStats(Attributes, Skills, HP, CharacterStats.CalculateDefense(Attributes, Skills));
    }
}
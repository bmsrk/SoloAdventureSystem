namespace SoloAdventureSystem.Engine.Rules;

public enum Attribute
{
    Soul,
    Body,
    Mind,
    Presence
}

public enum Skill
{
    Combat,
    Stealth,
    Knowledge,
    Awareness,
    Social,
    Will,
    Occult
}

public enum DamageType
{
    Light,
    Medium,
    Heavy,
    Unarmed
}

public enum AdvantageType
{
    None,
    Advantage,
    Disadvantage
}

public record class RollResult(
    int Total,
    List<int> Dice,
    Attribute Attribute,
    Skill Skill,
    int TargetNumber,
    bool Success,
    bool CriticalSuccess,
    bool CriticalFailure
);

public record class AttackResult(
    RollResult AttackRoll,
    int Defense,
    bool Hit
);

public record class DamageResult(
    DamageType DamageType,
    int Damage,
    List<int> Dice
);

public record class OpposedResult(
    RollResult Attacker,
    RollResult Defender,
    bool AttackerWins
);

public record class CharacterStats(
    Dictionary<Attribute, int> Attributes,
    Dictionary<Skill, int> Skills,
    int HP,
    int Defense
)
{
    public static int CalculateHP(Dictionary<Attribute, int> attributes)
        => 10 + attributes[Attribute.Body] * 2;

    public static int CalculateDefense(Dictionary<Attribute, int> attributes, Dictionary<Skill, int> skills)
        => skills[Skill.Combat] > 0
            ? 6 + attributes[Attribute.Body] + skills[Skill.Combat]
            : 6 + attributes[Attribute.Body] + skills[Skill.Awareness];
}

public static class DiceRoller
{
    public static List<int> Roll2D6()
    {
        var dice = new List<int> { Random.Shared.Next(1, 7), Random.Shared.Next(1, 7) };
        return dice;
    }

    public static List<int> Roll3D6()
    {
        var dice = new List<int> { Random.Shared.Next(1, 7), Random.Shared.Next(1, 7), Random.Shared.Next(1, 7) };
        return dice;
    }

    public static List<int> RollWithAdvantage()
    {
        var dice = Roll3D6();
        dice.Sort();
        return dice.Skip(1).ToList(); // highest 2
    }

    public static List<int> RollWithDisadvantage()
    {
        var dice = Roll3D6();
        dice.Sort();
        return dice.Take(2).ToList(); // lowest 2
    }
}

public class RuleEngine
{
    public static RollResult RollAction(Attribute attribute, Skill skill, int targetNumber, Dictionary<Attribute, int> attributes, Dictionary<Skill, int> skills, AdvantageType advantage = AdvantageType.None)
    {
        List<int> dice = advantage switch
        {
            AdvantageType.Advantage => DiceRoller.RollWithAdvantage(),
            AdvantageType.Disadvantage => DiceRoller.RollWithDisadvantage(),
            _ => DiceRoller.Roll2D6()
        };
        int diceSum = dice.Sum();
        int attrValue = attributes[attribute];
        int skillValue = skills[skill];
        int total = diceSum + attrValue + skillValue;
        bool criticalSuccess = diceSum == 12;
        bool criticalFailure = diceSum == 2;
        bool success = total >= targetNumber;
        return new RollResult(total, dice, attribute, skill, targetNumber, success, criticalSuccess, criticalFailure);
    }

    public static OpposedResult RollOpposed(CharacterStats attacker, CharacterStats defender, (Attribute attr, Skill skill) skillAttrPair)
    {
        var atkRoll = RollAction(skillAttrPair.attr, skillAttrPair.skill, 0, attacker.Attributes, attacker.Skills);
        var defRoll = RollAction(skillAttrPair.attr, skillAttrPair.skill, 0, defender.Attributes, defender.Skills);
        bool attackerWins = atkRoll.Total > defRoll.Total;
        return new OpposedResult(atkRoll, defRoll, attackerWins);
    }

    public static AttackResult RollCombatAttack(CharacterStats attacker, CharacterStats defender)
    {
        var attackRoll = RollAction(Attribute.Body, Skill.Combat, 0, attacker.Attributes, attacker.Skills);
        int defense = CharacterStats.CalculateDefense(defender.Attributes, defender.Skills);
        bool hit = attackRoll.Total > defense;
        return new AttackResult(attackRoll, defense, hit);
    }

    public static DamageResult RollDamage(DamageType damageType)
    {
        List<int> dice;
        int damage;
        switch (damageType)
        {
            case DamageType.Light:
                dice = DiceRoller.Roll2D6().Take(1).ToList();
                damage = dice.Sum();
                break;
            case DamageType.Medium:
                dice = DiceRoller.Roll2D6().Take(1).ToList();
                damage = dice.Sum() + 2;
                break;
            case DamageType.Heavy:
                dice = DiceRoller.Roll2D6();
                damage = dice.Sum();
                break;
            case DamageType.Unarmed:
                dice = DiceRoller.Roll2D6().Take(1).ToList();
                damage = Math.Max(1, dice.Sum() - 2);
                break;
            default:
                dice = new List<int> { 0 };
                damage = 0;
                break;
        }
        return new DamageResult(damageType, damage, dice);
    }

    public static RollResult RollFearTest(CharacterStats stats, int targetNumber)
    {
        return RollAction(Attribute.Soul, Skill.Will, targetNumber, stats.Attributes, stats.Skills);
    }

    public static AttackResult RollSocialInteraction(CharacterStats attacker, CharacterStats defender)
    {
        var attackRoll = RollAction(Attribute.Presence, Skill.Social, 0, attacker.Attributes, attacker.Skills);
        int defense = 6 + defender.Attributes[Attribute.Presence] + defender.Skills[Skill.Social];
        bool hit = attackRoll.Total > defense;
        return new AttackResult(attackRoll, defense, hit);
    }

    public static RollResult RollSupernatural(CharacterStats stats, int targetNumber)
    {
        return RollAction(Attribute.Soul, Skill.Occult, targetNumber, stats.Attributes, stats.Skills);
    }
}

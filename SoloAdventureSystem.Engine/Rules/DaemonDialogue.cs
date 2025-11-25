using System;
using System.Collections.Generic;
using SoloAdventureSystem.Engine.Models;
using SoloAdventureSystem.Engine.Game;

namespace SoloAdventureSystem.Engine.Rules
{
    public record class DaemonState(Dictionary<string, int> Drives)
    {
        public int GetDrive(string name) => Drives.TryGetValue(name, out var v) ? v : 0;

        public int GetSkillModifier(Skill skill)
        {
            return skill switch
            {
                Skill.Social => GetDrive("Ambition") / 2 + GetDrive("Presence") / 3,
                Skill.Combat => GetDrive("Rage") / 2,
                Skill.Knowledge => GetDrive("Curiosity") / 2,
                _ => 0
            };
        }

        public void AdjustDrive(string name, int delta)
        {
            Drives[name] = GetDrive(name) + delta;
        }
    }

    public record class CheckOutcome(
        RollResult Roll,
        int ModifiedTotal,
        bool Success,
        bool CriticalSuccess,
        bool CriticalFailure
    );

    public static class DaemonDialogue
    {
        public static CheckOutcome ResolveSkillCheck(
            Dictionary<GameAttribute, int> attributes,
            Dictionary<Skill, int> skills,
            DaemonState? daemon,
            GameAttribute attribute,
            Skill skill,
            int targetNumber,
            AdvantageType advantage = AdvantageType.None)
        {
            var roll = RuleEngine.RollAction(attribute, skill, targetNumber, attributes, skills, advantage);
            var daemonMod = daemon?.GetSkillModifier(skill) ?? 0;
            var modified = roll.Total + daemonMod;
            var success = modified >= targetNumber;
            return new CheckOutcome(roll, modified, success, roll.CriticalSuccess, roll.CriticalFailure);
        }

        public static OpposedResult ResolveOpposedSocial(
            CharacterStats attackerStats,
            CharacterStats defenderStats,
            (GameAttribute attr, Skill skill) skillAttrPair,
            DaemonState? attackerDaemon = null,
            DaemonState? defenderDaemon = null)
        {
            var atkRoll = RuleEngine.RollAction(skillAttrPair.attr, skillAttrPair.skill, 0, attackerStats.Attributes, attackerStats.Skills);
            var defRoll = RuleEngine.RollAction(skillAttrPair.attr, skillAttrPair.skill, 0, defenderStats.Attributes, defenderStats.Skills);

            var atkMod = attackerDaemon?.GetSkillModifier(skillAttrPair.skill) ?? 0;
            var defMod = defenderDaemon?.GetSkillModifier(skillAttrPair.skill) ?? 0;

            var atkTotal = atkRoll.Total + atkMod;
            var defTotal = defRoll.Total + defMod;

            var attackerWins = atkTotal > defTotal;
            return new OpposedResult(atkRoll, defRoll, attackerWins);
        }

        public static void ApplyEffect(GameState state, string effect)
        {
            if (state == null || string.IsNullOrEmpty(effect)) return;

            // Effects are simple strings in format Type:key:value
            var parts = effect.Split(':', 3);
            if (parts.Length < 2) return;

            var type = parts[0];
            var key = parts.Length > 1 ? parts[1] : string.Empty;
            var valStr = parts.Length > 2 ? parts[2] : "1";
            int.TryParse(valStr, out var val);

            switch (type)
            {
                case "Flag":
                    state.Flags[key] = val == 1;
                    break;
                case "Relation":
                    // Key is factionId:delta or factionId:targetId
                    // Not implemented here - integration point
                    break;
                case "Daemon":
                    // Key format: npcId|driveName
                    var kv = key.Split('|', 2);
                    if (kv.Length == 2)
                    {
                        var actorId = kv[0];
                        var drive = kv[1];
                        var ds = state.GetDaemonFor(actorId);
                        if (ds != null) ds.AdjustDrive(drive, val);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

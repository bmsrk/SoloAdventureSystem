using System.Collections.Generic;
using YamlDotNet.Serialization;
using SoloAdventureSystem.Engine.Rules;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class StoryNodeModel
    {
        [YamlMember(Alias = "id")]
        public string Id { get; set; } = "";
        [YamlMember(Alias = "title")]
        public string Title { get; set; } = "";
        [YamlMember(Alias = "text", ScalarStyle = YamlDotNet.Core.ScalarStyle.Literal)]
        public string Text { get; set; } = "";
        [YamlMember(Alias = "owner_npc_id")]
        public string OwnerNpcId { get; set; } = "";
        [YamlMember(Alias = "raw_json")]
        public string RawGeneratorJson { get; set; } = "";
        [YamlMember(Alias = "choices")]
        public List<StoryChoice> Choices { get; set; } = new();
    }
    public class StoryChoice
    {
        [YamlMember(Alias = "label")]
        public string Label { get; set; } = "";
        [YamlMember(Alias = "next")]
        public string Next { get; set; } = "";
        [YamlMember(Alias = "effects")]
        public List<string> Effects { get; set; } = new();
        [YamlMember(Alias = "skill_check")]
        public SkillCheckModel? SkillCheck { get; set; }
    }

    public class SkillCheckModel
    {
        [YamlMember(Alias = "attribute")]
        public GameAttribute Attribute { get; set; } = GameAttribute.Soul;
        [YamlMember(Alias = "skill")]
        public Skill Skill { get; set; } = Skill.Social;
        [YamlMember(Alias = "target_number")]
        public int TargetNumber { get; set; } = 10;
        [YamlMember(Alias = "opponent_npc_id")]
        public string OpponentNpcId { get; set; } = ""; // optional
    }
}

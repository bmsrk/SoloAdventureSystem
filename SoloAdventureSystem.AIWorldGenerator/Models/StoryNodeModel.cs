using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SoloAdventureSystem.ContentGenerator
{
    public class StoryNodeModel
    {
        [YamlMember(Alias = "id")]
        public string Id { get; set; } = "";
        [YamlMember(Alias = "title")]
        public string Title { get; set; } = "";
        [YamlMember(Alias = "text", ScalarStyle = YamlDotNet.Core.ScalarStyle.Literal)]
        public string Text { get; set; } = "";
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
    }
}

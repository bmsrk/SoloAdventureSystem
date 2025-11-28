using System;
using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    // Top-level V2 world model intended for deterministic export to JSON
    public class WorldV2
    {
        public Meta Meta { get; set; } = new Meta();
        public List<string> Skills { get; set; } = new List<string>();
        public WorldInfo World { get; set; } = new WorldInfo();
        public List<LocationV2> Locations { get; set; } = new List<LocationV2>();
        public List<FactionV2> Factions { get; set; } = new List<FactionV2>();
        public List<NpcV2> Npcs { get; set; } = new List<NpcV2>();
        public List<SceneV2> Scenes { get; set; } = new List<SceneV2>();
        public List<SecretV2> Secrets { get; set; } = new List<SecretV2>();
        public List<EndingV2> Endings { get; set; } = new List<EndingV2>();
    }

    public class Meta
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "v2";
        // When true, output is reproducible from inputs; V2 can operate non-deterministically when using LLM randomness
        public bool Deterministic { get; set; } = false;
    }

    public class WorldInfo
    {
        public string Name { get; set; } = "";
        public string Theme { get; set; } = "";
        public string Tone { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class LocationV2
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class FactionV2
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class NpcV2
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> PersonalityTraits { get; set; } = new List<string>();
        public string RelationToPlayer { get; set; } = "neutral"; // ally, neutral, rival
        public int Trust { get; set; } = 5; // 0-10
        public List<string> Secrets { get; set; } = new List<string>();
        public List<string> KnowledgeAreas { get; set; } = new List<string>();
    }

    public class SceneV2
    {
        public string Id { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ActionV2> Actions { get; set; } = new List<ActionV2>();
        public List<string> SecretsAndTwists { get; set; } = new List<string>();
    }

    public class ActionV2
    {
        // type: social_check, educational_check, interaction
        public string Type { get; set; } = "interaction";
        // skill must be one of the top-level Skills list
        public string Skill { get; set; } = "";
        public OutcomeV2 SuccessOutcome { get; set; } = new OutcomeV2();
        public OutcomeV2 FailureOutcome { get; set; } = new OutcomeV2();
        public int Difficulty { get; set; } = 5;
    }

    public class OutcomeV2
    {
        public string Text { get; set; } = "";
        public int ReputationChange { get; set; } = 0;
        public int TrustChange { get; set; } = 0;
        public List<string> UnlockSecrets { get; set; } = new List<string>();
        public string NextSceneId { get; set; } = "";
    }

    public class SecretV2
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class EndingV2
    {
        public string Name { get; set; } = "";
        public EndingConditions Conditions { get; set; } = new EndingConditions();
        public string Text { get; set; } = "";
    }

    public class EndingConditions
    {
        public int ReputationMin { get; set; } = Int32.MinValue;
        public List<string> RequiredAllies { get; set; } = new List<string>();
        public List<string> SecretsRequired { get; set; } = new List<string>();
    }
}

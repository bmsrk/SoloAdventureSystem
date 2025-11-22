using System;
using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Provides deterministic procedural name generation for world elements.
/// All generation uses seeds to ensure reproducibility.
/// </summary>
public static class ProceduralNames
{
    // Cyberpunk-themed room names
    private static readonly string[] RoomPrefixes = new[]
    {
        "Data", "Neural", "Cyber", "Quantum", "Bio", "Nano", "Tech", "Corp",
        "Shadow", "Neon", "Chrome", "Grid", "Net", "Sync", "Pulse", "Matrix",
        "Holo", "Synth", "Plasma", "Void", "Nexus", "Rift", "Echo", "Flux",
        "Aether", "Vertex", "Cryo", "Thermo", "Electro", "Hydro", "Pyro", "Laser"
    };

    private static readonly string[] RoomSuffixes = new[]
    {
        "Vault", "Hub", "Chamber", "Node", "Core", "Terminal", "Bay", "Suite",
        "Plaza", "Alley", "Market", "Lounge", "Lab", "Station", "Deck", "Archive",
        "Sector", "Module", "Complex", "Facility", "Center", "District", "Zone", "Bunker",
        "Nexus", "Spire", "Quarters", "Sanctum", "Haven", "Outpost", "Den", "Lair"
    };

    // NPC first names (expanded)
    private static readonly string[] FirstNames = new[]
    {
        "Marcus", "Sarah", "Johnny", "Kenji", "Maria", "Viktor", "Chen", "Isabella",
        "Dmitri", "Yuki", "Hassan", "Zara", "Diego", "Anika", "Raj", "Natasha",
        "Carlos", "Mei", "Ahmed", "Lena", "Takeshi", "Sofia", "Ivan", "Priya",
        "Jin", "Lucia", "Maxim", "Aisha", "Hiroshi", "Elena", "Omar", "Kira",
        "Felix", "Nadia", "Ryo", "Carmen", "Anton", "Fatima", "Hideo", "Vera"
    };

    // NPC last names (expanded)
    private static readonly string[] LastNames = new[]
    {
        "Chen", "Rodriguez", "Yamamoto", "Blake", "Volkov", "Singh", "Santos", "Kim",
        "Petrov", "Hassan", "Martinez", "Tanaka", "Kowalski", "Patel", "Silva", "Wong",
        "Ivanov", "Garcia", "Nakamura", "Müller", "Fernandez", "Suzuki", "Nielsen", "Kumar",
        "Park", "Lopez", "Andersson", "Dubois", "Rossi", "Lee", "Wang", "Brown",
        "Cohen", "O'Brien", "Novak", "Morales", "Sato", "Shah", "Romano", "Aziz"
    };

    // NPC nicknames/callsigns (expanded)
    private static readonly string[] Nicknames = new[]
    {
        "Wire", "Ghost", "Raven", "Spike", "Zero", "Blade", "Cipher", "Echo",
        "Nova", "Wraith", "Volt", "Shade", "Pulse", "Glitch", "Nexus", "Phantom",
        "Catalyst", "Vapor", "Axon", "Vertigo", "Dynamo", "Spectre", "Rift", "Jinx",
        "Strider", "Vex", "Flux", "Ash", "Bolt", "Frost", "Ember", "Storm",
        "Havoc", "Prism", "Surge", "Void", "Zephyr", "Onyx", "Chrome", "Viper"
    };

    // Faction names (expanded)
    private static readonly string[] FactionPrefixes = new[]
    {
        "Neon", "Shadow", "Chrome", "Digital", "Cyber", "Tech", "Neural", "Quantum",
        "Black", "Red", "Azure", "Crimson", "Silver", "Golden", "Obsidian", "Platinum",
        "Void", "Plasma", "Crystal", "Iron", "Steel", "Titanium", "Diamond", "Ruby",
        "Sapphire", "Emerald", "Dark", "Bright", "Deep", "High", "Prime", "Ultra"
    };

    private static readonly string[] FactionSuffixes = new[]
    {
        "Syndicate", "Collective", "Corporation", "Cartel", "Network", "Alliance", "Union",
        "Coalition", "Council", "Consortium", "Initiative", "Foundation", "Order", "Guild",
        "Federation", "Empire", "League", "Pact", "Accord", "Assembly", "Confederacy", "Dominion",
        "Directorate", "Combine", "Brotherhood", "Sisterhood", "Circle", "Clan", "House", "Sect"
    };

    /// <summary>
    /// Generates a procedural room name based on seed.
    /// </summary>
    public static string GenerateRoomName(int seed)
    {
        var rand = new Random(seed);
        var prefix = RoomPrefixes[rand.Next(RoomPrefixes.Length)];
        var suffix = RoomSuffixes[rand.Next(RoomSuffixes.Length)];
        return $"{prefix} {suffix}";
    }

    /// <summary>
    /// Generates a procedural NPC name based on seed.
    /// Format can be: "FirstName LastName" or "FirstName 'Nickname' LastName"
    /// </summary>
    public static string GenerateNpcName(int seed)
    {
        var rand = new Random(seed);
        var firstName = FirstNames[rand.Next(FirstNames.Length)];
        var lastName = LastNames[rand.Next(LastNames.Length)];
        
        // 30% chance of having a nickname
        if (rand.Next(100) < 30)
        {
            var nickname = Nicknames[rand.Next(Nicknames.Length)];
            return $"{firstName} '{nickname}' {lastName}";
        }
        
        return $"{firstName} {lastName}";
    }

    /// <summary>
    /// Generates a procedural faction name based on seed.
    /// </summary>
    public static string GenerateFactionName(int seed)
    {
        var rand = new Random(seed);
        var prefix = FactionPrefixes[rand.Next(FactionPrefixes.Length)];
        var suffix = FactionSuffixes[rand.Next(FactionSuffixes.Length)];
        return $"{prefix} {suffix}";
    }

    /// <summary>
    /// Generates atmospheric lighting description.
    /// </summary>
    public static string GenerateLighting(int seed)
    {
        var lightingOptions = new[]
        {
            "harsh fluorescent lights",
            "dim neon glow",
            "flickering holographic displays",
            "cold LED strips",
            "warm incandescent bulbs",
            "pulsing blue terminal lights",
            "strobing emergency lighting",
            "soft amber lamps",
            "stark white spotlights",
            "multicolored data streams",
            "shadowy corners with occasional glints",
            "bioluminescent panels",
            "crimson emergency beacons",
            "ultraviolet scanning lights",
            "cascading color-shift panels",
            "intermittent arc lights",
            "phosphorescent wall strips",
            "laser grid projections",
            "sodium vapor bulbs",
            "crystalline light diffusers"
        };
        
        var rand = new Random(seed);
        return lightingOptions[rand.Next(lightingOptions.Length)];
    }

    /// <summary>
    /// Generates atmospheric sound description.
    /// </summary>
    public static string GenerateSound(int seed)
    {
        var soundOptions = new[]
        {
            "humming servers",
            "distant traffic",
            "electronic beeping",
            "ventilation systems",
            "muffled conversations",
            "buzzing neon signs",
            "dripping water",
            "keyboard clicking",
            "pneumatic hissing",
            "gentle music",
            "crackling static",
            "mechanical whirring",
            "footsteps echoing",
            "crowd murmurs",
            "power generators thrumming",
            "hydraulic pumps cycling",
            "data transfer chirping",
            "cooling fans spinning",
            "surveillance drones buzzing",
            "automated announcements",
            "electromagnetic interference",
            "rain pattering on metal",
            "security alerts chiming",
            "magnetic locks engaging"
        };
        
        var rand = new Random(seed);
        return soundOptions[rand.Next(soundOptions.Length)];
    }

    /// <summary>
    /// Generates atmospheric smell description.
    /// </summary>
    public static string GenerateSmell(int seed)
    {
        var smellOptions = new[]
        {
            "ozone from electronics",
            "stale air",
            "synthetic flowers",
            "street food",
            "cleaning chemicals",
            "burnt circuitry",
            "coffee brewing",
            "cigarette smoke",
            "recycled air",
            "rain on concrete",
            "chemical tang",
            "industrial lubricant",
            "sterilization agents",
            "fried noodles",
            "machine oil",
            "hot metal",
            "plastic fumes",
            "sanitizer spray",
            "cheap perfume",
            "damp concrete",
            "electrical burning",
            "coolant vapor"
        };
        
        var rand = new Random(seed);
        return smellOptions[rand.Next(smellOptions.Length)];
    }

    /// <summary>
    /// Generates a complete atmospheric description combining multiple senses.
    /// </summary>
    public static string GenerateAtmosphere(int seed)
    {
        var rand = new Random(seed);
        var lighting = GenerateLighting(seed);
        var sound = GenerateSound(seed + 1);
        var smell = GenerateSmell(seed + 2);
        
        // Randomly choose format
        var format = rand.Next(3);
        return format switch
        {
            0 => $"Lit by {lighting}, filled with {sound}, and smelling of {smell}.",
            1 => $"The {sound} mix with the scent of {smell} under {lighting}.",
            2 => $"Bathed in {lighting}, you hear {sound} and smell {smell}.",
            _ => $"Atmosphere: {lighting}, {sound}, {smell}."
        };
    }
}

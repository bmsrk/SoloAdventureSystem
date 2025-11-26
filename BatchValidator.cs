using System;
using System.IO;
using System.Threading.Tasks;
using SoloAdventureSystem.Web.UI.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var validator = new WorldFileValidator();
        var worldsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "source", "repos", "SoloAdventureSystem", "content", "worlds");

        Console.WriteLine("Running batch validation on worlds...");
        var results = await validator.BatchValidateWorldsAsync(worldsDir, new Progress<string>(msg => Console.WriteLine(msg)));

        Console.WriteLine("\nBatch Validation Results:");
        foreach (var result in results)
        {
            Console.WriteLine($"World: {result.WorldName}");
            Console.WriteLine($"  Overall Score: {result.OverallScore}/100");
            Console.WriteLine($"  Room Score: {result.RoomScore}/100");
            Console.WriteLine($"  NPC Score: {result.NpcScore}/100");
            Console.WriteLine($"  Faction Score: {result.FactionScore}/100");
            Console.WriteLine($"  Consistency Score: {result.ConsistencyScore}/100");
            Console.WriteLine($"  Uniqueness Score: {result.UniquenessScore}/100");
            Console.WriteLine($"  Title Presence: {result.TitlePresenceScore}%");
            if (result.Errors.Any()) Console.WriteLine($"  Errors: {string.Join("; ", result.Errors)}");
            if (result.Warnings.Any()) Console.WriteLine($"  Warnings: {string.Join("; ", result.Warnings)}");
            Console.WriteLine();
        }
    }
}
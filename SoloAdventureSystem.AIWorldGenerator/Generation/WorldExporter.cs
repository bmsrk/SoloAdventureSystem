using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using YamlDotNet.Serialization;
using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator
{
    public class WorldExporter
    {
        public void Export(WorldGenerationResult result, WorldGenerationOptions options, string outputDir)
        {
            // Create output folder structure
            Directory.CreateDirectory(outputDir);
            Directory.CreateDirectory(Path.Combine(outputDir, "rooms"));
            Directory.CreateDirectory(Path.Combine(outputDir, "factions"));
            Directory.CreateDirectory(Path.Combine(outputDir, "npcs"));
            Directory.CreateDirectory(Path.Combine(outputDir, "story"));
            Directory.CreateDirectory(Path.Combine(outputDir, "assets/images"));
            Directory.CreateDirectory(Path.Combine(outputDir, "system"));
            Directory.CreateDirectory(Path.Combine(outputDir, "map"));

            // Write world.json
            File.WriteAllText(Path.Combine(outputDir, "world.json"), JsonSerializer.Serialize(result.World));

            // Write rooms
            foreach (var room in result.Rooms)
            {
                File.WriteAllText(Path.Combine(outputDir, "rooms", $"{room.Id}.json"), JsonSerializer.Serialize(room));
            }
            // Write factions
            foreach (var faction in result.Factions)
            {
                File.WriteAllText(Path.Combine(outputDir, "factions", $"{faction.Id}.json"), JsonSerializer.Serialize(faction));
            }
            // Write npcs
            foreach (var npc in result.Npcs)
            {
                File.WriteAllText(Path.Combine(outputDir, "npcs", $"{npc.Id}.json"), JsonSerializer.Serialize(npc));
            }
            // Write story nodes (YAML)
            var serializer = new SerializerBuilder().Build();
            foreach (var node in result.StoryNodes)
            {
                var yaml = serializer.Serialize(node);
                File.WriteAllText(Path.Combine(outputDir, "story", $"{node.Id}.yaml"), yaml);
            }
            // Write system files
            File.WriteAllText(Path.Combine(outputDir, "system", "seed.txt"), options.Seed.ToString());
            File.WriteAllText(Path.Combine(outputDir, "system", "generatorVersion.txt"), "1.0.0");
            // Write map placeholder
            File.WriteAllText(Path.Combine(outputDir, "map", "map.png"), ""); // TODO: Replace with actual image or sample asset
        }

        public void Zip(string sourceDir, string zipPath)
        {
            // Ensure the zip is created in a different location than sourceDir
            // to avoid trying to zip the zip file itself
            var zipDir = Path.GetDirectoryName(zipPath);
            if (zipDir != null && !Directory.Exists(zipDir))
            {
                Directory.CreateDirectory(zipDir);
            }

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            // Create temporary zip in a safe location first
            var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            try
            {
                ZipFile.CreateFromDirectory(sourceDir, tempZipPath);
                
                // Move to final destination
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                File.Move(tempZipPath, zipPath);
            }
            catch
            {
                // Clean up temp file if something goes wrong
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
                throw;
            }
        }
    }
}

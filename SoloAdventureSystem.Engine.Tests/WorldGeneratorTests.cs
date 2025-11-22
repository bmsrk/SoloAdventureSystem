using Xunit;
using SoloAdventureSystem.ContentGenerator;
using System.IO;

namespace SoloAdventureSystem.Engine.Tests
{
    public class WorldGeneratorTests
    {
        [Fact]
        public void SeededWorldGenerator_GeneratesValidWorld()
        {
            var options = new WorldGenerationOptions
            {
                Name = "TestWorld",
                Seed = 12345,
                Theme = "TestTheme",
                Regions = 5,
                NpcDensity = "medium",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            var result = generator.Generate(options);

            Assert.NotNull(result.World);
            Assert.NotNull(result.Rooms);
            Assert.True(result.Rooms.Count >= 3);
            Assert.NotNull(result.Factions);
            Assert.True(result.Factions.Count >= 1);
            Assert.NotNull(result.StoryNodes);
            Assert.True(result.StoryNodes.Count >= 1);
            Assert.Equal(options.Name, result.World.Name);
            Assert.Equal(options.Seed, options.Seed);
        }

        [Fact]
        public void WorldExporter_CreatesValidWorldZip()
        {
            var options = new WorldGenerationOptions
            {
                Name = "TestZip",
                Seed = 54321,
                Theme = "ZipTheme",
                Regions = 4,
                NpcDensity = "low",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            var result = generator.Generate(options);
            var tempDir = Path.Combine(Path.GetTempPath(), $"SoloAdventureWorld_{options.Name}_{options.Seed}");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);
            var exporter = new WorldExporter();
            exporter.Export(result, options, tempDir);
            var zipPath = Path.Combine(tempDir, $"SoloAdventureWorld_{options.Name}_{options.Seed}.zip");
            exporter.Zip(tempDir, zipPath);
            Assert.True(File.Exists(zipPath));
            var fileInfo = new FileInfo(zipPath);
            Assert.True(fileInfo.Length > 0);
            // Optionally: check for required files inside the zip
            using (var archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
            {
                Assert.Contains(archive.Entries, e => e.FullName == "world.json");
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("rooms/"));
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("factions/"));
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("npcs/"));
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("story/"));
            }
        }
    }
}

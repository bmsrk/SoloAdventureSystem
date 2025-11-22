using Xunit;
using SoloAdventureSystem.ContentGenerator;
using SoloAdventureSystem.ContentGenerator.Generation;
using System.IO;
using System.Linq;

namespace SoloAdventureSystem.Engine.Tests
{
    public class WorldGeneratorTests
    {
        [Fact]
        public void SeededWorldGenerator_GeneratesValidWorld()
        {
            // Arrange
            var options = new WorldGenerationOptions
            {
                Name = "TestWorld",
                Seed = 12345,
                Theme = "Cyberpunk Dystopia",
                Regions = 5,
                NpcDensity = "medium",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            
            // Act
            var result = generator.Generate(options);

            // Assert
            Assert.NotNull(result.World);
            Assert.NotNull(result.Rooms);
            Assert.True(result.Rooms.Count >= 3);
            Assert.NotNull(result.Factions);
            Assert.True(result.Factions.Count >= 1);
            Assert.NotNull(result.StoryNodes);
            Assert.True(result.StoryNodes.Count >= 1);
            Assert.Equal(options.Name, result.World.Name);
        }
        
        [Fact]
        public void SeededWorldGenerator_WithEnhancedNames_GeneratesProceduralNames()
        {
            // Arrange
            var options = new WorldGenerationOptions
            {
                Name = "EnhancedTest",
                Seed = 99999,
                Theme = "Cyberpunk",
                Regions = 3,
                NpcDensity = "low",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            
            // Act
            var result = generator.Generate(options);

            // Assert - Rooms should have procedural names
            Assert.All(result.Rooms, room =>
            {
                Assert.NotNull(room.Title);
                Assert.NotEmpty(room.Title);
                // Should NOT be "Room 1", "Room 2" anymore
                Assert.DoesNotContain("Room ", room.Title);
                // Should have space (Prefix + Suffix format)
                Assert.Contains(" ", room.Title);
            });
            
            // Assert - NPCs should have procedural names
            Assert.All(result.Npcs, npc =>
            {
                Assert.NotNull(npc.Name);
                Assert.NotEmpty(npc.Name);
                // Should NOT be "NPC 1", "NPC 2" anymore
                Assert.DoesNotContain("NPC ", npc.Name);
                // Should have space (FirstName + LastName format)
                Assert.Contains(" ", npc.Name);
            });
            
            // Assert - Faction should have procedural name
            Assert.All(result.Factions, faction =>
            {
                Assert.NotNull(faction.Name);
                Assert.NotEmpty(faction.Name);
                // Should NOT be "Faction One" anymore
                Assert.DoesNotContain("Faction One", faction.Name);
                // Should have space (Prefix + Suffix format)
                Assert.Contains(" ", faction.Name);
            });
        }
        
        [Fact]
        public void SeededWorldGenerator_SameSeed_GeneratesSameWorld()
        {
            // Arrange
            var options1 = new WorldGenerationOptions
            {
                Name = "Deterministic1",
                Seed = 42,
                Theme = "Cyberpunk",
                Regions = 4,
                NpcDensity = "medium",
                RenderImages = false
            };
            var options2 = new WorldGenerationOptions
            {
                Name = "Deterministic2", // Different name
                Seed = 42,              // Same seed
                Theme = "Cyberpunk",
                Regions = 4,
                NpcDensity = "medium",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            
            // Act
            var result1 = generator.Generate(options1);
            var result2 = generator.Generate(options2);

            // Assert - Structure should be identical
            Assert.Equal(result1.Rooms.Count, result2.Rooms.Count);
            Assert.Equal(result1.Npcs.Count, result2.Npcs.Count);
            Assert.Equal(result1.Factions.Count, result2.Factions.Count);
            
            // Assert - Names should be identical (procedurally generated from seed)
            for (int i = 0; i < result1.Rooms.Count; i++)
            {
                Assert.Equal(result1.Rooms[i].Title, result2.Rooms[i].Title);
            }
            
            for (int i = 0; i < result1.Npcs.Count; i++)
            {
                Assert.Equal(result1.Npcs[i].Name, result2.Npcs[i].Name);
            }
            
            Assert.Equal(result1.Factions[0].Name, result2.Factions[0].Name);
        }
        
        [Fact]
        public void SeededWorldGenerator_DifferentSeeds_GeneratesDifferentWorlds()
        {
            // Arrange
            var options1 = new WorldGenerationOptions
            {
                Name = "World1",
                Seed = 111,
                Theme = "Cyberpunk",
                Regions = 3,
                NpcDensity = "medium",
                RenderImages = false
            };
            var options2 = new WorldGenerationOptions
            {
                Name = "World2",
                Seed = 222, // Different seed
                Theme = "Cyberpunk",
                Regions = 3,
                NpcDensity = "medium",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            
            // Act
            var result1 = generator.Generate(options1);
            var result2 = generator.Generate(options2);

            // Assert - Names should be different
            var room1Names = result1.Rooms.Select(r => r.Title).ToList();
            var room2Names = result2.Rooms.Select(r => r.Title).ToList();
            
            // At least some room names should be different
            var differentRooms = room1Names.Zip(room2Names, (a, b) => a != b).Count(diff => diff);
            Assert.True(differentRooms > 0, "Different seeds should produce different room names");
            
            var npc1Names = result1.Npcs.Select(n => n.Name).ToList();
            var npc2Names = result2.Npcs.Select(n => n.Name).ToList();
            
            // At least some NPC names should be different
            var differentNpcs = npc1Names.Zip(npc2Names, (a, b) => a != b).Count(diff => diff);
            Assert.True(differentNpcs > 0, "Different seeds should produce different NPC names");
        }
        
        [Fact]
        public void SeededWorldGenerator_CreatesInterconnectedRooms()
        {
            // Arrange
            var options = new WorldGenerationOptions
            {
                Name = "Connected",
                Seed = 777,
                Theme = "Cyberpunk",
                Regions = 5,
                NpcDensity = "medium",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            
            // Act
            var result = generator.Generate(options);

            // Assert - All rooms should be connected
            Assert.All(result.Rooms, room =>
            {
                Assert.NotNull(room.Exits);
                // At least first and last rooms might have 1 exit, but middle rooms should have more
            });
            
            // Verify main path exists (first room can reach last room)
            var visited = new HashSet<string>();
            var toVisit = new Queue<string>();
            toVisit.Enqueue(result.Rooms[0].Id);
            
            while (toVisit.Count > 0)
            {
                var current = toVisit.Dequeue();
                if (visited.Contains(current)) continue;
                visited.Add(current);
                
                var room = result.Rooms.FirstOrDefault(r => r.Id == current);
                if (room?.Exits != null)
                {
                    foreach (var exit in room.Exits.Values)
                    {
                        if (!visited.Contains(exit))
                        {
                            toVisit.Enqueue(exit);
                        }
                    }
                }
            }
            
            // Should be able to reach all rooms from start
            Assert.Equal(result.Rooms.Count, visited.Count);
        }

        [Fact]
        public void WorldExporter_CreatesValidWorldZip()
        {
            // Arrange
            var options = new WorldGenerationOptions
            {
                Name = "TestZip",
                Seed = 54321,
                Theme = "Cyberpunk",
                Regions = 4,
                NpcDensity = "low",
                RenderImages = false
            };
            var generator = new SeededWorldGenerator(new StubSLMAdapter(), new StubImageAdapter());
            var result = generator.Generate(options);
            var tempDir = Path.Combine(Path.GetTempPath(), $"SoloAdventureWorld_{options.Name}_{options.Seed}");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);
            
            // Act
            var exporter = new WorldExporter();
            exporter.Export(result, options, tempDir);
            var zipPath = Path.Combine(tempDir, $"SoloAdventureWorld_{options.Name}_{options.Seed}.zip");
            exporter.Zip(tempDir, zipPath);
            
            // Assert
            Assert.True(File.Exists(zipPath));
            var fileInfo = new FileInfo(zipPath);
            Assert.True(fileInfo.Length > 0);
            
            // Check for required files inside the zip
            using (var archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
            {
                Assert.Contains(archive.Entries, e => e.FullName == "world.json");
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("rooms/"));
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("factions/"));
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("npcs/"));
                Assert.Contains(archive.Entries, e => e.FullName.StartsWith("story/"));
            }
            
            // Cleanup
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}

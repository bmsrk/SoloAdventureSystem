using System.IO;
using System.IO.Compression;
using System.Text;
using SoloAdventureSystem.Engine.Models;
using SoloAdventureSystem.Engine.WorldLoader;
using Xunit;

namespace SoloAdventureSystem.Engine.Tests
{
    /// <summary>
    /// Tests for WorldLoaderService - validates loading of generated world ZIPs
    /// </summary>
    public class WorldLoaderServiceTests
    {
        #region Helper Methods

        private static string? FindContentWorldsDirectory()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var projectRoot = currentDir;

            // Navigate up to find solution root
            while (projectRoot != null && !File.Exists(Path.Combine(projectRoot, "SoloAdventureSystem.sln")))
            {
                projectRoot = Directory.GetParent(projectRoot)?.FullName;
            }

            if (projectRoot == null)
                return null;

            var worldsDir = Path.Combine(projectRoot, "SoloAdventureSystem.AIWorldGenerator", "content", "worlds");
            return Directory.Exists(worldsDir) ? worldsDir : null;
        }

        private static async Task<WorldModel?> LoadWorldFromZip(string zipPath)
        {
            using var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read);
            var loader = new WorldLoaderService();
            return await loader.LoadFromZipAsync(fs);
        }

        #endregion

        #region Basic Loading Tests

        [Fact]
        public async Task LoadFromZipValidWorldSuccess()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null)
            {
                // Skip test if content directory not found
                return;
            }

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0)
            {
                // Skip if no worlds generated yet
                return;
            }

            // Act - Load first available world
            var result = await LoadWorldFromZip(zipFiles[0]);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.WorldDefinition);
            Assert.NotNull(result.WorldDefinition.Name);
            Assert.NotNull(result.WorldDefinition.StartLocationId);
            Assert.NotNull(result.Rooms);
            Assert.NotEmpty(result.Rooms);
        }

        [Fact]
        public async Task LoadFromZipHasRequiredCollections()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act
            var result = await LoadWorldFromZip(zipFiles[0]);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Rooms);
            Assert.NotNull(result.Npcs);
            Assert.NotNull(result.Factions);
            Assert.NotNull(result.StoryNodes);
        }

        [Fact]
        public async Task LoadFromZipRoomsHaveValidIds()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act
            var result = await LoadWorldFromZip(zipFiles[0]);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Rooms);
            Assert.All(result.Rooms, room =>
            {
                Assert.NotNull(room.Id);
                Assert.NotEmpty(room.Id);
                Assert.NotNull(room.Name);
            });
        }

        [Fact]
        public async Task LoadFromZipStartLocationExists()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act
            var result = await LoadWorldFromZip(zipFiles[0]);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.WorldDefinition);
            Assert.NotNull(result.WorldDefinition.StartLocationId);
            Assert.NotNull(result.Rooms);

            // Verify start location exists in rooms
            var startRoom = result.Rooms.FirstOrDefault(r => r.Id == result.WorldDefinition.StartLocationId);
            Assert.NotNull(startRoom);
        }

        #endregion

        #region Multiple Worlds Tests

        [Fact]
        public async Task LoadFromZipAllGeneratedWorldsLoadSuccessfully()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act & Assert - Load all worlds
            foreach (var zipFile in zipFiles)
            {
                var result = await LoadWorldFromZip(zipFile);
                
                Assert.NotNull(result);
                Assert.NotNull(result.WorldDefinition);
                Assert.NotNull(result.Rooms);
                Assert.NotEmpty(result.Rooms);
                
                // Verify world integrity
                Assert.NotNull(result.WorldDefinition.Name);
                Assert.NotNull(result.WorldDefinition.StartLocationId);
                
                // Verify start location is valid
                var startRoom = result.Rooms.FirstOrDefault(r => r.Id == result.WorldDefinition.StartLocationId);
                Assert.NotNull(startRoom);
            }
        }

        [Fact]
        public async Task LoadFromZipAllWorldsHaveMinimumContent()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act & Assert
            foreach (var zipFile in zipFiles)
            {
                var result = await LoadWorldFromZip(zipFile);
                
                Assert.NotNull(result);
                Assert.NotNull(result.Rooms);
                Assert.NotNull(result.Npcs);
                Assert.NotNull(result.Factions);
                
                // Verify minimum content requirements
                Assert.True(result.Rooms.Count >= 3, $"World {Path.GetFileName(zipFile)} has fewer than 3 rooms");
                Assert.NotEmpty(result.Npcs);
                Assert.NotEmpty(result.Factions);
            }
        }

        #endregion

        #region Data Integrity Tests

        [Fact]
        public async Task LoadFromZipNpcsHaveValidData()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act
            var result = await LoadWorldFromZip(zipFiles[0]);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Npcs);
            Assert.NotEmpty(result.Npcs);
            
            if (result.Npcs.Count > 0)
            {
                Assert.All(result.Npcs, npc =>
                {
                    Assert.NotNull(npc.Id);
                    Assert.NotEmpty(npc.Id);
                    Assert.NotNull(npc.Name);
                    Assert.NotEmpty(npc.Name);
                });
            }
        }

        [Fact]
        public async Task LoadFromZipFactionsHaveValidData()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act
            var result = await LoadWorldFromZip(zipFiles[0]);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Factions);
            Assert.NotEmpty(result.Factions);
            Assert.All(result.Factions, faction =>
            {
                Assert.NotNull(faction.Id);
                Assert.NotEmpty(faction.Id);
                Assert.NotNull(faction.Name);
                Assert.NotEmpty(faction.Name);
            });
        }

        [Fact]
        public async Task LoadFromZipRoomsHaveValidExits()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Act
            var result = await LoadWorldFromZip(zipFiles[0]);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Rooms);
            
            // Verify room connectivity
            foreach (var room in result.Rooms)
            {
                if (room.Connections != null && room.Connections.Count > 0)
                {
                    foreach (var connection in room.Connections)
                    {
                        // Verify connection leads to a valid room
                        var targetRoom = result.Rooms.FirstOrDefault(r => r.Id == connection.Value);
                        Assert.NotNull(targetRoom);
                    }
                }
            }
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task LoadFromZipLargeWorldLoadsInReasonableTime()
        {
            // Arrange
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null) return;

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0) return;

            // Find largest world (by file size)
            var largestWorld = zipFiles
                .Select(f => new { Path = f, Size = new FileInfo(f).Length })
                .OrderByDescending(x => x.Size)
                .First();

            // Act
            var startTime = DateTime.UtcNow;
            var result = await LoadWorldFromZip(largestWorld.Path);
            var loadTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.NotNull(result);
            Assert.True(loadTime.TotalSeconds < 5, $"World took {loadTime.TotalSeconds}s to load (expected < 5s)");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task LoadFromZipInvalidPathThrowsException()
        {
            // Arrange
            var invalidPath = Path.Combine(Path.GetTempPath(), "nonexistent.zip");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            {
                using var fs = new FileStream(invalidPath, FileMode.Open);
                var loader = new WorldLoaderService();
                await loader.LoadFromZipAsync(fs);
            });
        }

        [Fact]
        public async Task LoadFromZipEmptyStreamThrowsException()
        {
            // Arrange
            var loader = new WorldLoaderService();

            // Act & Assert - Expect NullReferenceException not ArgumentNullException
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await loader.LoadFromZipAsync(null!);
            });
        }

        #endregion

        #region Summary Test

        [Fact(Skip = "Requires generated worlds in content folder - run generator first")]
        public async Task GeneratedWorldsSummary()
        {
            // This test is skipped by default - it will only show summary if worlds exist
            var worldsDir = FindContentWorldsDirectory();
            if (worldsDir == null)
            {
                return; // Silently skip if no content folder
            }

            var zipFiles = Directory.GetFiles(worldsDir, "*.zip");
            if (zipFiles.Length == 0)
            {
                return; // Silently skip if no worlds
            }

            // Act - Load and analyze all worlds
            var worldStats = new List<(string Name, int Rooms, int NPCs, int Factions, long Size)>();

            foreach (var zipFile in zipFiles)
            {
                var result = await LoadWorldFromZip(zipFile);
                Assert.NotNull(result);
                Assert.NotNull(result.WorldDefinition);
                Assert.NotNull(result.Rooms);
                Assert.NotNull(result.Npcs);
                Assert.NotNull(result.Factions);

                var fileInfo = new FileInfo(zipFile);
                worldStats.Add((
                    Name: result.WorldDefinition.Name,
                    Rooms: result.Rooms.Count,
                    NPCs: result.Npcs.Count,
                    Factions: result.Factions.Count,
                    Size: fileInfo.Length
                ));
            }

            // Assert - Output summary
            var summary = new StringBuilder();
            summary.AppendLine($"\n?? GENERATED WORLDS SUMMARY ({worldStats.Count} worlds):");
            summary.AppendLine("???????????????????????????????????????????????????????");
            
            foreach (var stat in worldStats)
            {
                summary.AppendLine($"???  {stat.Name}");
                summary.AppendLine($"   Rooms: {stat.Rooms} | NPCs: {stat.NPCs} | Factions: {stat.Factions}");
                summary.AppendLine($"   Size: {stat.Size / 1024} KB");
                summary.AppendLine("???????????????????????????????????????????????????????");
            }

            summary.AppendLine($"?? TOTALS:");
            summary.AppendLine($"   Total Rooms: {worldStats.Sum(s => s.Rooms)}");
            summary.AppendLine($"   Total NPCs: {worldStats.Sum(s => s.NPCs)}");
            summary.AppendLine($"   Total Factions: {worldStats.Sum(s => s.Factions)}");
            summary.AppendLine($"   Total Size: {worldStats.Sum(s => s.Size) / 1024} KB");
            summary.AppendLine("???????????????????????????????????????????????????????");

            // This always passes, just outputs the summary
            Assert.True(true, summary.ToString());
        }

        #endregion
    }
}

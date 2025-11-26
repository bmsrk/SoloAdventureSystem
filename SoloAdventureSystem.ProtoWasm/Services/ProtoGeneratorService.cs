using System.Threading.Tasks;

namespace SoloAdventureSystem.ProtoWasm;

public class ProtoGeneratorService
{
    public Task<object> GenerateMockAsync(int seed, int rooms)
    {
        // Simple mock world
        var rnd = new System.Random(seed);
        var world = new
        {
            Name = $"DemoWorld-{seed}",
            Rooms = System.Linq.Enumerable.Range(1, rooms).Select(i => new {
                Id = i,
                Name = $"Room {i}",
                Description = $"A room with {rnd.Next(1,5)} exits and {rnd.Next(0,3)} items."
            }).ToArray()
        };
        return Task.FromResult((object)world);
    }
}
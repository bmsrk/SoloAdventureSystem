
namespace SoloAdventureSystem.Engine.WorldLoader
{
    public class WorldState : IWorldState
    {
        private WorldModel? _world;
        public bool IsLoaded => _world != null;

        public void SetWorld(WorldModel world)
        {
            _world = world;
        }

        public WorldModel? GetWorld()
        {
            return _world;
        }
    }
}
namespace SoloAdventureSystem.Engine.WorldLoader
{
    public interface IWorldState
    {
        void SetWorld(WorldModel world);
        WorldModel? GetWorld();
        bool IsLoaded { get; }
    }
}
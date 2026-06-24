using Godot;

namespace Game.Spawning
{
    [GlobalClass]
    public partial class EnemySpawnEntryData : Resource
    {
        [Export] public PackedScene EnemyScene;
        [Export] public int Count = 1;
    }
}
using Godot;

namespace Game.Spawning
{
    [GlobalClass]
    public partial class MeteorSpawnEntryData : Resource
    {
        [Export] public PackedScene MeteorScene;
        [Export] public int Count = 1;
    }
}
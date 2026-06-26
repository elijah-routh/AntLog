using Godot;
using Godot.Collections;

namespace Game.Spawning
{
    [GlobalClass]
    public partial class EnemySpawnWaveData : Resource
    {
        [Export] public Array<EnemySpawnEntryData> Enemies = new();

        [ExportGroup("Timing")]
        [Export] public float StartDelay = 0.5f;
        [Export] public float DelayBetweenSpawns = 1.0f;

        [ExportGroup("Limits")]
        [Export] public int MaxAlive = 5;

        [ExportGroup("Wave Completion")]
        [Export] public bool WaitForAllEnemiesDead = true;

        [ExportGroup("Repeat")]
        [Export] public bool RepeatForever = false;
    }
}
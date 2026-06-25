using Godot;
using Godot.Collections;

namespace Game.Spawning
{
    [GlobalClass]
    public partial class MeteorSpawnWaveData : Resource
    {
        [ExportGroup("Wave")]
        [Export] public string DisplayName = "Meteor Wave";
        [Export] public float StartDelay = 0.0f;
        [Export] public float DelayBetweenSpawns = 3.0f;
        [Export] public int MaxAlive = 2;
        [Export] public bool WaitForAllMeteorsGone = true;

        [ExportGroup("Endless")]
        [Export] public bool RepeatForever = false;

        [ExportGroup("Difficulty Scaling")]
        [Export] public bool ScaleDifficulty = false;

        [Export] public float MinimumSpawnDelay = 0.75f;
        [Export] public float SpawnDelayReductionPerLoop = 0.15f;

        [Export] public int MaxAliveIncreaseEveryLoops = 2;
        [Export] public int MaxAliveIncreaseAmount = 1;
        [Export] public int MaxAliveCap = 8;

        [Export] public float DifficultyMultiplierPerLoop = 0.1f;
        [Export] public float MaxDifficultyMultiplier = 3.0f;

        [ExportGroup("Meteors")]
        [Export] public Array<MeteorSpawnEntryData> Meteors = new();
    }
}
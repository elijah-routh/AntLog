using Godot;

namespace Game.Spawning
{
    [GlobalClass]
    public partial class EnemySpawnPoint3D : Marker3D
    {
        [Export] public bool Enabled = true;

        [ExportGroup("Random Offset")]
        [Export] public float SpawnRadius = 0.0f;

        public Vector3 GetSpawnPosition(RandomNumberGenerator rng)
        {
            if (SpawnRadius <= 0.0f)
                return GlobalPosition;

            Vector2 randomCircle = rng.Randf() * SpawnRadius * Vector2.Right.Rotated(rng.RandfRange(0.0f, Mathf.Tau));

            return GlobalPosition + new Vector3(
                randomCircle.X,
                0.0f,
                randomCircle.Y
            );
        }
    }
}
using Godot;

namespace Game.Spawning
{
    public partial class MeteorSpawnPoint3D : Node3D
    {
        [Export] public bool Enabled = true;
        [Export] public float Radius = 0.0f;

        public Vector3 GetSpawnPosition(RandomNumberGenerator rng)
        {
            if (Radius <= 0.0f)
                return GlobalPosition;

            float angle = rng.RandfRange(0.0f, Mathf.Tau);
            float distance = Mathf.Sqrt(rng.Randf()) * Radius;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                0.0f,
                Mathf.Sin(angle) * distance
            );

            return GlobalPosition + offset;
        }
    }
}
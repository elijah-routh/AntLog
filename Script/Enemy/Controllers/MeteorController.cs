using Game.Components;
using Godot;

public partial class MeteorController : Node3D
{
    [ExportGroup("Movement")]
    [Export] public float FallSpeed = 4.0f;
    [Export] public Node3D TargetPoint;
    [Export] public float RotateSpeed = 2.0f;
    [Export] public float ImpactDistance = 0.5f;

    [ExportGroup("Explosion")]
    [Export] public float ExplosionRadius = 6.0f;
    [Export] public float ExplosionDamage = 25.0f;
    [Export] public PackedScene ExplosionVfx;

    [ExportGroup("References")]
    [Export] public HealthComponent Health;

    private Vector3 _targetPosition;
    private bool _hasTarget;
    private bool _hasExploded;

    private float _baseFallSpeed;

    public override void _Ready()
    {
        _baseFallSpeed = FallSpeed;

        if (TargetPoint != null)
        {
            Initialize(TargetPoint.GlobalPosition);
        }

        if (Health != null)
        {
            Health.Died += Explode;
        }
    }

    public void Initialize(Vector3 targetPosition, float difficultyMultiplier = 1.0f)
    {
        _targetPosition = targetPosition;
        _hasTarget = true;

        FallSpeed = _baseFallSpeed * difficultyMultiplier;

        if (!GlobalPosition.IsEqualApprox(_targetPosition))
        {
            LookAt(_targetPosition, Vector3.Up);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hasExploded || !_hasTarget)
            return;

        float dt = (float)delta;

        Vector3 direction = GlobalPosition.DirectionTo(_targetPosition);
        GlobalPosition += direction * FallSpeed * dt;

        RotateY(RotateSpeed * dt);

        if (GlobalPosition.DistanceTo(_targetPosition) <= ImpactDistance)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (_hasExploded)
            return;

        _hasExploded = true;

        SpawnExplosionVfx();
        DamageNearbyObjects();

        QueueFree();
    }

    private void SpawnExplosionVfx()
    {
        if (ExplosionVfx == null)
            return;

        Node3D instance = ExplosionVfx.Instantiate<Node3D>();
        GetTree().CurrentScene.AddChild(instance);
        instance.GlobalPosition = GlobalPosition;
    }

    private void DamageNearbyObjects()
    {
        // Later:
        // Use an Area3D explosion scene, or PhysicsDirectSpaceState3D
        // to find nearby IDamageable objects.
    }
}
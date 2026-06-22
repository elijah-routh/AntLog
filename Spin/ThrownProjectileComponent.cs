using Godot;
using Game.Entity;

public partial class ThrownProjectileComponent : Area3D
{
    [ExportGroup("References")]
    [Export] public Node3D ProjectileRoot;

    [ExportGroup("Projectile")]
    [Export] public float Lifetime = 2.0f;
    [Export] public float Damage = 25.0f;
    [Export] public bool StopOnHit = true;

    public bool IsActive { get; private set; }

    private Vector3 _velocity;
    private float _timer;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;
        Monitoring = false;

        if (ProjectileRoot == null)
            ProjectileRoot = GetParent<Node3D>();

        GD.Print($"[ThrownProjectile] Ready. Root: {ProjectileRoot?.Name}");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsActive || ProjectileRoot == null)
            return;

        float dt = (float)delta;

        _timer -= dt;

        if (_timer <= 0.0f)
        {
            StopProjectile();
            return;
        }

        ProjectileRoot.GlobalPosition += _velocity * dt;
    }

    public void Launch(Vector3 velocity)
    {
        _velocity = velocity;
        _timer = Lifetime;
        IsActive = true;
        Monitoring = true;

        GD.Print($"[ThrownProjectile] Launched with velocity: {_velocity}");
    }

    private void OnBodyEntered(Node3D body)
    {
        GD.Print($"[ThrownProjectile] Hit body: {body.Name}");
        Hit(body);
    }

    private void OnAreaEntered(Area3D area)
    {
        GD.Print($"[ThrownProjectile] Hit area: {area.Name}");
        Hit(area);
    }

    private void Hit(Node target)
    {
        if (!IsActive)
            return;

        if (ProjectileRoot != null)
        {
            if (target == ProjectileRoot)
                return;

            if (ProjectileRoot.IsAncestorOf(target))
                return;

            if (target.IsAncestorOf(ProjectileRoot))
                return;
        }

        if (target is IDamageable damageable)
        {
            damageable.TakeDamage(Damage);
            GD.Print($"[ThrownProjectile] Damaged {target.Name} for {Damage}");

            if (StopOnHit)
                StopProjectile();
        }
    }

    public void StopProjectile()
    {
        IsActive = false;
        _velocity = Vector3.Zero;
        Monitoring = false;

        GD.Print("[ThrownProjectile] Stopped.");
    }
}
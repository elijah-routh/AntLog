using Godot;
using Game.Entity;

public abstract partial class ProjectileBase : Area3D
{
    protected Vector3 Direction;
    protected WeaponData WeaponData;

    private float _lifeTimer;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;
    }

    public virtual void Initialize(Vector3 direction, WeaponData weaponData)
    {
        Direction = direction.Normalized();
        WeaponData = weaponData;
        _lifeTimer = weaponData.ProjectileLifetime;
    }

    public override void _PhysicsProcess(double delta)
    {
        _lifeTimer -= (float)delta;

        if (_lifeTimer <= 0)
        {
            QueueFree();
            return;
        }

        Move((float)delta);
    }

    protected abstract void Move(float delta);

    private void OnBodyEntered(Node3D body)
    {
        GD.Print($"Projectile hit body: {body.Name}");
        Hit(body);
    }

    private void OnAreaEntered(Area3D area)
    {
        GD.Print($"Projectile hit area: {area.Name}");
        Hit(area);
    }

    protected virtual void Hit(Node target)
    {
        if (target is IDamageable damageable)
            damageable.TakeDamage(WeaponData.Damage);

        QueueFree();
    }
}

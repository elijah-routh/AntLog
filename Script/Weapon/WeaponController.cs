using Game.Items;
using Godot;

public partial class WeaponController : Node3D
{
    [Export] public WeaponData CurrentWeapon;
    [Export] public Marker3D Barrel;

    private float _cooldownTimer;

    public override void _Process(double delta)
    {
        _cooldownTimer -= (float)delta;

        if (PlayerInput.ShootPressed)
        {
            TryShoot();
        }
    }

    public void TryShoot()
    {
        if (CurrentWeapon == null)
            return;

        if (CurrentWeapon.ProjectileScene == null)
            return;

        if (Barrel == null)
            return;

        if (_cooldownTimer > 0)
            return;

        Shoot();

        _cooldownTimer = CurrentWeapon.FireRate;
    }

    private void Shoot()
    {

        GD.Print("Shoot() called");

        var projectile = CurrentWeapon.ProjectileScene.Instantiate<ProjectileBase>();

        GetTree().CurrentScene.AddChild(projectile);

        projectile.GlobalTransform = Barrel.GlobalTransform;

        Vector3 direction = Barrel.GlobalTransform.Basis.Z;

        projectile.Initialize(direction, CurrentWeapon);
    }
}

using Godot;

[GlobalClass]
public partial class WeaponData : Resource
{
    [Export] public PackedScene ProjectileScene;
    [Export] public float Damage = 10;
    [Export] public float FireRate = 0.2f;
    [Export] public float BulletSpeed = 800;
    [Export] public int BulletsPerShot = 1;
    [Export] public float SpreadDegrees = 0;
    [Export] public float ProjectileLifetime = 3f;
}  
using Godot;
using System;

public partial class EnemyLaser : ProjectileBase
{
    protected override void Move(float delta)
    {
        GlobalPosition += Direction * WeaponData.BulletSpeed * delta;
    }
}

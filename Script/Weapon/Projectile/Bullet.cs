using Godot;
using System;

public partial class Bullet : ProjectileBase
{
    protected override void Move(float delta)
    {
        GlobalPosition += Direction * WeaponData.BulletSpeed * delta;
    }
}

using Godot;
using System;

public class Gun : StraightProjectileWeapon
{
    public Gun() : base(20.0f, 50.0f, 0.1f)
    {
        projectileScene = ResourceLoader.Load<PackedScene>("res://Scenes/Bullet.tscn");
    }
}

using Godot;
using System;

public class OverheatingGun : StraightProjectileWeapon
{
    [Export]
    private readonly float coolDownSpeed = 0.4f;
    [Export]
    private readonly float heatPerProjectile = 0.1f;

    private float heatLevel = 0.0f;
    private bool isOverheated = false;
    private ProgressBar heatLevelBar;

    protected override bool CanFire()
    {
        return base.CanFire() && !isOverheated;
    }

    public OverheatingGun() : base(20.0f, 50.0f, 0.1f)
    {
        projectileScene = ResourceLoader.Load<PackedScene>("res://Scenes/Bullet.tscn");
    }

    public override void _Ready()
    {
        base._Ready();
        heatLevelBar = GetTree().Root.FindNode("HeatLevel", owned: false) as ProgressBar;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!isTryingToFire || !CanFire())
        {
            heatLevel -= coolDownSpeed * delta;
            heatLevel = Mathf.Max(heatLevel, 0);
            if (heatLevel == 0)
                isOverheated = false;
        }

        heatLevelBar.Value = heatLevel;

        // Shoot if not overheating
        base._PhysicsProcess(delta);
    }


    protected override void OnProjectileFired()
    {
        heatLevel += heatPerProjectile;
        if (heatLevel >= 1)
        {
            isOverheated = true;
            heatLevel = 1;
        }
    }
}

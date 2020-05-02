using Godot;

/// <summary>
/// A gun that shoots straight projectiles that have a chance to mark (see <see cref="MarkStatus"/>) enemies.
/// Heats up per projectile shot, and will overheat
/// when the maximum heat level has been reached.
/// When the weapon is overheated, the weapon has to cool down all the way to be used again.
/// </summary>
public class OverheatingGun : StraightProjectileWeapon
{
    [Export]
    private readonly float coolDownSpeed = 0.4f;
    [Export]
    private readonly float heatPerProjectile = 0.1f;
    [Export]
    private readonly float markChance = 0.3f;
    [Export]
    public readonly float MarkDuration = 3.0f;

    private float heatLevel = 0.0f;
    private bool isOverheated = false;
    private ProgressBar heatLevelBar;
    private readonly RandomNumberGenerator rng;

    protected override bool CanFire()
    {
        return base.CanFire() && !isOverheated;
    }

    public OverheatingGun() : base(20.0f, 10.0f, 100.0f, 0.1f)
    {
        projectileScene = ResourceLoader.Load<PackedScene>("res://Scenes/Bullet.tscn");
        rng = new RandomNumberGenerator();
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

    public void Cool(float heat)
    {
        heatLevel = Mathf.Max(heatLevel - heat, 0);
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

    protected override void OnProjectileHit(Enemy enemy)
    {
        // TODO: Mark enemy
        rng.Randomize();
        if (rng.Randf() < markChance)
            enemy.ApplyStatus<MarkStatus>(MarkDuration);
        base.OnProjectileHit(enemy);
    }
}

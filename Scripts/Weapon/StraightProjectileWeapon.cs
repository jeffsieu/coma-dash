using Godot;

public abstract class StraightProjectileWeapon : AimableAttack
{
    // 0.5s cooldown
    protected readonly float coolDownDuration;
    protected readonly float projectileDamage;
    protected readonly float projectileSpeed;
    protected PackedScene projectileScene;
    protected Spatial projectileContainer;
    protected bool isTryingToFire = false;
    protected float cooldown = 0.0f;
    [Signal]
    public delegate void Fired(Projectile projectile);

    public StraightProjectileWeapon(float range, float projectileDamage, float projectileSpeed, float coolDownDuration) : base(range)
    {
        this.projectileDamage = projectileDamage;
        this.projectileSpeed = projectileSpeed;
        this.coolDownDuration = coolDownDuration;
    }

    protected virtual bool CanFire()
    {
        return cooldown <= 0;
    }

    public override void _Ready()
    {
        base._Ready();
        projectileContainer = GetTree().Root.GetNode<Spatial>("Level");
        Spatial projectileInstance = projectileScene.Instance() as Spatial;
        if (AimIndicator is StraightAimIndicator)
            (AimIndicator as StraightAimIndicator).Width = projectileInstance.Scale.x;
        if (AimIndicator is GeneralAimIndicator)
        {
            (AimIndicator as GeneralAimIndicator).IndicatorType = GeneralAimIndicator.AimIndicatorType.STRAIGHT;
            (AimIndicator as GeneralAimIndicator).Width = projectileInstance.Scale.x;
        }
        projectileInstance.QueueFree();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        cooldown = Mathf.Max(cooldown - delta, 0);

        if (isTryingToFire && CanFire())
        {
            cooldown = coolDownDuration;
            Projectile projectile = new Projectile(range, projectileSpeed, -GlobalTransform.basis.z.Normalized(), projectileScene)
            {
                GlobalTransform = GlobalTransform
            };
            projectileContainer.AddChild(projectile);
            projectile.Connect("hit_enemy", this, "OnProjectileHit");
            EmitSignal(nameof(Fired), projectile);
            OnProjectileFired();
        }
    }

    protected abstract void OnProjectileFired();

    protected virtual void OnProjectileHit(Enemy enemy)
    {
        enemy.Damage(projectileDamage);
    }

    protected override void OnAttackButtonPressed()
    {
        isTryingToFire = true;
    }

    protected override void OnAttackButtonReleased()
    {
        isTryingToFire = false;
    }
}

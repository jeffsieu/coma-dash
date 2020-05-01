using Godot;

public abstract class StraightProjectileWeapon : Weapon
{
    // 0.5s cooldown
    private readonly float coolDownDuration;
    private readonly float projectileSpeed;
    protected PackedScene projectileScene;
    private Spatial projectileContainer;
    private bool isShooting = false;
    private float cooldown = 0.0f;


    public StraightProjectileWeapon(float range, float projectileSpeed, float coolDownDuration) : base(range)
    {
        this.projectileSpeed = projectileSpeed;
        this.coolDownDuration = coolDownDuration;
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
        cooldown -= delta;
        cooldown = Mathf.Max(0, cooldown);

        if (isShooting && cooldown == 0)
        {
            cooldown = coolDownDuration;
            Spatial projectile = new Projectile(range, projectileSpeed, -GlobalTransform.basis.z.Normalized(), projectileScene)
            {
                GlobalTransform = GlobalTransform
            };
            projectileContainer.AddChild(projectile);
        }
    }

    protected override void OnAttackButtonPressed()
    {
        isShooting = true;
    }

    protected override void OnAttackButtonReleased()
    {
        isShooting = false;
    }
}

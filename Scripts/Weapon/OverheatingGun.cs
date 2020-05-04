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
    private float HeatLevel
    {
        get
        {
            return heatLevel;
        }
        set
        {
            heatLevel = value;
            if (heatLevelBarLeft != null)
            {

                Color progressBarColor = isOverheated ? maxHeatColor : minHeatColor.LinearInterpolate(maxHeatColor, HeatLevel);
                (heatLevelBarLeft.Get("custom_styles/fg") as StyleBoxFlat).BgColor = progressBarColor;
                (heatLevelBarRight.Get("custom_styles/fg") as StyleBoxFlat).BgColor = progressBarColor;

                heatLevelBarLeft.Value = value;
                heatLevelBarRight.Value = value;
            }
        }
    }

    private bool isOverheated = false;
    private ProgressBar heatLevelBarLeft;
    private ProgressBar heatLevelBarRight;
    private readonly int borderWidth = 8;
    private readonly int borderRadius = 4;
    private readonly Color maxHeatColor = new Color("#E86252");
    private readonly Color minHeatColor = new Color("#9CAFB7");
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
        heatLevelBarLeft = GetTree().Root.FindNode("HeatLevelLeft", owned: false) as ProgressBar;
        StyleBox fg = new StyleBoxFlat
        {
            BgColor = new Color("#F1AB86"),
            AntiAliasing = false,
            BorderColor = Colors.Transparent,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = 0,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = borderRadius,
        };
        StyleBox bg = new StyleBoxFlat
        {
            BgColor = Colors.Black,
            AntiAliasing = false,
            BorderColor = Colors.Black,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = 0,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = borderRadius,
        };
        heatLevelBarLeft.AddStyleboxOverride("fg", fg);
        heatLevelBarLeft.AddStyleboxOverride("bg", bg);

        heatLevelBarRight = GetTree().Root.FindNode("HeatLevelRight", owned: false) as ProgressBar;
        heatLevelBarRight.AddStyleboxOverride("fg", fg);
        heatLevelBarRight.AddStyleboxOverride("bg", bg);

        heatLevelBarLeft.RectScale = new Vector2(-1, 1);
        heatLevelBarLeft.RectMinSize = new Vector2(0, 20);
        heatLevelBarRight.RectMinSize = new Vector2(0, 20);
        GD.Print(heatLevelBarLeft.RectMinSize);
        heatLevelBarLeft.Connect("resized", this, "BarResized");
    }

    private void BarResized()
    {
        heatLevelBarLeft.RectPivotOffset = heatLevelBarLeft.RectSize / 2;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!isTryingToFire || !CanFire())
        {
            HeatLevel = Mathf.Max(HeatLevel - coolDownSpeed * delta, 0);
            if (HeatLevel == 0)
                isOverheated = false;
        }


        // Shoot if not overheating
        base._PhysicsProcess(delta);
    }

    public void Cool(float heat)
    {
        HeatLevel = Mathf.Max(HeatLevel - heat, 0);
    }

    protected override void OnProjectileFired()
    {
        HeatLevel += heatPerProjectile;
        if (HeatLevel >= 1)
        {
            isOverheated = true;
            HeatLevel = 1;
        }
    }

    protected override void OnProjectileHit(Enemy enemy)
    {
        rng.Randomize();
        if (rng.Randf() < markChance)
            enemy.ApplyStatus<MarkStatus>(MarkDuration);
        base.OnProjectileHit(enemy);
    }
}

using System;
using System.Collections.Generic;
using Godot;

public abstract class HealthEntity : RigidBody, IStatusHolder
{
    private readonly Dictionary<Type, Status> statuses;

    public float Health
    {
        get
        {
            return health;
        }
        protected set
        {
            if (healthBar != null)
                healthBar.Value = value;
            if (whiteHealthBar != null && value < health)
            {
                tween.RemoveAll();
                tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;
                tween.InterpolateProperty(whiteHealthBar, "value", null, value, whiteBarAnimationDuration, Tween.TransitionType.Linear, Tween.EaseType.In);
                tween.Start();
            }
            health = value;
        }
    }

    public float MaxHealth
    {
        get
        {
            return maxHealth;
        }
        protected set
        {
            maxHealth = value;
            Health = value;
            if (healthBar != null)
                healthBar.MaxValue = value;
            if (whiteHealthBar != null)
                whiteHealthBar.MaxValue = value;
        }
    }

    public float RegenerationDelay
    {
        get; protected set;
    } = 0;

    public float RegenerationRate
    {
        get; protected set;
    } = 0;

    public Color HealthBarColor
    {
        set
        {
            healthBarStyleBox.BgColor = value;
            whiteHealthBarStyleBox.BgColor = value.Lightened(0.9f);
        }
    }

    private static readonly int borderWidth = 3;
    private static readonly int borderRadius = 3;
    private static readonly Vector2 healthBarSize = new Vector2(50, 15);
    private static readonly Vector2 regenerationBarSize = new Vector2(50, 10);
    private static readonly float whiteBarAnimationDuration = 0.5f;
    private static readonly Color defaultHealthBarColor = new Color("#F1AB86");
    private static readonly Color regenerationBarColor = new Color("#4FF05A");

    protected Vector3 healthBarPositionOffset;
    protected ProgressBar healthBar;
    protected ProgressBar whiteHealthBar;
    protected ProgressBar regenerationBar;
    protected StyleBoxFlat healthBarStyleBox;
    protected StyleBoxFlat whiteHealthBarStyleBox;
    protected VBoxContainer column;
    protected Tween tween;

    private float health;
    private float maxHealth;
    private float deltaUntilRegeneration = 0;

    private Camera camera;

    public HealthEntity()
    {
        statuses = new Dictionary<Type, Status>();
        AddUserSignal("died");
    }

    public override void _Ready()
    {
        // Collide with weapons
        SetCollisionMaskBit(ColLayer.Bit.Projectiles, true);

        camera = GetTree().Root.GetCamera();
        Health = maxHealth;
        healthBar = new ProgressBar
        {
            Value = health,
            MinValue = 0,
            MaxValue = maxHealth,
            RectMinSize = healthBarSize,
            RectSize = healthBarSize,
            PercentVisible = false
        };
        healthBarStyleBox = new StyleBoxFlat
        {
            BgColor = defaultHealthBarColor,
            AntiAliasing = false,
            BorderColor = Colors.Transparent,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = borderRadius,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = borderRadius,
            CornerRadiusTopRight = borderRadius
        };
        healthBar.AddStyleboxOverride("fg", healthBarStyleBox);
        healthBar.AddStyleboxOverride("bg", new StyleBoxFlat
        {
            BgColor = Colors.Transparent
        });

        whiteHealthBar = new ProgressBar
        {
            Value = health,
            MinValue = 0,
            MaxValue = maxHealth,
            RectMinSize = healthBarSize,
            RectSize = healthBarSize,
            PercentVisible = false
        };
        whiteHealthBarStyleBox = new StyleBoxFlat
        {
            AntiAliasing = false,
            BgColor = defaultHealthBarColor.Lightened(0.9f),
            BorderColor = Colors.Transparent,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = borderRadius,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = borderRadius,
            CornerRadiusTopRight = borderRadius
        };
        whiteHealthBar.AddStyleboxOverride("fg", whiteHealthBarStyleBox);
        whiteHealthBar.AddStyleboxOverride("bg", new StyleBoxFlat
        {
            BgColor = Colors.Black,
            BorderColor = Colors.Black,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = borderRadius,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = borderRadius,
            CornerRadiusTopRight = borderRadius
        });

        regenerationBar = new ProgressBar
        {
            Value = 0,
            MinValue = 0,
            MaxValue = 1,
            RectMinSize = regenerationBarSize,
            RectSize = regenerationBarSize,
            PercentVisible = false
        };
        StyleBoxFlat regenerationBarStyleBox = new StyleBoxFlat
        {
            BgColor = regenerationBarColor,
            AntiAliasing = false,
            BorderColor = Colors.Black,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = borderRadius,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = borderRadius,
            CornerRadiusTopRight = borderRadius
        };
        regenerationBar.AddStyleboxOverride("fg", regenerationBarStyleBox);
        regenerationBar.AddStyleboxOverride("bg", new StyleBoxFlat
        {
            BgColor = Colors.Black,
            AntiAliasing = true,
            BorderColor = Colors.Black,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = borderRadius,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = borderRadius,
            CornerRadiusTopRight = borderRadius
        });

        tween = new Tween();
        AddChild(tween);

        Node2D statusBarContainer = new Node2D();
        column = new VBoxContainer();

        CenterContainer healthBarContainer = new CenterContainer();
        healthBarContainer.AddChild(whiteHealthBar);
        healthBarContainer.AddChild(healthBar);

        CenterContainer regenerationBarContainer = new CenterContainer();
        regenerationBarContainer.AddChild(regenerationBar);

        column.AddConstantOverride("separation", 0);
        column.AddChild(regenerationBarContainer);
        column.AddChild(healthBarContainer);

        statusBarContainer.AddChild(column);
        statusBarContainer.ZIndex = -1;

        AddChild(statusBarContainer);
    }

    public override void _Process(float delta)
    {
        UpdateStatusBars(delta);
    }

    protected void UpdateStatusBars(float delta)
    {
        Vector2 positionOnScreen = camera.UnprojectPosition(GlobalTransform.origin + healthBarPositionOffset);
        if (Health > 0)
        {
            Vector2 size = column.RectSize;
            Vector2 scale = column.RectScale;

            Vector2 healthBarPosition = positionOnScreen + new Vector2(-size.x * scale.x / 2, -size.y * scale.y) + Vector2.Up * 10;

            column.RectPosition = healthBarPosition;
        }

        deltaUntilRegeneration = Mathf.Max(deltaUntilRegeneration - delta, 0);
        if (Health < MaxHealth)
        {
            regenerationBar.Value = RegenerationDelay > 0 ? 1 - deltaUntilRegeneration / RegenerationDelay : 1;

            if (deltaUntilRegeneration == 0)
            {
                Health = Mathf.Min(Health + RegenerationRate * delta, MaxHealth);
            }
        }

        bool showRegenBar = regenerationBar.Value != regenerationBar.MaxValue && Health < MaxHealth && RegenerationRate > 0;
        Color m = regenerationBar.Modulate;
        m.a = showRegenBar ? 1 : 0;
        regenerationBar.Modulate = m;
    }

    public void ResetHealthBarColor()
    {
        healthBarStyleBox.BgColor = defaultHealthBarColor;
        whiteHealthBarStyleBox.BgColor = defaultHealthBarColor.Lightened(0.9f);
    }

    public virtual void Damage(float damage)
    {
        if (Health == 0)
            return;

        Health = Mathf.Max(Health - damage, 0);
        deltaUntilRegeneration = RegenerationDelay;

        if (Health == 0)
        {
            whiteHealthBar.Visible = false;
            healthBar.Visible = false;
            tween.RemoveAll();
            statuses.Clear();
            Die();
        }
    }

    protected virtual void Die()
    {
        EmitSignal("died", this);
        CollisionMask = ColLayer.Environment;
    }

    public bool HasStatus<S>() where S : Status
    {
        return GetStatusOrNull<S>() != null;
    }

    public S GetStatusOrNull<S>() where S : Status
    {
        if (statuses.ContainsKey(typeof(S)))
            return statuses[typeof(S)] as S;
        return null;
    }

    public float GetPercentLeft<S>() where S : Status
    {
        S status = GetStatusOrNull<S>();
        if (status == null)
            return 0;
        return status.TimeLeft / status.Duration;
    }

    public void ApplyStatus<S>(float duration) where S : Status, new()
    {
        S currentStatus = GetStatusOrNull<S>();
        if (currentStatus != null)
        {
            // Reset the duration of currently applied status
            currentStatus.TimeLeft = duration;
        }
        else
        {
            S status = new S
            {
                TimeLeft = duration
            };
            statuses[typeof(S)] = status;
            AddChild(status);
        }


        if (typeof(S) == typeof(MarkStatus))
        {
            HealthBarColor = new Color("#32A852");
        }
    }

    public void OnStatusEnd(Status status)
    {
        if (status is MarkStatus)
        {
            ResetHealthBarColor();
        }
        statuses[status.GetType()] = null;
    }
}

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
            health = value;
            if (healthBar != null)
                healthBar.Value = value;
            if (whiteHealthBar != null)
            {
                tween.RemoveAll();
                tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;
                tween.InterpolateProperty(whiteHealthBar, "value", null, value, whiteBarAnimationDuration, Tween.TransitionType.Linear, Tween.EaseType.In);
                tween.Start();
            }

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
    private static readonly Vector2 healthBarSize = new Vector2(50, 10);
    private static readonly float whiteBarAnimationDuration = 0.5f;
    private static readonly Color defaultHealthBarColor = new Color("#F1AB86");

    protected Vector3 healthBarPositionOffset;
    protected ProgressBar healthBar;
    protected ProgressBar whiteHealthBar;
    protected StyleBoxFlat healthBarStyleBox;
    protected StyleBoxFlat whiteHealthBarStyleBox;
    protected Tween tween;
    private float health;
    private float maxHealth;
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

        tween = new Tween();
        AddChild(tween);
        Node2D healthBarContainer = new Node2D();
        healthBarContainer.AddChild(whiteHealthBar);
        healthBarContainer.AddChild(healthBar);
        healthBarContainer.ZIndex = -1;
        AddChild(healthBarContainer);
    }

    public override void _PhysicsProcess(float delta)
    {
        DisplayHealthBar();
    }

    private void DisplayHealthBar()
    {
        Vector2 positionOnScreen = camera.UnprojectPosition(GlobalTransform.origin + healthBarPositionOffset);
        if (Health > 0)
        {
            Vector2 size = healthBar.RectSize;
            Vector2 scale = healthBar.RectScale;

            Vector2 healthBarPosition = positionOnScreen + new Vector2(-size.x * scale.x / 2, -size.y * scale.y) + Vector2.Up * 10;

            healthBar.RectPosition = healthBarPosition;
            whiteHealthBar.RectPosition = healthBarPosition;
        }
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

        if (Health == 0)
        {
            whiteHealthBar.QueueFree();
            healthBar.QueueFree();
            tween.QueueFree();
            statuses.Clear();
            Die();
        }
    }

    protected virtual void Die()
    {
        EmitSignal("died", this);
        SetCollisionMaskBit(ColLayer.Bit.Projectiles, false);
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

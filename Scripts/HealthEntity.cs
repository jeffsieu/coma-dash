using Godot;
using System;
using System.Collections.Generic;

public abstract class HealthEntity : KinematicBody, IStatusHolder
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
        }
    }

    private static readonly int borderWidth = 3;
    private static readonly int borderRadius = 3;
    private static readonly Vector2 healthBarSize = new Vector2(50, 10);

    protected ProgressBar healthBar;
    private float health;
    private float maxHealth;
    private Camera camera;

    public HealthEntity()
    {
        statuses = new Dictionary<Type, Status>();
    }

    public override void _Ready()
    {
        camera = GetTree().Root.GetCamera();
        healthBar = new ProgressBar
        {
            Value = health,
            MinValue = 0,
            MaxValue = maxHealth,
            RectSize = healthBarSize,
            PercentVisible = false
        };
        Health = maxHealth;
        healthBar.AddStyleboxOverride("fg", new StyleBoxFlat
        {
            BgColor = new Color("#F1AB86"),
            BorderColor = Colors.Black,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            CornerRadiusBottomLeft = borderRadius,
            CornerRadiusBottomRight = borderRadius,
            CornerRadiusTopLeft = borderRadius,
            CornerRadiusTopRight = borderRadius,
        });
        healthBar.AddStyleboxOverride("bg", new StyleBoxFlat
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
            CornerRadiusTopRight = borderRadius,
        });
        AddChild(healthBar);
    }

    public override void _PhysicsProcess(float delta)
    {
        DisplayHealthBar();
    }

    private void DisplayHealthBar()
    {
        Vector2 positionOnScreen = camera.UnprojectPosition(GlobalTransform.origin);

        var size = healthBar.RectSize;
        var scale = healthBar.RectScale;
        healthBar.SetPosition(positionOnScreen + new Vector2(-size.x * scale.x / 2, -size.y * scale.y) + Vector2.Up * 10);
    }

    public virtual void Damage(float damage)
    {
        Health = Mathf.Max(Health - damage, 0);

        if (Health == 0)
            Die();
    }

    protected abstract void Die();

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
    }

    public void OnStatusEnd(Status status)
    {
        statuses[status.GetType()] = null;
    }
}

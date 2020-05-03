using Godot;

/// <summary>
/// A class representing attacks that require aiming.
/// </summary>
public abstract class AimableAttack : Spatial
{
    public Vector2 WeightedAttackDirection
    {
        set
        {
            AimIndicator.WeightedAttackDirection = value;
        }
    }

    [Export]
    public float Range
    {
        get
        {
            return range;
        }
        set
        {
            range = value;
            if (AimIndicator != null)
                AimIndicator.Range = value;
        }
    }
    public bool IsAttackButtonPressed
    {
        set
        {
            if (isAttackButtonPressed != value)
            {
                if (isAttackButtonPressed)
                {
                    OnAttackButtonReleased();
                }
                else
                {
                    OnAttackButtonPressed();
                }
            }
            isAttackButtonPressed = value;
        }
    }

    public AimIndicator AimIndicator;
    protected float range;
    protected bool isAttackButtonPressed;

    public AimableAttack(float range)
    {
        this.range = range;
    }

    public override void _Ready()
    {
        AimIndicator = GetNodeOrNull<AimIndicator>("AimIndicator");
        if (AimIndicator == null)
        {
            AimIndicator = new GeneralAimIndicator();
            AddChild(AimIndicator);
        }
        AimIndicator.Range = range;
    }

    public Vector2 GetWeightedAttackDirectionFromMouseDisplacement(Vector3 mouseDisplacement)
    {
        Vector2 mouseDisplacement2D = new Vector2(mouseDisplacement.x, mouseDisplacement.z);
        mouseDisplacement2D = (mouseDisplacement2D / range).Clamped(1.0f);
        return mouseDisplacement2D;
    }

    protected abstract void OnAttackButtonPressed();

    protected abstract void OnAttackButtonReleased();
}

using Godot;

/// <summary>
/// <para>
/// All in one version of <see cref="AimIndicator"/>. Exposes an enum for us to choose between
/// <see cref="ConicalAimIndicator"/>, <see cref="DetachedCircularAimIndicator"/> and <see cref="StraightAimIndicator"/>.
/// </para>
/// <para>
/// This is used to easily switch between aim indicators.
/// </para>
/// </summary>

public class GeneralAimIndicator : AimIndicator
{
    [Export(PropertyHint.Range, "0, 360")]
    public float SpreadDegrees
    {
        get
        {
            return spreadDegrees;
        }
        set
        {
            spreadDegrees = value;
            RebuildIndicator();
        }
    }
    [Export]
    public float Radius
    {
        get
        {
            return radius;
        }
        set
        {
            radius = value;
            RebuildIndicator();
        }
    }
    [Export]
    public float Width
    {
        get
        {
            return width;
        }
        set
        {
            width = value;
            RebuildIndicator();
        }
    }

    [Export(PropertyHint.Enum)]
    public AimIndicatorType IndicatorType
    {
        get
        {
            return indicatorType;
        }
        set
        {
            indicatorType = value;
            RebuildIndicator();
        }
    }

    public enum AimIndicatorType
    {
        CONICAL, DETACHED_CIRCULAR, STRAIGHT
    }

    protected float spreadDegrees = 90.0f;
    protected float radius = 2.5f;
    protected float width = 2.0f;
    private AimIndicatorType indicatorType = AimIndicatorType.CONICAL;

    protected override void BuildIndicator()
    {
        switch (indicatorType)
        {
            case AimIndicatorType.CONICAL:
                indicator = new ConicalAimIndicator
                {
                    Range = range,
                    Height = height,
                    SpreadDegrees = spreadDegrees
                };
                break;
            case AimIndicatorType.DETACHED_CIRCULAR:
                indicator = new DetachedCircularAimIndicator
                {
                    Range = range,
                    Height = height,
                    Radius = radius
                };
                break;
            case AimIndicatorType.STRAIGHT:
                indicator = new StraightAimIndicator
                {
                    Range = range,
                    Height = height,
                    Width = width
                };
                break;
            default:
                indicator = new ConicalAimIndicator
                {
                    Range = range,
                    Height = height,
                    SpreadDegrees = spreadDegrees
                };
                break;
        }
    }

    protected override void UpdateTransform(Vector2 weightedAttackDirection)
    {
    }

    protected override void UpdateIndicatorTransform(Vector2 weightedAttackDirection)
    {
        (indicator as AimIndicator).WeightedAttackDirection = weightedAttackDirection;
    }
}

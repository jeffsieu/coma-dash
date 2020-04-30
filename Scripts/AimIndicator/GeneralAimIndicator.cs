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

    public enum AimIndicatorType
    {
        CONICAL, DETACHED_CIRCULAR, STRAIGHT
    }

    [Export(PropertyHint.Enum)]
    public AimIndicatorType IndicatorType
    {
        set
        {
            indicatorType = value;
            BuildIndicator();
        }
        get
        {
            return indicatorType;
        }
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
                indicator = new ConicalAimIndicator();
                break;
            case AimIndicatorType.DETACHED_CIRCULAR:
                indicator = new DetachedCircularAimIndicator();
                break;
            case AimIndicatorType.STRAIGHT:
                indicator = new StraightAimIndicator();
                break;
            default:
                indicator = new ConicalAimIndicator();
                break;
        }
    }

    protected override void UpdateTransform(Vector2 correctedJoyAxis)
    {
        GD.Print("Sir");
    }

    protected override void UpdateIndicatorTransform(Vector2 correctedJoyAxis)
    {
        (indicator as AimIndicator).JoyAxis = correctedJoyAxis;
    }
}

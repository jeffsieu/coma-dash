using Godot;

/// <summary>
/// <para>
/// An <see cref="AimIndicator"/> that displays a conical shaped indicator of a given angle that originates from the player.
/// </para>
/// <para>
/// This is used for melee weapons or weapons that have a conical trajectory.
/// </para>
/// </summary>

public class ConicalAimIndicator : AimIndicator
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

    protected float spreadDegrees = 90.0f;

    protected override void BuildIndicator()
    {
        indicator = new CSGPolygon
        {
            Polygon = new Vector2[] { Vector2.Zero, new Vector2(0, height), new Vector2(range, height), new Vector2(range, 0) },
            Mode = CSGPolygon.ModeEnum.Spin,
            SpinDegrees = spreadDegrees,
            SpinSides = (int)(spreadDegrees / 5),
            RotationDegrees = new Vector3(0, -spreadDegrees / 2, 0),
            Translation = new Vector3(0, height / 2, 0),
            Material = material
        };
    }
}

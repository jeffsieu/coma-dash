using Godot;

/// <summary>
/// <para>
/// An <see cref="AimIndicator"/> that displays a circle that is not centred on the player.
/// </para>
/// 
/// <para>
/// This is used for area-of-effect weapons that could be thrown away from the player.
/// As such. its position is determined by how far the player aims the projectile.
/// </para>
/// </summary>
public class DetachedCircularAimIndicator : AimIndicator
{
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

    private float radius = 2.5f;

    protected override void BuildIndicator()
    {
        indicator = new CSGCylinder
        {
            Radius = radius,
            Height = height,
            Sides = 16,
            SmoothFaces = true,
            Translation = new Vector3(0, height / 2, 0),
            Material = material
        };
    }

    protected override void UpdateIndicatorTransform(Vector2 correctedJoyAxis)
    {
        GD.Print(correctedJoyAxis);
        GD.Print(correctedJoyAxis.Length());
        indicator.Translation = new Vector3(correctedJoyAxis.Length() * range, height / 2, 0);
    }
}

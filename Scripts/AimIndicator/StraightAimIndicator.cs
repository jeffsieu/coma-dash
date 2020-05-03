using Godot;

/// <summary>
/// <para>
/// An <see cref="AimIndicator"/> that displays a straight line of a given width that originates from the player
/// </para>
/// <para>
/// This is used for weapons with straight trajectories.
/// </para>
/// </summary>
public class StraightAimIndicator : AimIndicator
{
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

    protected float width = 2.0f;

    protected override void BuildIndicator()
    {
        indicator = new CSGBox
        {
            Width = width,
            Height = height,
            Depth = range,
            Material = material,
            Translation = new Vector3(0, height, -range / 2)
        };
    }
}

using System;
using Godot;

/// <summary>
/// <para>
/// A <see cref="Spatial"/> that displays a mesh representing the trajectory of a <see cref="Weapon"/>.
/// The mesh is positioned such that it rests on, and is projected outward from the origin of the <see cref="AimIndicator"/>.
/// </para>
///
/// The following classes implement this class
/// <seealso cref="ConicalAimIndicator"/>,
/// <seealso cref="DetachedCircularAimIndicator"/>,
/// <seealso cref="GeneralAimIndicator"/>,
/// <seealso cref="StraightAimIndicator"/>
/// </summary>
public abstract class AimIndicator : Spatial
{
    public float Range
    {
        get
        {
            return range;
        }
        set
        {
            range = value;
            RebuildIndicator();
        }
    }

    [Export]
    public float Height
    {
        get
        {
            return height;
        }
        set
        {
            height = value;
            RebuildIndicator();
        }
    }

    [Export]
    public Material Material
    {
        get
        {
            return material;
        }
        set
        {
            material = value;
            RebuildIndicator();
        }
    }

    public Vector2 JoyAxis
    {
        set
        {
            UpdateTransform(value);
            UpdateIndicatorTransform(value);
        }
    }

    protected float range = 10.0f;
    protected float height = 0.1f;
    protected Material material = new SpatialMaterial()
    {
        AlbedoColor = new Color(1, 1, 1, 0.5f),
        FlagsTransparent = true
    };
    protected Spatial indicator;

    public override void _Ready()
    {
        RebuildIndicator();
    }

    protected abstract void BuildIndicator();

    protected void RebuildIndicator()
    {
        indicator?.QueueFree();
        BuildIndicator();
        AddChild(indicator);
    }

    protected virtual void UpdateTransform(Vector2 correctedJoyAxis)
    {
        if (correctedJoyAxis.Length() == 0)
            indicator.Hide();
        else
            indicator.Show();

        float attackAngleDegrees = (float)(new Vector2(correctedJoyAxis.x, -correctedJoyAxis.y).Angle() * 180 / Math.PI);
        RotationDegrees = new Vector3(0, attackAngleDegrees, 0);
    }

    protected virtual void UpdateIndicatorTransform(Vector2 correctedJoyAxis)
    {

    }
}

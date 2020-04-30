using Godot;

public class Weapon : Spatial
{
    public Vector2 AttackDirection
    {
        set
        {
            AimIndicator.JoyAxis = value;
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

    public AimIndicator AimIndicator;
    private float range = 10.0f;

    public override void _Ready()
    {
        AimIndicator = GetNode<AimIndicator>("AimIndicator");
        AimIndicator.Range = range;
    }

    public Vector2 GetAttackJoyAxisFromMouseDisplacement(Vector3 mouseDisplacement)
    {
        Vector2 mouseDisplacement2D = new Vector2(mouseDisplacement.x, mouseDisplacement.z);
        mouseDisplacement2D = (mouseDisplacement2D / range).Clamped(1.0f);
        return mouseDisplacement2D;
    }
}

using Godot;

public class Enemy : HealthEntity
{
    public Vector3 Velocity;
    protected SpatialMaterial material;
    protected Player player;

    public Enemy()
    {
        MaxHealth = 100;
    }

    public override void _Ready()
    {
        base._Ready();
        CollisionLayer = ColLayer.Enemies;
        SetCollisionMaskBit(ColLayer.Bit.Enemies, true);
        material = new SpatialMaterial()
        {
            AlbedoColor = Colors.White,
            FlagsTransparent = false
        };

        MeshInstance cylinder = GetNode<MeshInstance>("MeshInstance");
        cylinder.SetSurfaceMaterial(0, material);

        player = GetTree().Root.GetNode<Spatial>("Level").GetNode<Player>("Player");

        RegenerationRate = 0.0f;
    }

    public override void _Process(float delta)
    {
        Color baseColor = Colors.Red;
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - Health / 100);
        material.AlbedoColor = healthColor;
        UpdateStatusBars(delta);
    }

    protected Vector3 GetNextPointToTarget()
    {
        return GlobalTransform.origin + GlobalTransform.origin.DirectionTo(player.GlobalTransform.origin);
    }

    protected override void Die()
    {
        base.Die();
        material.FlagsTransparent = true;
        Tween tween = new Tween();
        AddChild(tween);
        tween.InterpolateProperty(material, "albedo_color:a", 0.5f, 0, 1.0f);
        tween.InterpolateCallback(this, 1.0f, "queue_free");
        tween.Start();
    }
}

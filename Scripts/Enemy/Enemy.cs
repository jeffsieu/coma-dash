using Godot;

public class Enemy : HealthEntity
{
    public Vector3 Velocity;
    private readonly float gravity = 4.85f;
    private SpatialMaterial material;

    public Enemy()
    {
        MaxHealth = 100;
    }

    public override void _Ready()
    {
        base._Ready();
        material = new SpatialMaterial()
        {
            AlbedoColor = Colors.White,
            FlagsTransparent = false
        };

        MeshInstance cylinder = GetNode<MeshInstance>("MeshInstance");
        cylinder.SetSurfaceMaterial(0, material);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        Velocity.x *= 0.9f;
        Velocity.y -= gravity;
        Velocity.z *= 0.9f;
        Velocity = MoveAndSlide(Velocity);
        Color baseColor = Colors.Red.LinearInterpolate(Colors.Blue, GetPercentLeft<MarkStatus>());
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - Health / 100);
        material.AlbedoColor = healthColor;
    }

    protected override void Die()
    {
        material.FlagsTransparent = true;
        Tween tween = new Tween();
        AddChild(tween);
        tween.InterpolateProperty(material, "albedo_color:a", 0.5f, 0, 1.0f);
        tween.InterpolateCallback(this, 1.0f, "queue_free");
        tween.Start();
    }
}

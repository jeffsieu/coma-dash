using Godot;

public class Enemy : HealthEntity
{
    public Vector3 Velocity;
    protected readonly float gravity = 4.85f;
    private SpatialMaterial material;

    protected Player player;
    protected float acceleration = 5f;
    protected float angularSpeed = 3f;

    public Enemy()
    {
        Velocity = Vector3.Zero;
    }

    public override void _Ready()
    {
        base._Ready();
        CollisionLayer = ColLayer.Enemies;
        material = new SpatialMaterial()
        {
            AlbedoColor = Colors.White,
            FlagsTransparent = false
        };

        MeshInstance cylinder = GetNode<MeshInstance>("MeshInstance");
        cylinder.SetSurfaceMaterial(0, material);
        player = GetTree().Root.GetNode("Level").GetNodeOrNull<Player>("Player");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        Velocity.x *= 0.9f;
        Velocity.y -= gravity;
        Velocity.z *= 0.9f;
        Velocity = MoveAndSlide(Velocity);
        Color baseColor = Colors.Red;
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - Health / 100);
        material.AlbedoColor = healthColor;
    }

    // Currently it is just the player's position since there are no obstacles
    // Once the level generator is done, we can use AStar, and the return value will be the next point to move to
    // in pathfinding to player
    protected Vector3 NextPointInPath()
    {
        return player.GlobalTransform.origin;
    }

    protected void MoveToPlayer(float delta)
    {
        Vector3 moveDirection = NextPointInPath() - GlobalTransform.origin;
        Velocity += acceleration * moveDirection.Normalized() * delta;
    }

    protected void RotateToPlayer(float delta)
    {
        Vector3 directionToPlayer = NextPointInPath() - GlobalTransform.origin;
        float angle = Mathf.Pi - GlobalTransform.basis.z.AngleTo(directionToPlayer);
        float angleSign = -Mathf.Sign(GlobalTransform.basis.x.Dot(directionToPlayer));
        float rotationAngle = Mathf.Min(angularSpeed * delta, angle) * angleSign;
        Rotate(Vector3.Up, rotationAngle);
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
using Godot;

public abstract class Enemy : HealthEntity
{
    public Vector3 Velocity;
    protected readonly float gravity = 4.85f;

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
        player = GetTree().Root.GetNode("Level").GetNodeOrNull<Player>("Player");
        CollisionLayer = 4;
        CollisionMask = 1 + 2 + 8;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
    }

    // Currently it is just the player's position since there are no obstacles
    // Once the level generator is done, we can use AStar, and the return value will be the next point to move to
    // in pathfinding to player
    protected Vector3 NextPointInPath()
    {
        return player.GlobalTransform.origin;
    }

    protected Vector3 GetDirectionToPlayer()
    {
        return GlobalTransform.origin.DirectionTo(NextPointInPath());
    }

    protected void LookAtPlayerDirection(float delta)
    {
        Vector3 directionToPlayer = GetDirectionToPlayer();
        float angle = Mathf.Pi - GlobalTransform.basis.z.AngleTo(directionToPlayer);
        float angleSign = -Mathf.Sign(GlobalTransform.basis.x.Dot(directionToPlayer));
        float rotationAngle = Mathf.Min(angularSpeed * delta, angle) * angleSign;
        Rotate(Vector3.Up, rotationAngle);
    }
}

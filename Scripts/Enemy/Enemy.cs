using Godot;

public class Enemy : HealthEntity
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
    }
}

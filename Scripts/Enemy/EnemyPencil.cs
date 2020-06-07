using System;
using Godot;

public class EnemyPencil : RigidBody
{
    public float JumpImpulse = 200.0f;
    public float JumpAngleDeg = 60.0f;
    public PhysicsBody player;
    public CollisionShape collisionShape;
    public bool onGround = false;

    public float pauseReset = 3.0f;
    public float pause = 3.0f;

    public override void _Ready()
    {
        collisionShape = GetNode<CollisionShape>("CollisionShape");
        player = GetTree().Root.GetNode("Level").GetNode<PhysicsBody>("Player");
        pause = pauseReset;
        Connect("body_entered", this, "BodyEnteredSelf");
        Connect("body_exited", this, "BodyExitedSelf");
    }

    private bool IsOnGround()
    {
        return onGround;
    }

    private void BodyEnteredSelf(Node node)
    {
        PhysicsBody body = node as PhysicsBody;
        if (body.CollisionLayer == 1 && body.Name == "Floor")
            onGround = true;
    }

    private void BodyExitedSelf(Node node)
    {
        PhysicsBody body = node as PhysicsBody;
        if (body.CollisionLayer == 1 && body.Name == "Floor")
            onGround = false;

    }

    public override void _PhysicsProcess(float delta)
    {
        pause -= delta;
        if (pause < 0 && IsOnGround())
        {
            Vector3 direction = player.GlobalTransform.origin - GlobalTransform.origin;
            direction = direction.Normalized();
            Vector3 axisRight = direction.Cross(Vector3.Up);
            direction = direction.Rotated(axisRight, Mathf.Deg2Rad(JumpAngleDeg));
            ApplyCentralImpulse(direction * JumpImpulse);
            pause = pauseReset + (GD.Randf() * 2.0f - 1.0f);
        }
    }
}

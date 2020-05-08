using Godot;
using System;

public class EnemyPencil : RigidBody
{
    public bool onGround;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Connect("body_entered", this, "BodyEnteredSelf");
        Connect("body_exited", this, "BodyExitedSelf");
    }

    private bool IsOnGround() 
    {
        return onGround;
    }

    private void BodyEnteredSelf(Node node)
    {
        
    }

    private void BodyExitedSelf(Node node)
    {

    }

    public override void _PhysicsProcess(float delta)
    {
        ApplyCentralImpulse(Vector3.Up);
        
    }
}

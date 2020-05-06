using Godot;
using Godot.Collections;
using System;

public class ZigZagEnemy : Enemy
{
    float max_velocity = 4.0f;
    float max_acceleration = 5.0f;

    Player player;
    // Should belong in Enemy, here for debug
    ImmediateGeometry geometry;
    Vector3 controlPoint;
    Vector3 velocity;
    Vector3 acceleration;
    float segmentMaxDuration = 3.0f;
    float segmentDuration = 10.0f;
    float segmentLength = 3.0f;
    float segmentAngleOffsetDeg = 60.0f;
    int segmentSide = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        player = GetTree().Root.GetNode("Level").GetNodeOrNull<Player>("Player");
        geometry = GetNode<ImmediateGeometry>("ImmediateGeometry");
        velocity = Vector3.Zero;
        acceleration = Vector3.Zero;
        controlPoint = GlobalTransform.origin;
    }

    public override void _PhysicsProcess(float delta) 
    {
        DrawControlPoint();
        if (IsAtControlPoint() || segmentDuration < 0)
            NextControlPoint();
        MoveToControlPoint(delta);
    }

    private void DrawControlPoint()
    {
        geometry.Clear();
        geometry.Begin(Godot.Mesh.PrimitiveType.Lines);
        geometry.SetColor(new Color(0, 0, 0));
        geometry.AddVertex(Vector3.Zero);
        geometry.AddVertex(controlPoint - GlobalTransform.origin);
        geometry.End();
    }

    public void NextControlPoint()
    {
        Vector3 directionToPlayer = player.GlobalTransform.origin - GlobalTransform.origin;
        float distanceToPlayer = directionToPlayer.Length();
        directionToPlayer = directionToPlayer.Normalized();
        Vector3 movementDirection = directionToPlayer.Rotated(Vector3.Up, segmentSide * Mathf.Deg2Rad(segmentAngleOffsetDeg));
        controlPoint = GlobalTransform.origin + movementDirection * Mathf.Min(segmentLength, distanceToPlayer);
        Dictionary ray = GetWorld().DirectSpaceState.IntersectRay(
            GlobalTransform.origin, controlPoint, collisionMask: 1);
        if (ray.Count > 0)
            controlPoint = (Vector3)ray["position"];
        segmentSide *= -1; // flip to other direction
        segmentDuration = segmentMaxDuration;
    }

    private bool IsAtControlPoint()
    {
        return (controlPoint - GlobalTransform.origin).Length() < 0.1;
    }

    public void MoveToControlPoint(float delta)
    {
        segmentDuration -= delta;
        //velocity *= 0.9f;
        //acceleration *= 0.1f;
        // acceleration *= 0.2f;
        acceleration = Vector3.Zero;
        Vector3 accelDirection = controlPoint - GlobalTransform.origin;
        if (accelDirection.Length() > 1.0f)
            accelDirection = accelDirection.Normalized();
        acceleration += accelDirection * max_acceleration;
        if (acceleration.Length() > max_acceleration)
            acceleration = acceleration.Normalized() * max_acceleration;
        velocity += acceleration;
        if (velocity.Length() > max_velocity)
            velocity = velocity.Normalized() * max_velocity;
        MoveAndSlide(velocity);
    }


}

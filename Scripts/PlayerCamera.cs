using Godot;
using System;

public class PlayerCamera : Camera
{
    [Export]
    private float CloseDistThreshold = 4f;
    [Export]
    private float FarDistThreshold = 6f;
    [Export]
    private float OffsetZ = 25f;
    [Export]
    private float Height = 50f;
    private Player player;
    private float TargetHeight;
    public override void _Ready()
    {
        player = GetNodeOrNull<Player>("../Player");
        TargetHeight = Height;
    }
    public override void _PhysicsProcess(float delta)
    {
        Vector3 moveDirection = Vector3.Zero;

        Vector3 distFromPlayer = GlobalTransform.origin - player.GlobalTransform.origin;
        float distX = distFromPlayer.x;
        float distZ = distFromPlayer.z - OffsetZ;

        float midDistThreshold = (FarDistThreshold + CloseDistThreshold) / 2;
        if (distX > FarDistThreshold || distX < CloseDistThreshold)
            moveDirection.x = midDistThreshold - distX;
        if (distZ > FarDistThreshold || distZ < CloseDistThreshold)
            moveDirection.z = midDistThreshold - distZ;

        if (player.IsMovementLocked)
            TargetHeight = Height / 2;
        else
            TargetHeight = Height;

        moveDirection.y = TargetHeight - distFromPlayer.y;

        Translation += moveDirection * delta;
    }
}

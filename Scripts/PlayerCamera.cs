using Godot;

public class PlayerCamera : Camera
{
    [Export]
    private readonly float PlayerDistanceThreshold = 2f;
    [Export]
    private readonly float CursorDistanceThreshold = 1f;
    [Export]
    private readonly float OffsetZ = 25f;
    [Export]
    private readonly float Height = 50f;
    [Export]
    private readonly float LookRange = 5.0f;
    [Export]
    private readonly float FollowCursorSpeedFactor = 0.3f;
    [Export]
    private readonly float FollowPlayerSpeedFactor = 0.7f;
    [Export]
    private readonly float VelocityDamping = 0.9f;

    private Player player;
    private float TargetHeight;
    private Vector3 followCursorVelocity;
    private Vector3 followPlayerVelocity;
    private Vector3 currentCursorOffset;

    public override void _Ready()
    {
        player = GetNodeOrNull<Player>("../Player");
        TargetHeight = Height;
        followCursorVelocity = new Vector3();
        followPlayerVelocity = new Vector3();
        currentCursorOffset = new Vector3();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (player.IsMovementLocked)
            TargetHeight = Height * 0.75f;
        else
            TargetHeight = Height;

        Vector3 playerTranslation = player.GlobalTransform.origin;
        Vector3 currentTranslation = GlobalTransform.origin - currentCursorOffset;
        Vector3 targetTranslation = new Vector3(playerTranslation.x, TargetHeight, playerTranslation.z + OffsetZ);
        Vector2 cursorPosition = player.GetWeightedAttackDirection();

        Vector3 targetCursorOffset = new Vector3(cursorPosition.x, 0, cursorPosition.y) * LookRange;
        Vector3 followPlayerDirection = targetTranslation - currentTranslation;
        Vector3 followCursorDirection = targetCursorOffset - currentCursorOffset;

        followPlayerDirection = Threshold(followPlayerDirection, PlayerDistanceThreshold);
        followCursorDirection = Threshold(followCursorDirection, CursorDistanceThreshold);

        followPlayerVelocity += followPlayerDirection;
        followCursorVelocity += followCursorDirection;

        followPlayerVelocity *= VelocityDamping;
        followCursorVelocity *= VelocityDamping;

        currentCursorOffset += (followCursorVelocity * FollowCursorSpeedFactor) * delta;

        Transform transform = GlobalTransform;
        transform.origin += (followPlayerVelocity * FollowPlayerSpeedFactor + followCursorVelocity * FollowCursorSpeedFactor) * delta;
        GlobalTransform = transform;
    }

    private static Vector3 Threshold(Vector3 vector, float threshold)
    {
        if (vector.Length() <= threshold)
            return Vector3.Zero;
        else
        {
            return vector.Normalized() * (vector.Length() - threshold);
        }
    }
}

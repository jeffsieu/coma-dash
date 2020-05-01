using System;
using Godot;
using Godot.Collections;

public class Player : KinematicBody
{
    [Export]
    protected float MaxSpeed = 25f;

    [Export]
    protected float TurningResistance = 0.2f;

    [Export]
    protected float MaxAccelerationFactor = 0.3f;

    [Export]
    protected float MaxDecelerationFactor = 0.1f;

    [Export]
    protected float WalkingSlowFactor = 0.7f;
    protected float RotationSpeed = 30f;

    private float WalkingSpeed
    {
        get
        {
            return WalkingSlowFactor * MaxSpeed;
        }
    }

    private float MaxAcceleration
    {
        get
        {
            return MaxAccelerationFactor * MaxSpeed;
        }
    }
    private float MaxDeceleration
    {
        get
        {
            return MaxDecelerationFactor * MaxSpeed;
        }
    }

    private bool IsSprinting
    {
        get
        {
            return Input.IsActionPressed("movement_sprint");
        }
    }

    private Vector2 mousePosition;
    private Vector3 velocity;
    private Camera camera;
    private InputMode inputMode = InputMode.Keyboard;
    private Vector3 previousFaceDirection = Vector3.Forward;
    private Weapon weapon;

    public override void _Ready()
    {
        camera = GetParent().GetNode<Camera>("Camera");
        weapon = GetNode<Weapon>("Weapon");
    }

    public override void _Input(InputEvent @event)
    {
        // Mouse in viewport coordinates
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            mousePosition = eventMouseMotion.Position;
            inputMode = InputMode.Keyboard;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 weightedDirection = GetWeightedMovementDirection();
        Move(weightedDirection, delta);
        float angle = GetFaceDirection().AngleTo(previousFaceDirection);
        angle = Math.Min(angle, RotationSpeed * delta);
        Vector3 newFaceDirection = previousFaceDirection.Rotated(GetFaceDirection().Cross(previousFaceDirection), -angle);
        LookAt(Translation + newFaceDirection, Vector3.Up);
        previousFaceDirection = newFaceDirection.Normalized();

        // So that the global rotation of the weapon will be zero
        weapon.Rotation = -Rotation;
        weapon.AttackDirection = GetAttackDirection();
    }

    public void Move(Vector2 weightedDirection, float delta)
    {
        float targetSpeed = IsSprinting ? MaxSpeed : WalkingSpeed;
        Vector3 targetVelocity = targetSpeed * new Vector3(weightedDirection.x, 0, weightedDirection.y);

        Vector3 targetAcceleration = targetVelocity - velocity;


        Vector3 actualAcceleration = targetAcceleration;

        // Player trying to accelerate in the same direction
        if (targetVelocity.Dot(targetAcceleration) > 0)
        {
            if (targetAcceleration.Length() > MaxAcceleration)
            {
                actualAcceleration = targetAcceleration.Normalized() * MaxAcceleration;
            }
        }
        // Player trying to slow down/switch direction
        else
        {
            if (targetAcceleration.Length() > MaxDeceleration)
            {
                actualAcceleration = targetAcceleration.Normalized() * MaxDeceleration;
            }
        }

        // 1.0 when player is continuing forward/trying to go backward,
        // 0.0 when player is trying to turn 90 degrees
        float directionSpeedFactor = Mathf.Abs(Mathf.Cos(targetVelocity.AngleTo(velocity)));

        // Map speed factor to [1 - resistance, 1], so player is slowed by <resistance> when he is turning 90 degrees
        directionSpeedFactor = Mathf.Lerp(1 - TurningResistance, 1.0f, directionSpeedFactor);

        // Slow player when he is turning
        actualAcceleration *= directionSpeedFactor;

        Vector3 newVelocity = velocity + actualAcceleration;
        velocity = newVelocity;
        this.MoveAndCollide(newVelocity * delta);
    }

    public Vector2 CorrectJoystick(Vector2 rawInput, float deadZone)
    {
        return rawInput.Length() > deadZone ? rawInput : default;
    }

    public Vector2 GetAttackDirection()
    {
        if (Input.GetConnectedJoypads().Count > 0)
        {
            float horizontal = Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left");
            float vertical = Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up");
            Vector2 joystick = new Vector2(horizontal, vertical).Clamped(1.0f);
            if (joystick.Length() > 0)
            {
                inputMode = InputMode.Controller;
                return joystick;
            }
        }

        if (inputMode == InputMode.Keyboard)
        {
            Vector3? cursorPosition = GetCursorPointOnPlayerPlane();
            if (cursorPosition.HasValue)
            {
                Vector3 displacement = cursorPosition.Value - Translation;
                return weapon.GetAttackJoyAxisFromMouseDisplacement(displacement);
            }
        }
        return Vector2.Zero;
    }

    private Vector2 GetWeightedMovementDirection()
    {
        float horizontal = Input.GetActionStrength("movement_right") - Input.GetActionStrength("movement_left");
        float vertical = Input.GetActionStrength("movement_down") - Input.GetActionStrength("movement_up");
        Vector2 direction = new Vector2(horizontal, vertical);

        return direction.Clamped(1.0f);
    }

    private Vector3 GetFaceDirection()
    {
        Vector3 direction = default;

        if (Input.GetConnectedJoypads().Count > 0)
        {
            float horizontal = Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left");
            float vertical = Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up");
            Vector2 joystick = new Vector2(horizontal, vertical).Clamped(1.0f);
            if (joystick.Length() > 0)
            {
                inputMode = InputMode.Controller;
            }
            direction += ((Vector3.Right * joystick.x) + (Vector3.Back * joystick.y)).Normalized();
        }

        if (inputMode == InputMode.Keyboard)
        {
            Vector3? cursorPosition = GetCursorPointOnPlayerPlane();
            if (cursorPosition.HasValue)
            {
                direction = cursorPosition.Value - Translation;
            }
        }

        return direction.Normalized();
    }

    /// <summary>
    /// Returns the point above the floor that the cursor is pointing at, which is at the same height as the origin of the player.
    /// Can be used to make the player face the cursor.
    /// </summary>
    /// <returns>The cursor point on the same plane as the player.</returns>
    private Vector3? GetCursorPointOnPlayerPlane()
    {
        Vector3 cameraRayOrigin = camera.ProjectRayOrigin(mousePosition);
        Vector3 cameraRayTarget = cameraRayOrigin + (camera.ProjectRayNormal(mousePosition) * 1000);
        Dictionary ray = GetWorld().DirectSpaceState.IntersectRay(cameraRayOrigin, cameraRayTarget, new Godot.Collections.Array { this });
        if (ray.Count > 0)
        {
            Vector3 cursorPointOnFloor = (Vector3)ray["position"];
            cursorPointOnFloor.y = GlobalTransform.origin.y;

            return cursorPointOnFloor;
        }
        return null;
    }
}

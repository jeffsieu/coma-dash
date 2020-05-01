using System;
using Godot;
using Godot.Collections;

public class Player : KinematicBody
{
    private const float Speed = 20f;
    private const float RotationSpeed = 30f;
    private Vector2 mousePosition;
    private Camera camera;
    private InputMode inputMode = InputMode.Keyboard;
    private Vector3 previousFaceDirection = Vector3.Forward;
    private Weapon weapon;

    public override void _Ready()
    {
        camera = GetParent().GetNode<Camera>("Camera");
        weapon = GetNode<Weapon>("Weapon");
    }

    public void Move(Vector3 direction, float delta)
    {
        this.MoveAndCollide(direction.Normalized() * Speed * delta);
    }

    public Vector2 CorrectJoystick(Vector2 rawInput, float deadZone)
    {
        return rawInput.Length() > deadZone ? rawInput : default;
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
        Vector3 movementDirection = GetMovementDirection();
        Move(movementDirection, delta);
        float angle = GetFaceDirection().AngleTo(previousFaceDirection);
        angle = Math.Min(angle, RotationSpeed * delta);
        Vector3 newFaceDirection = previousFaceDirection.Rotated(GetFaceDirection().Cross(previousFaceDirection), -angle);
        LookAt(Translation + newFaceDirection, Vector3.Up);
        previousFaceDirection = newFaceDirection.Normalized();

        // So that the global rotation of the weapon will be zero
        weapon.Rotation = -Rotation;
        weapon.AttackDirection = GetAttackDirection();
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 direction = default;
        direction += (Vector3.Forward * Input.GetActionStrength("movement_up")).Normalized();
        direction += (Vector3.Back * Input.GetActionStrength("movement_down")).Normalized();
        direction += (Vector3.Left * Input.GetActionStrength("movement_left")).Normalized();
        direction += (Vector3.Right * Input.GetActionStrength("movement_right")).Normalized();

        return direction;
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

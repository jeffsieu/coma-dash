using System;
using Godot;
using Godot.Collections;

public class Player : KinematicBody
{
    private const float Speed = 2000f;
    private const float DeadZone = 0.5f;
    private const float RotationSpeed = 30f;

    private Vector2 mousePosition;
    private Camera camera;
    private InputMode inputMode = InputMode.Keyboard;
    private Vector3 previousFaceDirection = Vector3.Forward;

    public override void _Ready()
    {
        camera = GetParent().GetNode<Camera>("Camera");
    }

    public void Move(Vector3 direction, float delta)
    {
        this.MoveAndSlide(direction.Normalized() * Speed * delta);
    }

    public Vector2 CorrectJoystick(Vector2 rawInput, float deadZone)
    {
        return
        rawInput.Length() > deadZone ? rawInput : default;
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
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 direction = default;
        if (Input.IsActionPressed("movement_up"))
        {
            direction += Vector3.Forward;
        }

        if (Input.IsActionPressed("movement_down"))
        {
            direction += Vector3.Back;
        }

        if (Input.IsActionPressed("movement_left"))
        {
            direction += Vector3.Left;
        }

        if (Input.IsActionPressed("movement_right"))
        {
            direction += Vector3.Right;
        }

        if (Input.GetConnectedJoypads().Count > 0)
        {
            Vector2 rawJoystickInput = new Vector2(Input.GetJoyAxis(0, 0), Input.GetJoyAxis(0, 1));
            Vector2 joystick = CorrectJoystick(rawJoystickInput, DeadZone);
            if (joystick.Length() > 0)
            {
                inputMode = InputMode.Controller;
            }

            direction += ((Vector3.Right * joystick.x) + (Vector3.Back * joystick.y)).Normalized();
        }

        return direction;
    }

    private Vector3 GetFaceDirection()
    {
        Vector3 direction = default;

        if (Input.GetConnectedJoypads().Count > 0)
        {
            Vector2 rawJoystickInput = new Vector2(Input.GetJoyAxis(0, 2), Input.GetJoyAxis(0, 3));
            Vector2 joystick = CorrectJoystick(rawJoystickInput, DeadZone);
            if (joystick.Length() > 0)
            {
                inputMode = InputMode.Controller;
            }

            direction += ((Vector3.Right * joystick.x) + (Vector3.Back * joystick.y)).Normalized();
        }

        if (inputMode == InputMode.Keyboard)
        {
            Vector3 cameraRayOrigin = camera.ProjectRayOrigin(mousePosition);
            Vector3 cameraRayTarget = cameraRayOrigin + (camera.ProjectRayNormal(mousePosition) * 1000);
            Dictionary ray = GetWorld().DirectSpaceState.IntersectRay(cameraRayOrigin, cameraRayTarget, null);
            if (ray.Count > 0)
            {
                Vector3 cursorPointOnFloor = (Vector3)ray["position"];
                cursorPointOnFloor.y = GlobalTransform.origin.y;

                direction = cursorPointOnFloor - Translation;
            }
        }

        return direction.Normalized();
    }
}

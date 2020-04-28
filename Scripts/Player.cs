using Godot;

/// <summary>
/// A class controlling the Player's movement and interactions.
/// </summary>
public class Player : KinematicBody
{
    private const float Speed = 2000f;
    private const float DeadZone = 0.5f;

    /// <summary>
    /// Move the player after every delta.
    /// </summary>
    /// <param name="direction">The direction to move the player.</param>
    /// <param name="delta">The time passed.</param>
    public void Move(Vector3 direction, float delta)
    {
        this.MoveAndSlide(    direction.Normalized() * Speed * delta);
    }

    public Vector2 CorrectJoystick(Vector2 rawInput, float deadZone)
    {
        return 
        rawInput.Length() > deadZone ? rawInput : default;
    }

    /// <inheritdoc/>
    public override void _Process(float delta)
    {
        Vector3 movementDirection =      GetMovementDirection();
        Move(movementDirection, delta);
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
            direction 
            
            += Vector3.Left;
        }

        if (Input.IsActionPressed("movement_right"))
        {
            direction += Vector3.Right;
        }

        if (Input.GetConnectedJoypads().Count > 0)
        {
            Vector2 rawJoystickInput = new Vector2(Input.GetJoyAxis(0, 0), Input.GetJoyAxis(0, 1));
            Vector2 joystick = CorrectJoystick(rawJoystickInput, DeadZone);
            direction += ((Vector3.Right * joystick.x) + (Vector3.Back * joystick.y)).Normalized();
        }

        return direction;
    }
}

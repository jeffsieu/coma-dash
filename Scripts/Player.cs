using System;
using Godot;
using Godot.Collections;

public class Player : HealthEntity
{
    [Export]
    protected float SprintAccelerationFactor = 200f;
    [Export]
    protected float WalkAccelerationFactor = 100f;
    [Export]
    protected float DragFactor = 2f;
    [Export]
    protected float FrictionFactor = 1.6f;
    [Export]
    protected float TorqueFactor = 400f;
    public bool IsMovementLocked = false;
    public bool disableFriction = false;

    public float gravity;

    private bool IsSprinting
    {
        get
        {
            return Input.IsActionPressed("movement_sprint");
        }
    }

    private bool IsPrimaryAttackPressed
    {
        get
        {
            return Input.IsActionPressed("attack_primary");
        }
    }

    private bool IsSecondaryAttackPressed
    {
        get
        {
            return Input.IsActionPressed("attack_secondary");
        }
    }

    private Vector2 mousePosition;
    private Vector3 velocity;
    private Camera camera;
    private InputMode inputMode = InputMode.Keyboard;
    private Vector3 previousFaceDirection = Vector3.Forward;
    private AimableAttack weapon;
    private AimableAttack skill;
    private RunningCharacter character;

    public Player()
    {
        MaxHealth = 100;
    }


    public override void _Ready()
    {
        base._Ready();
        camera = GetParent().GetNode<Camera>("Camera");
        weapon = GetNode<AimableAttack>("Weapon");
        skill = GetNode<AimableAttack>("Skill");
        character = GetNode<RunningCharacter>("RunningChar");
        gravity = (float)PhysicsServer.AreaGetParam(GetWorld().Space, PhysicsServer.AreaParameter.Gravity);
        // Move weapon to the front of the player
        weapon.Translation = Vector3.Forward * Scale.z + Vector3.Up * Scale.y / 2;
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
        base._PhysicsProcess(delta);

        if (!IsMovementLocked)
        {
            Vector2 weightedDirection = GetWeightedMovementDirection();
            Move(weightedDirection, delta);
        }

        Vector3 faceDirection = GetFaceDirection();
        Vector3 vel = LinearVelocity;
        float velMag = LinearVelocity.Length();

        float animationTimeFactor = 0.15f;


        if (velMag > 1)
        {
            Face(vel.Normalized(), delta);
            character.RunAnimation();
            character.SetSpeed(velMag * animationTimeFactor);
        }
        else
        {
            Face(faceDirection, delta);
            character.PlayIdle();
            character.SetSpeed(1);
        }

        // So that the global rotation of the weapon will be zero
        weapon.WeightedAttackDirection = GetWeightedAttackDirection();
        weapon.IsAttackButtonPressed = IsPrimaryAttackPressed;

        skill.WeightedAttackDirection = GetWeightedAttackDirection();
        skill.IsAttackButtonPressed = IsSecondaryAttackPressed;

        bool showSkillAimIndicator = IsSecondaryAttackPressed;
        weapon.AimIndicator.Visible = !showSkillAimIndicator;
        skill.AimIndicator.Visible = showSkillAimIndicator;

        weapon.AimIndicator.Translation = Vector3.Down;
        skill.AimIndicator.Translation = Vector3.Down;
    }

    public void Move(Vector2 weightedDirection, float delta)
    {
        // Add acceleration based on user input
        float accFactor = IsSprinting ? SprintAccelerationFactor : WalkAccelerationFactor;
        Vector3 direction = new Vector3(weightedDirection.x, 0, weightedDirection.y);
        AddCentralForce(accFactor * Mass * direction);

        // Add drag to cap the maximum speed, proportional to square of speed
        Vector3 velocity = LinearVelocity * new Vector3(1, 0, 1);
        Vector3 velDirection = velocity.Length() > 1 ? velocity.Normalized() : velocity;

        if (!disableFriction)
        {
            float speedSquared = velocity.LengthSquared();
            AddCentralForce(-DragFactor * speedSquared * velDirection);
            // Add kinetic friction
            AddCentralForce(-FrictionFactor * Mass * GravityScale * gravity * velDirection);
        }

    }

    public void Face(Vector3 faceDirection, float delta)
    {
        float currAngularVelocity = AngularVelocity.y;

        // Calculate angle from current orientation
        float angle = faceDirection.AngleTo(-GlobalTransform.basis.z);
        // Check if clockwise or counter-clockwise
        if (faceDirection.Dot(GlobalTransform.basis.x) > 0)
            angle = -angle;

        // Approximately reached target
        if (Mathf.Abs(angle) < Mathf.Deg2Rad(0.5f))
        {
            // Kill current angular velocity whatever it is
            ApplyTorqueImpulse(Mass * -AngularVelocity);
        }
        else
        //else if (Mathf.Abs(angle) < Mathf.Deg2Rad(5))
        {
            // Kill current angular velocity whatever it is
            ApplyTorqueImpulse(Mass * -AngularVelocity);
            // Rotate towards correct direction
            AddTorque(Mass * TorqueFactor * angle * Vector3.Up);
        }
    }

    public Vector2 GetWeightedAttackDirection()
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
                return weapon.GetWeightedAttackDirectionFromMouseDisplacement(displacement);
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
        Vector3 rayDirection = camera.ProjectRayNormal(mousePosition);
        float factor = (GlobalTransform.origin.y - cameraRayOrigin.y) / rayDirection.y;
        return cameraRayOrigin + rayDirection * factor;
    }

    protected override void Die()
    {
        base.Die();
        QueueFree();
    }
}

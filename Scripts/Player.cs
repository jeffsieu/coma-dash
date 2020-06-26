using Godot;

public class Player : HealthEntity
{
    [Export]
    protected float SprintAccelerationFactor = 300f;
    [Export]
    protected float WalkAccelerationFactor = 200f;
    [Export]
    protected float DragFactor = 10f;
    [Export]
    protected float FrictionFactor = 1.6f;
    [Export]
    protected float TorqueFactor = 400f;
    [Export]
    protected float RecoilImpulseMultiplier = 10f;

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

    public OverheatingGun Weapon { get; private set; }

    private Vector2 mousePosition;
    private Camera camera;
    private InputMode inputMode = InputMode.Keyboard;
    private AimableAttack skill;
    private RunningCharacter character;
    private Level level;

    public Player()
    {
        MaxHealth = 100;
        RegenerationDelay = 1.0f;
        RegenerationRate = 10.0f;
    }

    public override void _Ready()
    {
        base._Ready();
        healthBarPositionOffset = Vector3.Up * Scale.z * 3.5f;
        level = GetTree().Root.GetNode<Level>("Level");
        camera = GetParent().GetNode<Camera>("Camera");
        character = GetNode<RunningCharacter>("RunningChar");
        skill = character.GetNode<AimableAttack>("Skill");
        Weapon = character.GetNode<OverheatingGun>("Weapon");
        gravity = (float)PhysicsServer.AreaGetParam(GetWorld().Space, PhysicsServer.AreaParameter.Gravity);
        // Move weapon to the front of the player
        Weapon.Translation = Vector3.Forward * Weapon.Scale.z + Vector3.Up * Weapon.Scale.y;
        Weapon.Connect("Fired", this, "WeaponFired");
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
        Vector3 velDirection = vel.Normalized();

        float animationTimeFactor = 0.15f;

        Face(faceDirection, delta);
        if (velMag > 1)
        {
            if (velDirection.Dot(faceDirection) < 0)
            {
                character.BackRunAnimation();
                character.SetSpeed(velMag * animationTimeFactor);
            }
            else
            {
                character.RunAnimation();
                character.SetSpeed(velMag * animationTimeFactor);
            }

        }
        else
        {
            character.PlayIdle();
            character.SetSpeed(1);
        }

        // So that the global rotation of the weapon will be zero
        Weapon.WeightedAttackDirection = GetWeightedAttackDirection();
        Weapon.IsAttackButtonPressed = IsPrimaryAttackPressed;

        skill.WeightedAttackDirection = GetWeightedAttackDirection();
        skill.IsAttackButtonPressed = IsSecondaryAttackPressed;

        bool showSkillAimIndicator = IsSecondaryAttackPressed;
        Weapon.AimIndicator.Visible = !showSkillAimIndicator;
        skill.AimIndicator.Visible = showSkillAimIndicator;

        Weapon.AimIndicator.Translation = Vector3.Down;
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

    public void WeaponFired(Projectile projectile)
    {
        ApplyCentralImpulse(-1 * RecoilImpulseMultiplier * projectile.Mass * projectile.Velocity);
    }

    public void Face(Vector3 faceDirection, float delta)
    {
        character.LookAt(character.GlobalTransform.origin + faceDirection, Vector3.Up);
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
                return Weapon.GetWeightedAttackDirectionFromMouseDisplacement(displacement);
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
        level.GameOver();
    }
}

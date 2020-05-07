using Godot;

public class NoobEnemy : Enemy
{
    [Export]
    public float AttackCooldown = 3f;
    [Export]
    public float AttackDuration = 0;
    [Export]
    public float ChargeSpeed = 60f;
    [Export]
    public float ChargeDistance = 10f;
    [Export]
    public float DetectDistance = 8f;
    [Export]
    public float AttackDamage = 2f;
    [Export]
    public float KnockbackFactor = 0.4f;

    private SpatialMaterial material;
    private float attackCooldownLeft = 3f;
    private float attackDurationLeft = 0f;

    public NoobEnemy()
    {
        MaxHealth = 100;
        acceleration = 20f;
    }

    public override void _Ready()
    {
        base._Ready();
        attackCooldownLeft = AttackCooldown;
        material = new SpatialMaterial()
        {
            AlbedoColor = Colors.White,
            FlagsTransparent = true
        };

        MeshInstance mesh = GetNode<MeshInstance>("MeshInstance");
        mesh.SetSurfaceMaterial(0, material);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        // stop charging if attack duration turns 0 in this frame
        if (attackDurationLeft > 0 && attackDurationLeft - delta <= 0)
            Velocity = Vector3.Zero;

        attackDurationLeft = Mathf.Max(attackDurationLeft - delta, 0);
        attackCooldownLeft = Mathf.Max(attackCooldownLeft - delta, 0);

        TryAttackPlayer();
        Move(delta);

        Color baseColor = Colors.Red.LinearInterpolate(Colors.Blue, GetPercentLeft<MarkStatus>());
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - Health / 100);
        material.AlbedoColor = healthColor;
    }

    private void Move(float delta)
    {
        Velocity.y -= gravity;
        if (attackDurationLeft == 0)
        {
            Velocity.x *= 0.9f;
            Velocity.z *= 0.9f;
            MoveToPlayer(delta);
            Velocity = MoveAndSlide(Velocity);
            RotateToPlayer(delta);
        }
        else
        {
            MoveAndSlide(Velocity);
            CheckCollision();
        }
    }

    private void TryAttackPlayer()
    {
        Vector3 directionToPlayer = player.GlobalTransform.origin - GlobalTransform.origin;
        float angleToPlayer = Mathf.Pi - GlobalTransform.basis.z.AngleTo(directionToPlayer);
        if (GlobalTransform.origin.DistanceTo(player.GlobalTransform.origin) < DetectDistance && angleToPlayer < Mathf.Pi / 2
            && attackCooldownLeft == 0)
        {
            LookAt(player.GlobalTransform.origin, Vector3.Up);
            Velocity = directionToPlayer.Normalized() * ChargeSpeed;
            attackCooldownLeft = AttackCooldown;
            attackDurationLeft = ChargeDistance / ChargeSpeed;
        }
    }

    private void CheckCollision()
    {
        if (GetSlideCount() > 0)
        {
            for (int i = 0; i < GetSlideCount(); ++i)
            {
                Object collider = GetSlideCollision(i).Collider;
                if (collider is Player)
                {
                    player.Velocity += Velocity * KnockbackFactor;
                    player.Damage(AttackDamage);
                }
                if (!(collider is Floor))
                {
                    Velocity = Vector3.Zero;
                    attackDurationLeft = 0;
                }
            }
        }
    }

    protected override void Die()
    {
        Tween tween = new Tween();
        AddChild(tween);
        tween.InterpolateProperty(material, "albedo_color:a", 0.5f, 0, 1.0f);
        tween.InterpolateCallback(this, 1.0f, "queue_free");
        tween.Start();

        base.Die();
    }
}

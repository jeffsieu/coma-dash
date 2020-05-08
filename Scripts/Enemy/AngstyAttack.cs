using Godot;
using System.Runtime;

public class AngstyAttack : Spatial
{
    [Export]
    public float AttackCooldown = 2f;
    [Export]
    public float MovebackDuration = 0.2f;
    [Export]
    public float MovebackSpeed = 8f;
    [Export]
    public float ChargeSpeed = 80f;
    [Export]
    public float ChargeDistance = 15f;
    [Export]
    public float DetectDistance = 10f;
    [Export]
    public float AttackDamage = 2f;
    [Export]
    public float KnockbackFactor = 0.4f;

    private float attackCooldownLeft = 3f;
    private float attackDurationLeft = 0f;
    private float chargeDuration;

    private Player player;
    private Enemy parent;

    public override void _Ready()
    {
        attackCooldownLeft = AttackCooldown;
        chargeDuration = ChargeDistance / ChargeSpeed;
        player = GetTree().Root.GetNode("Level").GetNode<Player>("Player");
        parent = GetParent<Enemy>();
    }

    public override void _PhysicsProcess(float delta)
    {
        // stop charging if attack duration turns 0 in this frame
        if (attackDurationLeft > 0 && attackDurationLeft - delta <= 0)
            parent.Velocity = Vector3.Zero;

        attackDurationLeft = Mathf.Max(attackDurationLeft - delta, 0);
        attackCooldownLeft = Mathf.Max(attackCooldownLeft - delta, 0);
    }

    public void TryStartAttackSequence()
    {
        if (GlobalTransform.origin.DistanceTo(player.GlobalTransform.origin) < DetectDistance && attackCooldownLeft == 0)
        {
            LookAt(player.GlobalTransform.origin, Vector3.Up);
            attackDurationLeft = chargeDuration + MovebackDuration;
            attackCooldownLeft = AttackCooldown + attackDurationLeft;
        }
    }

    public void AttackPlayer(float delta)
    {
        Vector3 directionToPlayer = GlobalTransform.origin.DirectionTo(player.GlobalTransform.origin);
        if (IsMovingBack())
        {
            parent.Velocity = -directionToPlayer * MovebackSpeed;
        }
        else if (attackDurationLeft <= chargeDuration && attackDurationLeft + delta > chargeDuration)
        {
            parent.Velocity = directionToPlayer * ChargeSpeed;
        }
        parent.MoveAndSlide(parent.Velocity);
        CheckCollision();
    }

    private void CheckCollision()
    {
        if (parent.GetSlideCount() > 0)
        {
            for (int i = 0; i < parent.GetSlideCount(); ++i)
            {
                Object collider = parent.GetSlideCollision(i).Collider;
                if (collider is Player)
                {
                    player.Velocity += parent.Velocity * KnockbackFactor;
                    player.Damage(AttackDamage);
                    parent.Velocity = Vector3.Zero;
                    attackDurationLeft = 0;
                }
            }
        }
    }

    private bool IsMovingBack()
    {
        return attackDurationLeft > chargeDuration;
    }

    public bool IsInAttackSequence()
    {
        return attackDurationLeft > 0;
    }
}
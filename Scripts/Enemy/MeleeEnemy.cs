using Godot;

public class MeleeEnemy : Enemy
{
    protected bool isAttacking = false;
    protected float range = 5.0f;
    protected float preAttackPauseDuration = 0.5f;
    protected float postAttackPauseDuration = 0.5f;

    protected float attackDelta = 0;
    protected bool hasAttacked = false;

    protected float damage = 10.0f;

    public override void _Process(float delta)
    {
        Color baseColor = Colors.Red;
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - Health / 100);
        material.AlbedoColor = healthColor;

        if (isAttacking && attackDelta < preAttackPauseDuration)
            material.AlbedoColor = Colors.White;

        UpdateStatusBars(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (isAttacking)
        {
            attackDelta += delta;
            if (attackDelta < preAttackPauseDuration)
            {
                // Become white
                LinearVelocity = Vector3.Zero;
            }
            else if (attackDelta < preAttackPauseDuration + postAttackPauseDuration)
            {
                // Attack, then pause
                if (!hasAttacked)
                {
                    // Try to attack target
                    if (GlobalTransform.origin.DistanceTo(player.GlobalTransform.origin) <= range)
                        player.Damage(damage);
                    hasAttacked = true;
                }
                else
                {
                    // Remain on the spot until "stun" duration is over


                }
            }
            else
            {
                isAttacking = false;
                hasAttacked = false;
                attackDelta = 0;
            }
            return;
        }


        if (GlobalTransform.origin.DistanceTo(player.GlobalTransform.origin) < range)
        {
            AttackTarget();
        }
        else
        {
            Vector3 velocity = 5.0f * GlobalTransform.origin.DirectionTo(GetNextPointToTarget());
            LinearVelocity = velocity;
        }
    }

    protected void AttackTarget()
    {
        isAttacking = true;
    }
}

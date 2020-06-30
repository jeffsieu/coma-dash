using Godot;

public class MeleeEnemy : Enemy
{
    protected static float range = 5.0f;
    protected static float preAttackPauseDuration = 0.5f;
    protected static float postAttackPauseDuration = 0.5f;
    protected static float damage = 10.0f;

    protected StateManager stateManager;
    protected bool inPreAttack = false;

    public override void _Ready()
    {
        base._Ready();
        stateManager = new StateManager();
        AddChild(stateManager);

        stateManager.GoTo(Idle);
    }

    public override void _Process(float delta)
    {
        Color baseColor = Colors.Red;
        material.AlbedoColor = inPreAttack ? Colors.White : baseColor;
        UpdateStatusBars(delta);
    }

    protected void Idle(float delta, float elapsedDelta)
    {
        if (GlobalTransform.origin.DistanceTo(player.GlobalTransform.origin) < range)
        {
            AttackTarget();
        }
        else
        {
            Vector3 velocity = 500f * delta * GlobalTransform.origin.DirectionTo(GetNextPointToTarget());
            LinearVelocity = velocity;
        }
    }

    protected void PreAttack(float delta, float elapsedDelta)
    {
        // Become white
        LinearVelocity = Vector3.Zero;
        inPreAttack = true;

        if (elapsedDelta >= preAttackPauseDuration)
        {
            stateManager.GoTo(Attack);
        }
    }

    protected void Attack(float delta, float elapsedDelta)
    {
        // Try to attack target
        if (GlobalTransform.origin.DistanceTo(player.GlobalTransform.origin) <= range)
            player.Damage(damage);
        inPreAttack = false;
        stateManager.GoTo(PostAttack);
    }

    protected void PostAttack(float delta, float elapsedDelta)
    {
        // Remain on the spot until "stun" duration is over
        if (elapsedDelta >= postAttackPauseDuration)
        {
            stateManager.GoTo(Idle);
        }
    }

    protected void AttackTarget()
    {
        stateManager.GoTo(PreAttack);
    }
}

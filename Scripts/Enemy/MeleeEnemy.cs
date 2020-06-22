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
    }

    public override void _Process(float delta)
    {
        Color baseColor = Colors.Red;
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - Health / 100);
        material.AlbedoColor = inPreAttack ? Colors.White : healthColor;
        UpdateStatusBars(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        // Let StateManager take over
        if (stateManager.IsRunning)
            return;

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

    protected void PreAttack(float elapsedDelta)
    {
        // Become white
        LinearVelocity = Vector3.Zero;
        inPreAttack = true;

        if (elapsedDelta >= preAttackPauseDuration)
        {
            stateManager.GoTo(Attack);
        }
    }

    protected void Attack(float elapsedDelta)
    {
        // Try to attack target
        if (GlobalTransform.origin.DistanceTo(player.GlobalTransform.origin) <= range)
            player.Damage(damage);
        inPreAttack = false;
        stateManager.GoTo(PostAttack);
    }

    protected void PostAttack(float elapsedDelta)
    {
        // Remain on the spot until "stun" duration is over
        if (elapsedDelta >= postAttackPauseDuration)
        {
            stateManager.Stop();
        }
    }

    protected void AttackTarget()
    {
        stateManager.GoTo(PreAttack);
    }
}

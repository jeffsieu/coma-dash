using Godot;

public class BigMeleeEnemy : MeleeEnemy
{
    public BigMeleeEnemy()
    {
        range = 5.0f;
        preAttackPauseDuration = 0.25f;
        postAttackPauseDuration = 0.25f;
        damage = 30.0f;
        MaxHealth = 300;
    }
    public override void _Ready()
    {
        base._Ready();
        healthBarPositionOffset = Vector3.Up * Scale.z * 6.0f;
    }
}

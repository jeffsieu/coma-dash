using Godot;

public class AngstyEnemy : Enemy
{

    private SpatialMaterial material;
    private AngstyAttack attack;

    public AngstyEnemy()
    {
        MaxHealth = 100;
        acceleration = 20f;
    }

    public override void _Ready()
    {
        base._Ready();
        material = new SpatialMaterial()
        {
            AlbedoColor = Colors.White,
            FlagsTransparent = true
        };

        MeshInstance mesh = GetNode<MeshInstance>("MeshInstance");
        mesh.SetSurfaceMaterial(0, material);

        attack = GetNode<AngstyAttack>("Attack");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        attack.TryStartAttackSequence();
        if (attack.IsInAttackSequence())
        {
            attack.AttackPlayer(delta);
        }
        else
        {
            Move(delta);
        }

        Color baseColor = Colors.Red.LinearInterpolate(Colors.Blue, GetPercentLeft<MarkStatus>());
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - Health / 100);
        material.AlbedoColor = healthColor;
    }

    private void Move(float delta)
    {
        Velocity.y -= gravity;
        Velocity.x *= 0.9f;
        Velocity.z *= 0.9f;

        Velocity += acceleration * delta * GetDirectionToPlayer();

        Velocity = MoveAndSlide(Velocity);
        LookAtPlayerDirection(delta);
    }

    protected override void Die()
    {
        Tween tween = new Tween();
        AddChild(tween);
        tween.InterpolateProperty(material, "albedo_color:a", 0.5f, 0, 1.0f);
        tween.InterpolateCallback(this, 1.0f, "queue_free");
        tween.Start();
    }
}

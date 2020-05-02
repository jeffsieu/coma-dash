using Godot;

public class Enemy : KinematicBody
{
    private readonly float gravity = 4.85f;
    public Vector3 Velocity;
    private float health = 100f;
    private SpatialMaterial material;
    public bool IsMarked = false;

    public override void _Ready()
    {
        material = new SpatialMaterial()
        {
            AlbedoColor = Colors.White
        };

        CSGCylinder cylinder = GetNode<CSGCylinder>("CSGCylinder");
        cylinder.Material = material;
    }

    public override void _PhysicsProcess(float delta)
    {
        Velocity.y -= gravity;
        Velocity.x = 0;
        Velocity.z = 0;
        Velocity = MoveAndSlide(Velocity);
    }

    public void Damage(float damage)
    {
        health -= damage;
        material.AlbedoColor = Colors.White.LinearInterpolate(Colors.Red, 1 - health / 100);

        if (health <= 0)
            QueueFree();
    }
}

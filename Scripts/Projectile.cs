using Godot;

/// <summary>
/// <para>
/// Represents a projectile shot in a given direction, with a given speed and range.
/// </para>
/// <para>
/// Instantiates the projectile from a given <see cref="PackedScene"/>.
/// Projectile scenes are to have their root node as an <see cref="Area"/>.
/// The collision boxes is to be scaled such that its bounding box is a cube with side length 1.
/// The actual scale of the projectile should be set by scaling the root <see cref="Area"/> node.
/// </para>
/// </summary>
public class Projectile : Spatial
{
    private float distanceTravelled;
    private readonly float range;
    private readonly float speed;
    private readonly Vector3 direction;
    private readonly PackedScene projectileScene;
    private readonly PackedScene explosion = ResourceLoader.Load<PackedScene>("res://Scenes/Assets/Explosion.tscn");
    private float projectileLength;
    private bool hit;

    public Projectile(float range, float speed, Vector3 direction, PackedScene projectileScene)
    {
        this.range = range;
        this.speed = speed;
        this.direction = direction;
        this.projectileScene = projectileScene;
        AddUserSignal("hit_enemy");
    }

    public override void _Ready()
    {
        RigidBody projectileInstance = projectileScene.Instance() as RigidBody;
        projectileLength = 3; // TODO no hardcode
        projectileInstance.Connect("body_entered", this, "OnBodyEnteredProjectile");
        // TODO change to lifetime
        projectileInstance.GravityScale = 2;
        // To make the bullet's origin at the edge of the projectile weapon
        Vector3 weaponFrontDirection = -GlobalTransform.basis.z;
        Translation += projectileLength / 2 * weaponFrontDirection;
        hit = false;
        AddChild(projectileInstance);
        // TODO fix ImpulseFactor
        projectileInstance.ApplyCentralImpulse(
            1.5f * projectileInstance.Mass * speed * weaponFrontDirection);
    }

    public override void _PhysicsProcess(float delta)
    {
        RigidBody instance = GetNode<RigidBody>("Bullet");
        Vector3 velocity = instance.LinearVelocity * new Vector3(1, 0, 1);
        // TODO add DragFactor
        instance.AddCentralForce(0.003f * velocity.Length() * -velocity);
    }

    public void OnBodyEnteredProjectile(Node body)
    {
        if (body is Player)
            return;
        if (!hit && body is Enemy)
        {
            Enemy enemy = body as Enemy;
            EmitSignal("hit_enemy", enemy);
        }
        hit = true;
        CPUParticles particles = explosion.Instance() as CPUParticles;
        Camera camera = GetTree().Root.GetCamera();
        GetTree().Root.AddChild(particles);
        RigidBody bullet = GetNode<RigidBody>("Bullet");
        CollisionShape collisionShape = bullet.GetNode<CollisionShape>("CollisionShape");

        Vector3 collisionPosition = bullet.GlobalTransform.origin + -bullet.GlobalTransform.basis.z * (collisionShape.Shape as BoxShape).Extents.z;

        particles.LookAtFromPosition(
            collisionPosition,
            camera.GlobalTransform.origin,
            camera.GlobalTransform.basis.y
        );
        particles.Emitting = true;
        Timer timer = new Timer();
        particles.AddChild(timer);
        timer.WaitTime = 5;
        timer.Connect("timeout", particles, "queue_free");
        timer.Start();
        QueueFree();
    }
}

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
    private Vector3 origin;
    private RigidBody projectileInstance;

    public float Mass
    {
        get
        {
            return projectileInstance.Mass;
        }
    }

    public Vector3 Velocity
    {
        get 
        {
            return speed * direction;
        }
    }

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
        projectileInstance = projectileScene.Instance() as RigidBody;
        projectileLength = projectileInstance.GetNode<CSGBox>("CSGBox").Depth;
        projectileInstance.Connect("body_entered", this, "OnBodyEnteredProjectile");
        projectileInstance.GravityScale = 0;
        // To make the bullet's origin at the edge of the projectile weapon
        Vector3 weaponFrontDirection = -GlobalTransform.basis.z;
        projectileInstance.ApplyCentralImpulse(projectileInstance.Mass * speed * weaponFrontDirection);
        AddChild(projectileInstance);
        origin = projectileInstance.GlobalTransform.origin;
    }

    public override void _PhysicsProcess(float delta)
    {
        if ((projectileInstance.GlobalTransform.origin - origin).Length() > range)
            QueueFree();
    }

    public void OnBodyEnteredProjectile(Node body)
    {
        if (body is Player)
            return;
        if (body is Enemy)
        {
            Enemy enemy = body as Enemy;
            EmitSignal("hit_enemy", enemy);
        }
        CPUParticles particles = explosion.Instance() as CPUParticles;
        Camera camera = GetTree().Root.GetCamera();
        GetTree().Root.AddChild(particles);
        particles.LookAtFromPosition(
            projectileInstance.GlobalTransform.origin,
            camera.GlobalTransform.origin,
            camera.GlobalTransform.basis.y
        );
        particles.Emitting = true;
        Timer timer = new Timer();
        particles.AddChild(timer);
        timer.WaitTime = 1;
        timer.Connect("timeout", particles, "queue_free");
        timer.Start();
        QueueFree();
    }
}

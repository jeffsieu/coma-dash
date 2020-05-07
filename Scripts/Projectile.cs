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
    private float projectileLength;

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
        Area projectileInstance = projectileScene.Instance() as Area;
        projectileLength = projectileInstance.Scale.z;

        projectileInstance.Connect("body_entered", this, "OnBodyEnteredProjectile");

        // To make the bullet's origin at the edge of the projectile weapon
        Vector3 weaponFrontDirection = -GlobalTransform.basis.z;
        Translation += projectileLength * weaponFrontDirection;
        AddChild(projectileInstance);
    }

    public override void _PhysicsProcess(float delta)
    {
        float distanceToTravel = speed * delta;
        Vector3 displacementToTravel = distanceToTravel * direction;
        distanceTravelled += distanceToTravel;

        // Destroy when the front edge reaches the end point
        if (distanceTravelled > range - projectileLength)
            QueueFree();

        Translation += displacementToTravel;
        GD.Print(Translation);
    }

    public void OnBodyEnteredProjectile(Node body)
    {
        if (body is Enemy)
        {
            Enemy enemy = body as Enemy;
            EmitSignal("hit_enemy", enemy);
        }
        QueueFree();
    }
}

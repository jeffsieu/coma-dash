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
    private readonly Vector3 direction;
    private readonly float speed;
    private readonly PackedScene projectileScene;
    private float projectileLength;

    public Projectile(float range, float speed, Vector3 direction, PackedScene projectileScene)
    {
        this.range = range;
        this.speed = speed;
        this.direction = direction;
        this.projectileScene = projectileScene;
    }

    public override void _Ready()
    {
        CollisionObject projectileInstance = projectileScene.Instance() as CollisionObject;
        projectileLength = projectileInstance.Scale.z;

        // To make the bullet's origin at the edge of the projectile weapon
        Vector3 weaponFrontDirection = -GlobalTransform.basis.z;
        Translation += projectileLength / 2 * weaponFrontDirection;
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
    }
}

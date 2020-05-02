using Godot;
using Godot.Collections;

/// <summary>
/// A skill that allows the player to dash forwards.
/// Dashing to an enemy marked (see <see cref="MarkStatus"/>) by the <see cref="OverheatingGun"/>
/// will deal damage to it upon impact, as well as damaging other enemies within the area of impact.
/// The damaged enemies are also knocked back.
/// </summary>
public class DashSkill : AimableAttack
{
    private readonly float spreadDegrees;
    private readonly float dashSpeed;
    private readonly float damage;
    private readonly float impactRange;
    private readonly float coolAmount;

    private Area targetArea;
    private Area coneSides;
    private Area impactArea;
    private Spatial hintReticle;
    private Spatial impactAimIndicator;
    private bool showTargetHint = false;
    private Player player;
    private Enemy targetEnemy;
    private bool isRunning = false;

    private OverheatingGun overheatingGun;

    public DashSkill() : base(10.0f)
    {
        dashSpeed = 50.0f;
        spreadDegrees = 25.0f;
        damage = 30.0f;
        impactRange = 5.0f;
        coolAmount = 0.4f;
    }

    public override void _Ready()
    {
        base._Ready();
        player = GetParent<Player>();
        overheatingGun = player.GetNode<OverheatingGun>("Weapon");

        (AimIndicator as GeneralAimIndicator).IndicatorType = GeneralAimIndicator.AimIndicatorType.STRAIGHT;
        (AimIndicator as GeneralAimIndicator).SpreadDegrees = spreadDegrees;
        targetArea = new Area();
        targetArea.AddChild(new CollisionShape() { Shape = new CylinderShape { Radius = range } });

        coneSides = new Area();
        coneSides.AddChild(new CollisionShape
        {
            Shape = new BoxShape { Extents = new Vector3(0.01f, 1, range / 2) },
            RotationDegrees = Vector3.Up * spreadDegrees / 2,
            Translation = range / 2 * Vector3.Forward.Rotated(Vector3.Up, Mathf.Deg2Rad(spreadDegrees / 2))
        });
        coneSides.AddChild(new CollisionShape
        {
            Shape = new BoxShape { Extents = new Vector3(0.01f, 1, range / 2) },
            RotationDegrees = Vector3.Up * -spreadDegrees / 2,
            Translation = range / 2 * Vector3.Forward.Rotated(Vector3.Up, Mathf.Deg2Rad(-spreadDegrees / 2))
        });

        impactArea = new Area();
        impactArea.AddChild(new CollisionShape() { Shape = new CylinderShape { Radius = impactRange } });

        hintReticle = new Spatial();
        hintReticle.AddChild(new CSGBox
        {
            Scale = 0.5f * Vector3.One,
            Translation = 2 * Vector3.Up,
            Material = new SpatialMaterial
            {
                AlbedoColor = Colors.Green
            }
        });
        impactAimIndicator = new ConicalAimIndicator { SpreadDegrees = 360, Range = impactRange };
        hintReticle.AddChild(impactAimIndicator);

        AddChild(targetArea);
        AddChild(coneSides);
        AddChild(impactArea);
        AddChild(hintReticle);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (isRunning)
            return;
        if (showTargetHint)
        {
            targetEnemy = GetTargetEnemy();
            hintReticle.Visible = targetEnemy != null;

            if (targetEnemy != null)
            {

                Vector3 enemyLocation = targetEnemy.GlobalTransform.origin;
                Dictionary ray = GetWorld().DirectSpaceState.IntersectRay(GlobalTransform.origin, enemyLocation, collisionMask: 4);

                if (ray.Count == 0)
                    return;
                Vector3 enemyBoundary = (Vector3)ray["position"];

                impactAimIndicator.Translation = enemyBoundary - enemyLocation;

                Transform hintReticleGlobalTransform = hintReticle.GlobalTransform;
                hintReticleGlobalTransform.origin = enemyLocation;
                hintReticle.GlobalTransform = hintReticleGlobalTransform;
            }
        }
        else
        {
            hintReticle.Hide();
            targetEnemy = null;
        }
    }

    private Enemy GetTargetEnemy()
    {
        Array bodies = targetArea.GetOverlappingBodies();
        Enemy closestEnemy = null;
        float lowestDegreesDifference = 0;

        Vector3 forward = -GlobalTransform.basis.z;
        foreach (PhysicsBody body in bodies)
        {
            Vector3 bodyDisplacement = body.GlobalTransform.origin - GlobalTransform.origin;
            Plane upFacingPlane = new Plane(GlobalTransform.basis.y, 0);
            Vector3 flattenedBodyDisplacement = upFacingPlane.Project(bodyDisplacement);

            float degreesDifference = Mathf.Rad2Deg(forward.AngleTo(flattenedBodyDisplacement));

            if (!(body is Enemy))
                continue;

            if (degreesDifference > spreadDegrees / 2)
                continue;

            if (!(body as Enemy).HasStatus<MarkStatus>())
                continue;

            if (closestEnemy == null || degreesDifference < lowestDegreesDifference)
            {
                closestEnemy = body as Enemy;
                lowestDegreesDifference = degreesDifference;
            }
        }

        // If no enemies found within main cone area, look at the sides
        if (closestEnemy == null)
        {
            Array sideBodies = coneSides.GetOverlappingBodies();
            foreach (PhysicsBody body in sideBodies)
            {
                if (!(body is Enemy))
                    continue;

                if (!(body as Enemy).HasStatus<MarkStatus>())
                    continue;
                Vector3 bodyDisplacement = body.GlobalTransform.origin - GlobalTransform.origin;
                Plane upFacingPlane = new Plane(GlobalTransform.basis.y, 0);
                Vector3 flattenedBodyDisplacement = upFacingPlane.Project(bodyDisplacement);

                float degreesDifference = Mathf.Rad2Deg(forward.AngleTo(flattenedBodyDisplacement));
                if (closestEnemy == null || degreesDifference < lowestDegreesDifference)
                {
                    closestEnemy = body as Enemy;
                    lowestDegreesDifference = degreesDifference;
                }
            }
        }

        return closestEnemy;
    }

    protected override void OnAttackButtonPressed()
    {
        showTargetHint = true;
    }

    protected override void OnAttackButtonReleased()
    {
        showTargetHint = false;
        if (isRunning)
            return;
        Vector3 currentLocation = GlobalTransform.origin;
        Vector3 targetLocation;
        if (targetEnemy == null)
        {
            targetLocation = currentLocation + range * (-GlobalTransform.basis.z);
        }
        else
        {
            Vector3 enemyLocation = targetEnemy.GlobalTransform.origin;
            Dictionary ray = GetWorld().DirectSpaceState.IntersectRay(currentLocation, enemyLocation, collisionMask: 4);
            Vector3 enemyBoundary = (Vector3)ray["position"];

            // Go right up beside enemy
            targetLocation = enemyBoundary + player.Scale * GlobalTransform.basis.z;
        }

        Transform currentTransform = player.GlobalTransform;
        Transform targetTransform = new Transform(player.GlobalTransform.basis, targetLocation);

        float duration = (targetLocation - currentTransform.origin).Length() / dashSpeed;

        Tween tween = new Tween();
        AddChild(tween);
        tween.InterpolateProperty(player, "GlobalTransform", currentTransform, targetTransform, duration, transType: Tween.TransitionType.Quart);
        tween.InterpolateCallback(this, duration, "OnFinished");
        tween.Start();

        player.IsMovementLocked = true;
        isRunning = true;
    }

    public void OnFinished()
    {
        if (targetEnemy != null)
        {
            Vector3 targetEnemyDirection = (targetEnemy.GlobalTransform.origin - GlobalTransform.origin).Normalized();
            Vector3 accelerationOntargetEnemy = 50.0f * targetEnemyDirection + 40.0f * Vector3.Up;
            targetEnemy.Velocity += accelerationOntargetEnemy;
            targetEnemy.Damage(damage);

            foreach (PhysicsBody body in impactArea.GetOverlappingBodies())
            {
                if (!(body is Enemy))
                    continue;

                Enemy enemy = body as Enemy;
                Vector3 enemyDirection = (enemy.GlobalTransform.origin - GlobalTransform.origin).Normalized();
                Vector3 accelerationOnEnemy = 25.0f * enemyDirection + 20.0f * Vector3.Up;
                enemy.Velocity += accelerationOnEnemy;
                enemy.Damage(damage / 2);
            }

            overheatingGun.Cool(coolAmount);
        }
        player.IsMovementLocked = false;
        isRunning = false;
        targetEnemy = null;
    }
}

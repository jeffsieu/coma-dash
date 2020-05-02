using Godot;
using Godot.Collections;

public class DashSkill : Weapon
{
    private readonly float spreadDegrees;
    private readonly float dashSpeed;

    private Area targetArea;
    private Area coneSides;
    private Spatial hintReticle;
    private bool showTargetHint = false;
    private Player player;
    private Enemy targetEnemy;
    private bool isRunning = false;

    public DashSkill() : base(10.0f)
    {
        dashSpeed = 50.0f;
        spreadDegrees = 25.0f;
    }

    public override void _Ready()
    {
        base._Ready();
        player = GetParent<Player>();
        (AimIndicator as GeneralAimIndicator).IndicatorType = GeneralAimIndicator.AimIndicatorType.CONICAL;
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

        hintReticle = new CSGBox { Scale = 0.5f * Vector3.One };

        AddChild(targetArea);
        AddChild(coneSides);
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
                Transform hintReticleGlobalTransform = hintReticle.GlobalTransform;
                hintReticleGlobalTransform.origin = targetEnemy.GlobalTransform.origin + Vector3.Up * 2;
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
            targetEnemy.Velocity.y += 50f;
            // TODO: Add AOE effects
        }
        player.IsMovementLocked = false;
        isRunning = false;
        targetEnemy = null;
    }
}

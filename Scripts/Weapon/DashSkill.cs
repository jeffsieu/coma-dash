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
    private Tween tween;
    private Spatial hintReticle;
    private Spatial impactAimIndicator;
    private bool showTargetHint = false;
    private Player player;
    private Enemy targetEnemy;
    private bool isRunning = false;
    private float dashLeft;
    private float shaderTime = 2.0f;

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
        player = GetParent().GetParent<Player>();
        overheatingGun = player.Weapon;
        dashLeft = 0;

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

        impactArea = new Area();
        impactArea.AddChild(new CollisionShape() { Shape = new CylinderShape { Radius = impactRange } });

        hintReticle = new Spatial();
        hintReticle.AddChild(new CSGBox
        {
            Scale = 0.5f * Vector3.One,
            Translation = 2f * Vector3.Up,
            Material = new SpatialMaterial
            {
                AlbedoColor = Colors.Green
            }
        });
        impactAimIndicator = new ConicalAimIndicator { SpreadDegrees = 360, Range = impactRange };
        hintReticle.AddChild(impactAimIndicator);

        tween = new Tween();

        AddChild(tween);
        AddChild(targetArea);
        AddChild(coneSides);
        AddChild(impactArea);
        AddChild(hintReticle);
    }

    public override void _PhysicsProcess(float delta)
    {
        shaderTime += delta;
        MeshInstance m = GetNode<MeshInstance>("Shockwave");
        ShaderMaterial mat = m.GetSurfaceMaterial(0) as ShaderMaterial;
        mat.SetShaderParam("t", shaderTime);

        if (dashLeft > 0)
        {
            dashLeft -= delta;
            if (dashLeft <= 0)
                OnFinished();
        }
        if (isRunning)
            return;
        if (showTargetHint)
        {
            targetEnemy = GetTargetEnemy();
            if (targetEnemy != null)
            {
                Vector3 enemyLocation = targetEnemy.GlobalTransform.origin;
                Dictionary ray = GetWorld().DirectSpaceState.IntersectRay(GlobalTransform.origin, enemyLocation, collisionMask: ColLayer.Enemies);

                if (ray.Count == 0)
                {
                    hintReticle.Hide();
                    return;
                }

                hintReticle.Show();
                Vector3 enemyBoundary = (Vector3)ray["position"];

                impactAimIndicator.Translation = enemyBoundary - enemyLocation + enemyLocation.DirectionTo(GlobalTransform.origin) * player.Scale.z;

                Transform hintReticleGlobalTransform = hintReticle.GlobalTransform;
                hintReticleGlobalTransform.origin = enemyLocation;
                hintReticleGlobalTransform.origin.y = targetEnemy.Scale.y + 2.0f;
                hintReticleGlobalTransform.basis = Basis.Identity;
                hintReticle.GlobalTransform = hintReticleGlobalTransform;
            }
            else
            {
                hintReticle.Hide();
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

        // docs said that GetOverlappingBodies returns an array of PhysicsBody but it also returns
        // a CSGPolygon which isn't a PhysicsBody
        foreach (Spatial body in bodies)
        {
            if (!(body is Enemy))
                continue;

            Vector3 bodyDisplacement = body.GlobalTransform.origin - GlobalTransform.origin;
            Plane upFacingPlane = new Plane(GlobalTransform.basis.y, 0);
            Vector3 flattenedBodyDisplacement = upFacingPlane.Project(bodyDisplacement);

            float degreesDifference = Mathf.Rad2Deg(forward.AngleTo(flattenedBodyDisplacement));

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
            foreach (Spatial body in sideBodies)
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
        Vector3 targetLocation = Vector3.Zero;

        Dictionary rayToEnemy = null;

        // Check if enemy still exists before going to it
        // It could have died or moved away at this point
        if (targetEnemy != null)
        {
            Vector3 enemyLocation = targetEnemy.GlobalTransform.origin;
            rayToEnemy = GetWorld().DirectSpaceState.IntersectRay(currentLocation, enemyLocation, collisionMask: ColLayer.Enemies);
            if (rayToEnemy.Count > 0)
            {
                Vector3 enemyBoundary = (Vector3)rayToEnemy["position"];
                targetLocation = enemyBoundary + player.Scale * GlobalTransform.basis.z;
            }
            else
            {
                targetEnemy = null;
            }
        }

        if (targetEnemy == null)
        {
            targetLocation = currentLocation + range * (-GlobalTransform.basis.z);
        }

        float duration = (targetLocation - currentLocation).Length() / dashSpeed;
        player.IsMovementLocked = true;
        isRunning = true;
        dashLeft = duration;
        Vector3 direction = (targetLocation - currentLocation).Normalized();
        direction.y = 0;
        player.disableFriction = true;
        player.ApplyCentralImpulse(player.Mass * -player.LinearVelocity);
        player.ApplyCentralImpulse(direction * player.Mass * dashSpeed);
    }

    public void OnFinished()
    {

        player.ApplyCentralImpulse(player.Mass * -player.LinearVelocity);

        if (targetEnemy != null)
        {
            Vector3 targetEnemyDirection = (targetEnemy.GlobalTransform.origin - GlobalTransform.origin).Normalized();
            Vector3 targetEnemyVelocity = 5.0f * targetEnemyDirection + 15.0f * Vector3.Up;
            targetEnemy.ApplyCentralImpulse(targetEnemy.Mass * targetEnemyVelocity);
            targetEnemy.Damage(damage);
            player.Weapon.Cool(coolAmount);
            MeshInstance m = GetNode<MeshInstance>("Shockwave");
            ShaderMaterial mat = m.GetSurfaceMaterial(0) as ShaderMaterial;
            mat.SetShaderParam("t", 0);
            mat.SetShaderParam("radius", impactRange);
            shaderTime = 0;
        }

        foreach (Spatial body in impactArea.GetOverlappingBodies())
        {
            if (!(body is Enemy))
                continue;

            Enemy enemy = body as Enemy;
            if (enemy == targetEnemy)
                continue;

            Vector3 enemyDirection = (enemy.GlobalTransform.origin - GlobalTransform.origin).Normalized();
            Vector3 enemyVelocity = 2.5f * enemyDirection + 15.0f * Vector3.Up;
            enemy.ApplyCentralImpulse(enemy.Mass * enemyVelocity);
            enemy.Damage(damage / 2);
        }

        player.IsMovementLocked = false;
        player.disableFriction = false;
        isRunning = false;
        targetEnemy = null;
    }
}

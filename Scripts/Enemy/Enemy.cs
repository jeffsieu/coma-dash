using Godot;
using System.Collections.Generic;

public class Enemy : KinematicBody, IStatusHolder
{
    public Vector3 Velocity;
    private readonly float gravity = 4.85f;
    private float health = 100f;
    private SpatialMaterial material;
    private List<Status> statuses;


    public override void _Ready()
    {
        material = new SpatialMaterial()
        {
            AlbedoColor = Colors.White
        };

        CSGCylinder cylinder = GetNode<CSGCylinder>("CSGCylinder");
        cylinder.Material = material;
        statuses = new List<Status>();
    }

    public override void _PhysicsProcess(float delta)
    {
        Velocity.y -= gravity;
        Velocity.x = 0;
        Velocity.z = 0;
        Velocity = MoveAndSlide(Velocity);
        Color baseColor = Colors.Red.LinearInterpolate(Colors.Blue, GetPercentLeft<MarkStatus>());
        Color healthColor = baseColor.LinearInterpolate(Colors.White, 1 - health / 100);
        material.AlbedoColor = healthColor;
    }

    public void Damage(float damage)
    {
        health -= damage;

        if (health <= 0)
            QueueFree();
    }

    public bool HasStatus<S>() where S : Status
    {
        return GetStatusOrNull<S>() != null;
    }

    public S GetStatusOrNull<S>() where S : Status
    {
        foreach (Status status in statuses)
        {
            if (status.GetType() == typeof(S))
                return status as S;
        }
        return null;
    }

    public float GetPercentLeft<S>() where S : Status
    {
        S status = GetStatusOrNull<S>();
        if (status == null)
            return 0;
        return status.TimeLeft / status.Duration;
    }

    public void ApplyStatus<S>(float duration) where S : Status, new()
    {
        S currentStatus = GetStatusOrNull<S>();
        if (currentStatus != null)
        {
            // Reset the duration of currently applied status
            currentStatus.TimeLeft = duration;
        }
        else
        {
            S status = new S
            {
                TimeLeft = duration
            };
            statuses.Add(status);
            AddChild(status);
        }
    }

    public void OnStatusEnd(Status status)
    {
        statuses.Remove(status);
    }
}

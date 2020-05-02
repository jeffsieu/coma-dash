using Godot;

/// <summary>
/// Represents status effects induced by weapons or skills. Can be carried by <see cref="IStatusHolder"/>.
/// </summary>
public class Status : Node
{
    public float TimeLeft
    {
        get
        {
            return timeLeft;
        }
        set
        {
            timeLeft = value;
            Duration = value;
        }
    }
    public float Duration { get; private set; }

    private float timeLeft;
    protected IStatusHolder statusHolder;

    public override void _Ready()
    {
        statusHolder = GetParent<IStatusHolder>();
    }

    public override void _Process(float delta)
    {
        ApplyStatus(delta);
        timeLeft -= delta;
        if (timeLeft <= 0)
        {
            statusHolder.OnStatusEnd(this);
            QueueFree();
        }
    }

    protected virtual void ApplyStatus(float delta) { }
}

using Godot;

using State = System.Action<float>;

public class StateManager : Node
{
    private State currentState;
    private float stateElapsedDelta;
    private bool isPaused;

    public bool IsRunning
    {
        get
        {
            return currentState != null && !isPaused;
        }
    }

    public override void _Ready()
    {
        stateElapsedDelta = 0;
        isPaused = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (currentState != null && !isPaused)
        {
            currentState.Invoke(stateElapsedDelta);
            stateElapsedDelta += delta;
        }
    }

    public void GoTo(State state)
    {
        currentState = state;
        stateElapsedDelta = 0;
        isPaused = false;
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Stop()
    {
        currentState = null;
    }
}

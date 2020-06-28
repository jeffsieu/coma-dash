using Godot;

public class GUI : Control
{
    [Export]
    private readonly NodePath objectiveTextPath;

    [Export]
    private readonly NodePath timeLeftTextPath;

    public Label ObjectiveText
    {
        get
        {
            return GetNode(objectiveTextPath) as Label;
        }
    }


    public Label TimeLeftText
    {
        get
        {
            return GetNode(timeLeftTextPath) as Label;
        }
    }
}

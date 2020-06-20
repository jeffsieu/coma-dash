using System;
using Godot;

public class GUI : Control
{
    [Export]
    private NodePath objectiveTextPath;

    public Label ObjectiveText
    {
        get
        {
            return GetNode(objectiveTextPath) as Label;
        }
    }
}

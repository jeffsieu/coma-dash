using System;
using Godot;

public class Level : Spatial
{
    private static readonly float initialTime = 30.0f;
    private GUI gui;

    public float TimeLeft
    {
        get
        {
            return timeLeft;
        }
        set
        {
            timeLeft = value;

            if (timeLeft <= 0)
                GameOver();

            if (gui != null)
                gui.TimeLeftText.Text = FormatTime(timeLeft);
        }
    }
    private float timeLeft;

    public override void _Ready()
    {
        TimeLeft = initialTime;
        gui = GetNode<GUI>("GUI");
    }

    public override void _Process(float delta)
    {
        TimeLeft -= delta;
    }

    private string FormatTime(float timeLeft)
    {
        return $"{Math.Floor(timeLeft)} s";
    }

    public void GameOver()
    {

    }
}

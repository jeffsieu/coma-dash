using System;
using Godot;

public class Level : Spatial
{
	private static readonly float initialTime = 30.0f;
	private GUI gui;
	private Spatial enemyContainer;
	private readonly PackedScene bossScene = ResourceLoader.Load<PackedScene>("res://Scenes/Enemy/BigMeleeEnemy.tscn");

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

	public void Win(HealthEntity entity)
	{
		// TODO: Replace with level cleared screen
		GD.Print("Cleared level!");
	}

	public void CreateBoss(int x, int y)
	{
		Enemy boss = bossScene.Instance() as Enemy;
		enemyContainer = GetNode<Spatial>("Enemies");
		enemyContainer.AddChild(boss);

		Transform transform = boss.GlobalTransform;
		transform.origin = new Vector3(x, 0, y);

		boss.GlobalTransform = transform;
		boss.Connect("died", this, "Win");
	}
}

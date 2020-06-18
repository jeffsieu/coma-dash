using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies in a parent <see cref="Room"/>, and keeps track of their status.
/// </summary>
public class EnemySpawner : Node
{
    private Room room;
    private Spatial enemyContainer;
    private GUI gui;
    private int spawnCount;
    private int deadCount = 0;
    private readonly HashSet<Enemy> spawnedEnemies = new HashSet<Enemy>();
    private readonly PackedScene enemyScene = ResourceLoader.Load<PackedScene>("res://Scenes/Enemy.tscn");
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        room = GetParent<Room>();
        gui = GetTree().Root.GetNode("Level").GetNode<GUI>("GUI");
        enemyContainer = GetTree().Root.GetNode("Level").GetNode<Spatial>("Enemies");
        spawnCount = 5;
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < spawnCount; ++i)
        {
            Enemy enemy = enemyScene.Instance() as Enemy;
            Vector3 tilePosition = room.GetRandomTileCenter();
            Transform transform = enemy.GlobalTransform;
            transform.origin = tilePosition;

            enemy.GlobalTransform = transform;
            enemy.Connect("died", this, "OnEnemyDied");
            enemyContainer.AddChild(enemy);
            spawnedEnemies.Add(enemy);
        }
        UpdateObjectiveText();
    }

    public override void _Process(float delta)
    {
        // For debug purposes
        if (Input.IsKeyPressed((int)KeyList.F))
        {
            foreach (Enemy enemy in spawnedEnemies)
            {
                enemy.Damage(100);
            }
        }
    }

    public void OnEnemyDied(HealthEntity entity)
    {
        Enemy enemy = entity as Enemy;
        if (enemy == null)
            return;
        deadCount++;
        UpdateObjectiveText();

        if (deadCount == spawnCount)
        {
            room.OpenAllConnectedDoors();

            foreach (Room neighbor in room.GetConnectedRooms())
                neighbor.Activate();
        }
    }

    private void UpdateObjectiveText()
    {
        gui.ObjectiveText.Text = $"Defeat {deadCount}/{spawnCount} enemies";
    }
}

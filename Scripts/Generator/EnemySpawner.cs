using System.Collections.Generic;
using Godot;

/// <summary>
/// Spawns enemies in a parent <see cref="Room"/>, and keeps track of their status.
/// </summary>
public class EnemySpawner : Node
{
    private Room room;
    private Level level;
    private Spatial enemyContainer;
    private GUI gui;
    private int spawnCount;
    private int deadCount = 0;
    private readonly HashSet<Enemy> spawnedEnemies = new HashSet<Enemy>();
    private readonly PackedScene enemyScene = ResourceLoader.Load<PackedScene>("res://Scenes/Enemy/MeleeEnemy.tscn");

    public override void _Ready()
    {
        EnemyRoomBehavior enemyRoomBehavior = GetParent<EnemyRoomBehavior>();
        room = enemyRoomBehavior.GetParent<Room>();
        level = GetTree().Root.GetNodeOrNull<Level>("Level");
        gui = level?.GetNode<GUI>("GUI");
        enemyContainer = level?.GetNode<Spatial>("Enemies");
        spawnCount = 5;

        // If room is already activated at this point
        if (room.IsActive)
            OnRoomActivated(room);
    }

    public void OnRoomActivated(Room room)
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < spawnCount; ++i)
        {
            Enemy enemy = enemyScene.Instance() as Enemy;
            enemyContainer.AddChild(enemy);

            Vector3 tilePosition = room.GetRandomTileCenter();
            Transform transform = enemy.GlobalTransform;
            transform.origin = tilePosition;

            enemy.GlobalTransform = transform;
            enemy.Connect("died", this, "OnEnemyDied");
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
        UpdateTimeLeft();

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

    private void UpdateTimeLeft()
    {
        level.TimeLeft += 5;
    }
}

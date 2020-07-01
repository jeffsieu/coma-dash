using Godot;

public class EnemyRoomBehavior : Node
{
    private EnemySpawner enemySpawner;

    public override void _Ready()
    {
        Room room = GetParent<Room>();
        enemySpawner = new EnemySpawner();
        AddChild(enemySpawner);
        room.Connect("activated", enemySpawner, "OnRoomActivated");
    }
}

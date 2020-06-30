using Godot;

public class EnemyRoomBehavior : Node
{
    private EnemySpawner enemySpawner;

    public override void _Ready()
    {
        Room room = GetParent<Room>();

        enemySpawner = new EnemySpawner();
        AddChild(enemySpawner);

        GD.Print("im connected");
        room.Connect("activated", enemySpawner, "OnRoomActivated");
    }
}

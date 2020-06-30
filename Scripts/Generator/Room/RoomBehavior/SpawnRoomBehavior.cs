using Godot;

public class SpawnRoomBehavior : Node
{
    public override void _Ready()
    {
        Room room = GetParent<Room>();
        room.OpenAllConnectedDoors();
        foreach (Room neighbor in room.GetConnectedRooms())
            neighbor.Activate();
    }
}

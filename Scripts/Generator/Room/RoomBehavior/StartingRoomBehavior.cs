using Godot;

public class StartingRoomBehavior : Node
{
    public override void _Ready()
    {
        Room room = GetParent<Room>();
        room.OpenAllConnectedDoors();
        foreach (Room neighbor in room.GetConnectedRooms())
            neighbor.Activate();
    }
}

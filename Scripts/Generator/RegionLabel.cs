using Godot;
using System;

public enum RegionType
{
    NONE, WALL, ROOM, DOOR
}

public class RegionLabel
{
    public RegionType Type { get; private set; }
    public int Id { get; private set; }
    public RegionLabel(RegionType type, int id)
    {
        Type = type;
        Id = id;
    }
}

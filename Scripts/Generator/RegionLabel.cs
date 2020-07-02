using System;
using Godot;

public enum RegionType
{
    NONE, WALL, ROOM, DOOR, TRANSPARENT_WALL
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

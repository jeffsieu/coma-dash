using System;
using System.Collections.Generic;
using Godot;

public class Door : LevelRegion
{
    private CSGPolygon doorMesh;
    public HashSet<Room> ConnectedRooms { get; private set; }

    public Door(RegionShape regionShape, int unitSize)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;

        ConnectedRooms = new HashSet<Room>();

        doorMesh = new CSGPolygon
        {
            Polygon = regionShape.MainPolygon
        };
        SpatialMaterial doorMaterial = new SpatialMaterial();
        doorMaterial.AlbedoColor = Colors.AliceBlue;
        doorMesh.Material = doorMaterial;
        doorMesh.UseCollision = true;
        doorMesh.CollisionLayer = ColLayer.Environment;
        doorMesh.CollisionMask = ColLayer.Environment;
        AddChild(doorMesh);
    }

    public void Open()
    {
        // TODO: Add animations or effects
        doorMesh.UseCollision = false;
        doorMesh.Visible = false;
    }

    public void Close()
    {
        // TODO: Add animations or effects
        doorMesh.UseCollision = true;
        doorMesh.Visible = true;
    }

    public void ConnectRoom(Room room)
    {
        ConnectedRooms.Add(room);
    }
}

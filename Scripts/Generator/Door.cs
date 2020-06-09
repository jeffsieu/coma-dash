using System;
using Godot;

public class Door : Room
{
    public Door(Vector2[][] polygon, int unitSize, Material material) : base(polygon, unitSize, material)
    {
        CSGPolygon doorMesh = new CSGPolygon
        {
            Polygon = polygon[0],
            RotationDegrees = new Vector3(90, 0, 0),
            Scale = unitSize * Vector3.One
        };

        SpatialMaterial doorMaterial = new SpatialMaterial();
        doorMaterial.AlbedoColor = Colors.AliceBlue;

        doorMesh.Material = doorMaterial;
        AddChild(doorMesh);

        CollisionLayer = 1;
        AddChild(new CollisionPolygon
        {
            Polygon = polygon[0],
            RotationDegrees = new Vector3(90, 0, 0),
            Scale = unitSize * Vector3.One
        });
    }
}

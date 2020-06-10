using System;
using Godot;

public class Door : StaticBody
{
    public Door(Vector2[][] polygon, int unitSize, Material material)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;
        CSGPolygon doorMesh = new CSGPolygon
        {
            Polygon = polygon[0]
        };

        SpatialMaterial doorMaterial = new SpatialMaterial();
        doorMaterial.AlbedoColor = Colors.AliceBlue;
        doorMesh.Material = doorMaterial;
        AddChild(doorMesh);

        doorMesh.UseCollision = true;
        doorMesh.CollisionLayer = 1;
        doorMesh.CollisionMask = 1;
    }
}

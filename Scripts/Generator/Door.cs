using System;
using Godot;

public class Door : LevelRegion
{
    private CSGPolygon doorMesh;
    public Door(Vector2[][] polygon, int unitSize)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;
        doorMesh = new CSGPolygon
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
}

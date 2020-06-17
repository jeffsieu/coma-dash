using System;
using Godot;

public class Wall : LevelRegion
{
    private readonly float WALL_HEIGHT = 2.2f;
    public Wall(Vector2[][] polygon, int unitSize, Material material)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;

        CSGPolygon wallMesh = new CSGPolygon
        {
            Polygon = polygon[0],
            Material = material,
            Depth = WALL_HEIGHT
        };

        for (int i = 1; i < polygon.Length; ++i)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = polygon[i],
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f * WALL_HEIGHT
            };
            wallMesh.AddChild(holeMesh);
        }
        AddChild(wallMesh);
        wallMesh.UseCollision = true;
        wallMesh.CollisionLayer = 1;
        wallMesh.CollisionMask = 1;
    }
}
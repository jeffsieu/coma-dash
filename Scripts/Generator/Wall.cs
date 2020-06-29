using System;
using Godot;

public class Wall : LevelRegion
{
    private readonly float WALL_HEIGHT = 2.2f;
    public Wall(PolygonPoints polygonPoints, int unitSize, Material material)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;

        CSGPolygon wallMesh = new CSGPolygon
        {
            Polygon = polygonPoints.MainPolygon,
            Material = material,
            Depth = WALL_HEIGHT
        };

        for (int i = 0; i < polygonPoints.HolePolygons.Length; ++i)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = polygonPoints.HolePolygons[i],
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f * WALL_HEIGHT
            };
            wallMesh.AddChild(holeMesh);
        }
        AddChild(wallMesh);
        wallMesh.UseCollision = true;
        wallMesh.CollisionLayer = ColLayer.Environment;
        wallMesh.CollisionMask = ColLayer.Environment;
    }
}
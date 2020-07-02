using System;
using Godot;

public class Wall : LevelRegion
{
    private readonly float WALL_HEIGHT = 2.2f;
    public Wall(RegionShape regionShape, int unitSize, Material material)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;

        CreateWallMesh(regionShape, unitSize, material);
    }

    private void CreateWallMesh(RegionShape regionShape, int unitSize, Material material)
    {
        CSGPolygon wallMesh = new CSGPolygon
        {
            Polygon = regionShape.MainPolygon,
            MaterialOverride = material,
            Depth = WALL_HEIGHT
        };

        foreach (Vector2[] holePolygon in regionShape.HolePolygons)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = holePolygon,
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f * WALL_HEIGHT
            };
            wallMesh.AddChild(holeMesh);
        }
        wallMesh.UseCollision = true;
        wallMesh.CollisionLayer = ColLayer.Environment;
        wallMesh.CollisionMask = ColLayer.Environment;
        AddChild(wallMesh);
    }
}
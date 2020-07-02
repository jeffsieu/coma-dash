using System;
using Godot;

public class TransparentWall : LevelRegion
{
    private readonly float WALL_HEIGHT = 2.2f;
    private CSGPolygon wallMesh, floorMesh;
    private Material normalMaterial, transparentMaterial;
    public TransparentWall(RegionShape regionShape, int unitSize, Material normalMaterial, Material transparentMaterial)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;

        this.normalMaterial = normalMaterial;
        this.transparentMaterial = transparentMaterial;

        CreateWallMesh(regionShape, unitSize);
        CreateFloorMesh(regionShape, unitSize);

        HideWall();
    }

    private void CreateWallMesh(RegionShape regionShape, int unitSize)
    {
        wallMesh = new CSGPolygon
        {
            Polygon = regionShape.MainPolygon,
            Material = normalMaterial,
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

    private void CreateFloorMesh(RegionShape regionShape, int unitSize)
    {
        floorMesh = new CSGPolygon
        {
            Polygon = regionShape.MainPolygon,
            Material = normalMaterial,
            Depth = 1
        };

        foreach (Vector2[] holePolygon in regionShape.HolePolygons)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = holePolygon,
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f
            };
            floorMesh.AddChild(holeMesh);
        }

        floorMesh.Translation = new Vector3(0, 0, 1.0f);

        AddChild(floorMesh);
    }

    public void HideWall()
    {
        wallMesh.Material = transparentMaterial;
    }

    public void UnhideWall()
    {
        wallMesh.Material = normalMaterial;
    }
}
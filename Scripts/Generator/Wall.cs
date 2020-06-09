using System;
using Godot;

public class Wall : StaticBody
{
    public Wall(Vector2[][] polygon, int unitSize, Material material)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;

        CSGPolygon wallMesh = new CSGPolygon
        {
            Polygon = polygon[0],
            Material = material
        };

        for (int i = 1; i < polygon.Length; ++i)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = polygon[i],
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f
            };
            wallMesh.AddChild(holeMesh);
            AddChild(new CollisionPolygon{
                Polygon = polygon[i]
            });
        }
        AddChild(wallMesh);
    }
}
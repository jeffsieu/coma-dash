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
            Polygon = polygon[2],
            Material = material
        };

        // CollisionLayer = 1;
        // wallMesh.AddChild(new CollisionPolygon
        // {
        //     Polygon = polygon[0],
        //     RotationDegrees = new Vector3(90, 0, 0),
        //     Scale = unitSize * Vector3.One
        // });

        for (int i = 1; i < polygon.Length; ++i)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = polygon[i],
                Operation = CSGShape.OperationEnum.Subtraction
            };
            wallMesh.AddChild(holeMesh);
        }

        // CollisionLayer = 1;
        // holeMesh.AddChild(new CollisionPolygon
        // {
        //     Polygon = polygon[i],
        //     RotationDegrees = new Vector3(90, 0, 0),
        //     Scale = unitSize * Vector3.One
        // });
        AddChild(wallMesh);
    }
}
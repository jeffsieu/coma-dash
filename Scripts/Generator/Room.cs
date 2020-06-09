using System;
using Godot;

public class Room : StaticBody
{
    public Room(Vector2[][] polygon, int unitSize, Material material)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;
        CSGPolygon floorMesh = new CSGPolygon
        {
            Polygon = polygon[0]
        };

        float minX = polygon[0][0].x, maxX = polygon[0][0].x;
        float minY = polygon[0][0].y, maxY = polygon[0][0].y;
        foreach (Vector2 point in polygon[0])
        {
            minX = Mathf.Min(point.x, minX);
            maxX = Mathf.Max(point.x, maxX);
            minY = Mathf.Min(point.y, minY);
            maxY = Mathf.Max(point.y, maxY);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        // make a duplicate of the shader so that can customize the `size` uniform
        // might want to see if this costs anything? I don't think so though
        // 5 lines of shader code shouldn't cost anything
        ShaderMaterial dupMaterial = (ShaderMaterial)material.Duplicate();
        dupMaterial.SetShaderParam("size", new Vector2(width, height));
        floorMesh.Material = dupMaterial;

        for (int i = 1; i < polygon.Length; ++i)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = polygon[i],
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f
            };
            floorMesh.AddChild(holeMesh);
            AddChild(new CollisionPolygon
            {
                Polygon = polygon[i]
            });
        }

        AddChild(floorMesh);
        Translation = new Vector3(0, -unitSize, 0);
    }
}
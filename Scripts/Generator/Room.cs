using Godot;
using System;

public class Room : StaticBody
{
    public Room(Vector2[] polygon, int unitSize, ShaderMaterial material)
    {
        CSGPolygon floorMesh = new CSGPolygon
        {
            Polygon = polygon,
            RotationDegrees = new Vector3(90, 0, 0),
            Scale = unitSize * Vector3.One
        };

        // make a duplicate of the shader so that can customize the `size` uniform
        // might want to see if this costs anything? I don't think so though
        // 5 lines of shader code shouldn't cost anything
        ShaderMaterial dupMaterial = (ShaderMaterial)material.Duplicate();

        float minX = polygon[0].x, maxX = polygon[0].x;
        float minY = polygon[0].y, maxY = polygon[0].y;
        foreach (Vector2 point in polygon)
        {
            minX = Mathf.Min(point.x, minX);
            maxX = Mathf.Max(point.x, maxX);
            minY = Mathf.Min(point.y, minY);
            maxY = Mathf.Max(point.y, maxY);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        dupMaterial.SetShaderParam("size", new Vector2(width, height));

        floorMesh.Material = dupMaterial;

        AddChild(floorMesh);
        Translation = new Vector3(0, -unitSize, 0);
    }
}
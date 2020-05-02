using Godot;
using System;

public class Floor : StaticBody
{
    public Floor(Vector2 position, Vector2 dimensions, ShaderMaterial material)
    {
        Mesh floorMesh = new CubeMesh
        {
            Size = Vector3.One
        };
        MeshInstance meshInstance = new MeshInstance
        {
            Mesh = floorMesh
        };

        // make a duplicate of the shader so that can customize the `size` uniform
        // might want to see if this costs anything? I don't think so though
        // 5 lines of shader code shouldn't cost anything
        ShaderMaterial dupMaterial = (ShaderMaterial)material.Duplicate();
        dupMaterial.SetShaderParam("size", new Vector2(dimensions.x, dimensions.y));
        meshInstance.SetSurfaceMaterial(0, dupMaterial);

        CollisionShape collisionShape = new CollisionShape
        {
            Shape = new BoxShape
            {
                Extents = 0.5f * Vector3.One
            }
        };
        AddChild(meshInstance);
        AddChild(collisionShape);

        Scale = new Vector3(dimensions.x, 1, dimensions.y);
        Translation = new Vector3(position.x + dimensions.x / 2, -1, position.y + dimensions.y / 2);
    }
}
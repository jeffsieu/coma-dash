using Godot;
using System;

public class Floor : StaticBody
{
    public Floor(Vector2 position, Vector2 dimensions, Material material)
    {
        Vector3 wallUp = Vector3.Up;
        Mesh wallMesh = new CubeMesh();
        MeshInstance meshInstance = new MeshInstance
        {
            Mesh = wallMesh
        };

        // make a duplicate of the shader so that can customize the `size` uniform
        // might want to see if this costs anything? I don't think so though
        // 5 lines of shader code shouldn't cost anything
        ShaderMaterial dupMaterial = (ShaderMaterial)material.Duplicate();
        dupMaterial.SetShaderParam("size", new Vector2(dimensions.x, dimensions.y));
        meshInstance.SetSurfaceMaterial(0, dupMaterial);

        CollisionShape collisionShape = new CollisionShape()
        {
            Shape = new BoxShape()
        };
        AddChild(meshInstance);
        AddChild(collisionShape);

        // divide scale by 2 because not sure why godot seems to double the effect
        Scale = new Vector3(dimensions.x / 2, 1, dimensions.y / 2);
        Translation = new Vector3(position.x + dimensions.x / 2, -1, position.y + dimensions.y / 2);
    }

    public override void _Process(float delta)
    {
    }
}
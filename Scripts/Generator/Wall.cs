using Godot;
using System;

public class Wall : StaticBody
{
    public Wall(Vector2 position, Vector2 dimensions, Material material)
    {
        Vector3 wallUp = Vector3.Up;
        Mesh wallMesh = new CubeMesh();
        MeshInstance meshInstance = new MeshInstance
        {
            Mesh = wallMesh
        };
        meshInstance.SetSurfaceMaterial(0, material);

        CollisionShape collisionShape = new CollisionShape()
        {
            Shape = new BoxShape()
        };
        AddChild(meshInstance);
        AddChild(collisionShape);

        // divide scale by 2 because not sure why godot seems to double the effect
        Scale = new Vector3(dimensions.x / 2, 3, dimensions.y / 2);
        Translation = new Vector3(position.x + dimensions.x / 2, 1, position.y + dimensions.y / 2);
    }

    public override void _Process(float delta)
    {
    }
}
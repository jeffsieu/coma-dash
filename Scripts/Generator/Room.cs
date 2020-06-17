using System;
using System.Collections.Generic;
using Godot;

public class Room : LevelRegion
{
    private CSGPolygon floorMesh;
    public HashSet<Door> ConnectedDoors { get; private set; }

    public Room(Vector2[][] polygon, int unitSize, Material material)
    {
        ConnectedDoors = new HashSet<Door>();

        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;
        floorMesh = new CSGPolygon
        {
            Polygon = polygon[0]
        };

        for (int i = 1; i < polygon.Length; ++i)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = polygon[i],
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f
            };
        }

        SetFloorShaderParams(polygon[0], (ShaderMaterial)material);

        AddChild(floorMesh);
        Translation = new Vector3(0, -unitSize, 0);

        floorMesh.UseCollision = true;
        floorMesh.CollisionLayer = 1;
        floorMesh.CollisionMask = 1;
    }

    public override void _PhysicsProcess(float delta)
    {
        // Todo: Remove after EnemySpawner is implemented
        if (Input.IsKeyPressed((int) KeyList.H))
        {
            OpenAllConnectedDoors();
        }
        if (Input.IsKeyPressed((int) KeyList.J))
        {
            CloseAllConnectedDoors();
        }
    }

    private void SetFloorShaderParams(Vector2[] polygon, ShaderMaterial material)
    {
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

        // make a duplicate of the shader so that can customize the `size` uniform
        // might want to see if this costs anything? I don't think so though
        // 5 lines of shader code shouldn't cost anything
        ShaderMaterial dupMaterial = (ShaderMaterial)material.Duplicate();
        dupMaterial.SetShaderParam("size", new Vector2(width, height));
        floorMesh.Material = dupMaterial;
    }

    public void AddDoor(Door door)
    {
        ConnectedDoors.Add(door);
    }

    public void OpenAllConnectedDoors()
    {
        foreach (Door door in ConnectedDoors) door.Open();
    }

    public void CloseAllConnectedDoors()
    {
        foreach (Door door in ConnectedDoors) door.Close();
    }
}
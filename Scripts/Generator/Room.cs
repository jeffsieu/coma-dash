using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Room : LevelRegion
{
    private MapLoader mapLoader;
    private readonly CSGPolygon floorMesh;
    private readonly EnemySpawner enemySpawner;

    public HashSet<Door> ConnectedDoors { get; private set; }
    public Vector2[] Tiles { get; private set; }

    private readonly RandomNumberGenerator rng;
    private readonly int unitSize;
    private bool isActive = false;

    public Room(Vector2[][] polygon, Vector2[] tiles, int unitSize, Material material)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;
        Translation = new Vector3(0, -unitSize, 0);

        ConnectedDoors = new HashSet<Door>();
        Tiles = tiles;
        rng = new RandomNumberGenerator();
        rng.Randomize();

        this.unitSize = unitSize;

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
        floorMesh.UseCollision = true;
        floorMesh.CollisionLayer = ColLayer.Environment;
        floorMesh.CollisionMask = ColLayer.Environment;

        enemySpawner = new EnemySpawner();
        SetFloorShaderParams(polygon[0], (ShaderMaterial)material);
    }

    public override void _Ready()
    {
        base._Ready();
        mapLoader = GetParent<MapLoader>();
        AddChild(floorMesh);
        AddChild(enemySpawner);

        Player player = GetTree().Root.GetNode("Level").GetNode<Player>("Player");
        if (Contains(player))
        {
            Activate();
        }
    }

    public override void _Process(float delta)
    {
        // Todo: Remove after EnemySpawner is implemented
        if (Input.IsKeyPressed((int)KeyList.H))
        {
            OpenAllConnectedDoors();
        }
        if (Input.IsKeyPressed((int)KeyList.J))
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

    public void Activate()
    {
        if (!isActive)
        {
            isActive = true;
            enemySpawner.SpawnEnemies();
        }
    }

    public void ConnectDoor(Door door)
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

    public HashSet<Room> GetConnectedRooms()
    {
        HashSet<Room> rooms = new HashSet<Room>();
        foreach (Door door in ConnectedDoors)
        {
            foreach (Room room in door.ConnectedRooms)
                if (room != this)
                    rooms.Add(room);
        }   
        return rooms;
    }

    public Vector3 GetRandomTileCenter()
    {
        Vector2 tile = Tiles[rng.RandiRange(0, Tiles.Length - 1)];
        return new Vector3((tile.x + 0.5f)* unitSize, 0, (tile.y + 0.5f) * unitSize) + mapLoader.GlobalTransform.origin;
    }

    public bool Contains(Spatial spatial)
    {
        Vector3 localTranslation = spatial.GlobalTransform.origin - mapLoader.GlobalTransform.origin;
        int localX = (int) localTranslation.x / unitSize;
        int localY = (int) localTranslation.z / unitSize;
        return Tiles.Contains<Vector2>(new Vector2(localX, localY));
    }
}

using Godot;
using System;
using System.Collections.Generic;

public class MapLoader : Spatial
{
    [Export]
    public int UnitSize
    {
        get
        {
            return unitSize;
        }
        set
        {
            unitSize = value;
            if (ready)
                RebuildMap();
        }
    }

    [Export(PropertyHint.File)]
    public String MapPath
    {
        get
        {
            return mapPath;
        }
        set
        {
            mapPath = value;
            if (ready)
                RebuildMap();
        }
    }

    [Export]
    public Material WallMaterial
    {
        get
        {
            return wallMaterial;
        }
        set
        {
            wallMaterial = value;
            if (ready)
                RebuildMap();
        }
    }

    [Export]
    public ShaderMaterial FloorMaterial
    {
        get
        {
            return floorMaterial;
        }
        set
        {
            floorMaterial = value;
            if (ready)
                RebuildMap();
        }
    }

    private int unitSize;
    private Material wallMaterial;
    private ShaderMaterial floorMaterial;
    private string mapPath;

    private int size;
    private bool ready = false;

    public override void _Ready()
    {
        BuildMap(ParseMap());
        ready = true;
    }

    private void RebuildMap()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
            RemoveChild(child);
        }
        BuildMap(ParseMap());
    }

    private List<int> ParseMap()
    {
        List<int> pixels = new List<int>();
        File mapFile = new File();
        mapFile.Open(mapPath, File.ModeFlags.Read);
        while (!mapFile.EofReached())
        {
            Int32 pixel = mapFile.Get8() << 16;
            pixel |= mapFile.Get8() << 8;
            pixel |= mapFile.Get8();
            pixels.Add(pixel);
        }
        size = (int)Math.Sqrt(pixels.Count);
        return pixels;
    }

    private void BuildMap(List<int> pixels)
    {
        BitMap floorMap = new BitMap();
        floorMap.Create(Vector2.One * size);
        BitMap doorMap = new BitMap();
        doorMap.Create(Vector2.One * size);

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                int index = y * size + x;
                int pixel = pixels[index];

                // Wall, white
                if (pixel == 0xffffff)
                {
                    Vector2 cellPosition = new Vector2(x, y) * unitSize;
                    Vector2 cellSize = Vector2.One * unitSize;
                    Wall wall = new Wall(cellPosition, new Vector3(cellSize.x, 6, cellSize.y), wallMaterial);
                    AddChild(wall);
                    continue;
                }

                // Door, red
                if (pixel == 0xff0000)
                {
                    doorMap.SetBit(new Vector2(x, y), true);
                    continue;
                }

                // Starting tile, green
                if (pixel == 0x00ff00)
                {
                    Translation = new Vector3((-x) * unitSize, -1, (-y) * unitSize);
                }

                // Room floor, other colors
                floorMap.SetBit(new Vector2(x, y), true);
            }
        }

        foreach (Vector2[] polygon in floorMap.OpaqueToPolygons(new Rect2(Vector2.Zero, Vector2.One * size)))
        {
            AddChild(new Room(polygon, unitSize, FloorMaterial));
        }

        foreach (Vector2[] polygon in doorMap.OpaqueToPolygons(new Rect2(Vector2.Zero, Vector2.One * size)))
        {
            AddChild(new Door(polygon, unitSize, FloorMaterial));
        }

        float mapLength = size * unitSize;
        StaticBody floor = new StaticBody
        {
            CollisionLayer = 1
        };
        floor.Name = "Floor";
        floor.AddChild(new CollisionShape
        {
            Shape = new BoxShape()
        });

        floor.Scale = new Vector3(mapLength, 1, mapLength);
        AddChild(floor);
    }
}

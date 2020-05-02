using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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

    private class Bounds
    {
        public int left, right, top, bottom;
    }

    private int unitSize;
    private Material wallMaterial;
    private ShaderMaterial floorMaterial;
    private string mapPath;

    private int size;
    private List<StaticBody> children = new List<StaticBody>();
    private bool ready = false;

    public override void _Ready()
    {
        BuildMap(ParseMap());
        ready = true;
    }

    private void RebuildMap()
    {
        foreach (var child in children)
        {
            child.QueueFree();
            RemoveChild(child);
        }
        children.Clear();
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
        List<Bounds> bounds = new List<Bounds>();
        int[] groupings = new int[size * size];

        for (int i = 0; i < size * size; ++i)
            groupings[i] = -1;

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                int index = y * size + x;
                int leftIndex = y * size + x - 1;

                int pixel = pixels[index];

                if (pixel == 0xffffff)
                    continue;

                // for now, add walls as 1x1 unit blocks
                if (pixel == 0xff0000)
                {
                    Vector2 cellPosition = new Vector2(x - size / 2, y - size / 2) * unitSize;
                    Vector2 cellSize = Vector2.One * unitSize;
                    Wall wall = new Wall(cellPosition, cellSize, wallMaterial);
                    AddChild(wall);
                    children.Add(wall);
                    continue;
                }

                // group up adjacent pixels that are not walls
                // very naive algorithm for now
                if (x > 0 && pixels[leftIndex] == pixels[index])
                    UpdateBounds(leftIndex, x, y, bounds, groupings);
                else
                    AddBounds(x, y, bounds, groupings);
            }
        }

        for (int i = 0; i < bounds.Count; ++i)
        {
            int x = bounds[i].left;
            int y = bounds[i].top;

            Vector2 cellPosition = new Vector2(x - size / 2, y - size / 2) * unitSize;
            Vector2 cellSize = new Vector2(bounds[i].right - x + 1, bounds[i].bottom - y + 1) * unitSize;
            Floor floor = new Floor(cellPosition, cellSize, floorMaterial);
            AddChild(floor);
            children.Add(floor);
        }
    }

    private void AddBounds(int x, int y, List<Bounds> bounds, int[] groupings)
    {
        int index = y * size + x;
        groupings[index] = bounds.Count;
        bounds.Add(new Bounds()
        {
            right = x,
            left = x,
            top = y,
            bottom = y
        });
    }

    private void UpdateBounds(int prevIndex, int x, int y, List<Bounds> bounds, int[] groupings)
    {
        int index = y * size + x;
        int group = groupings[index] = groupings[prevIndex];
        bounds[group].right = Math.Max(bounds[group].right, x);
        bounds[group].bottom = Math.Max(bounds[group].bottom, y);
    }
}

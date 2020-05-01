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
        }
    }

    [Export]
    public Material FloorMaterial
    {
        get
        {
            return floorMaterial;
        }
        set
        {
            floorMaterial = value;
        }
    }

    private class Bounds
    {
        public int left, right, top, bottom;
    }

    private int unitSize;
    private Material wallMaterial;
    private Material floorMaterial;
    private String mapPath;
    private File mapFile;

    private List<int> pixels = new List<int>();
    private int size;
    private List<Bounds> bounds = new List<Bounds>();
    private int[] groupings;

    public override void _Ready()
    {
        mapFile = new File();
        mapFile.Open(mapPath, File.ModeFlags.Read);
        while (!mapFile.EofReached())
        {
            Int32 pixel = mapFile.Get8() << 16;
            pixel |= mapFile.Get8() << 8;
            pixel |= mapFile.Get8();
            pixels.Add(pixel);
        }
        size = (int)Math.Sqrt(pixels.Count);

        parseMap();
    }

    private void parseMap()
    {
        groupings = new int[size * size];

        for (int i = 0; i < size * size; ++i)
            groupings[i] = -1;

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                int index = y * size + x;
                int upIndex = (y - 1) * size + x;
                int leftIndex = y * size + x - 1;

                int pixel = pixels[index];

                if (pixel == 0xffffff)
                    continue;

                // for now, add walls as 1x1 unit blocks
                if (pixel == 0xff0000)
                {
                    Vector2 cellPosition = new Vector2(x - size / 2, y - size / 2) * unitSize;
                    Vector2 cellSize = new Vector2(1, 1) * unitSize;
                    AddChild(new Wall(cellPosition, cellSize, wallMaterial));
                    continue;
                }

                // group up adjacent pixels that are not walls
                // very naive algorithm for now
                if (x > 0 && pixels[leftIndex] == pixels[index])
                    updateBounds(leftIndex, x, y);
                else
                    addNewBounds(x, y);
            }
        }

        for (int i = 0; i < bounds.Count; ++i)
        {
            int x = bounds[i].left;
            int y = bounds[i].top;

            Vector2 cellPosition = new Vector2(x - size / 2, y - size / 2) * unitSize;
            Vector2 cellSize = new Vector2(bounds[i].right - x + 1, bounds[i].bottom - y + 1) * unitSize;
            AddChild(new Floor(cellPosition, cellSize, floorMaterial));
        }
    }

    private void addNewBounds(int x, int y)
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

    private void updateBounds(int prevIndex, int x, int y)
    {
        int index = y * size + x;
        int group = groupings[index] = groupings[prevIndex];
        bounds[group].right = Math.Max(bounds[group].right, x);
        bounds[group].bottom = Math.Max(bounds[group].bottom, y);
    }
}

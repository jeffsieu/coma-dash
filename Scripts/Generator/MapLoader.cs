using System;
using System.Collections.Generic;
using Godot;

[Tool]
public class MapLoader : Spatial
{
    [Export]
    public bool Preview
    {
        get
        {
            return preview;
        }
        set
        {
            preview = value;
            if (!preview) RemoveAllChildren();
            else
                RebuildMap();
        }
    }

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
            RebuildMap();
        }
    }

    private bool preview;

    private int unitSize;
    private Material wallMaterial;
    private ShaderMaterial floorMaterial;
    private string mapPath;

    private int size;
    private bool ready = false;

    public override void _Ready()
    {
        ready = true;
        RebuildMap();
    }

    private void RebuildMap()
    {
        if (!ready || (Engine.EditorHint && !Preview)) return;
        RemoveAllChildren();
        BuildMap(ParseMap());
    }

    private void RemoveAllChildren()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
            RemoveChild(child);
        }
    }

    private int[,] ParseMap()
    {
        File mapFile = new File();
        Error openError = mapFile.Open(mapPath, File.ModeFlags.Read);
        if (openError != Error.Ok)
        {
            size = 0;
            return new int[0, 0];
        }

        // each pixel is 3 bytes wide
        size = (int)Math.Sqrt(mapFile.GetLen() / 3);
        int[,] pixels = new int[size, size];
        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                Int32 pixel = mapFile.Get8() << 16;
                pixel |= mapFile.Get8() << 8;
                pixel |= mapFile.Get8();
                pixels[x, y] = pixel;
            }
        }
        return pixels;
    }

    private void BuildMap(int[,] pixels)
    {
        BitMap floorMap = new BitMap();
        floorMap.Create(Vector2.One * size);
        BitMap doorMap = new BitMap();
        doorMap.Create(Vector2.One * size);
        BitMap wallMap = new BitMap();
        wallMap.Create(Vector2.One * size);

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                int pixel = pixels[x, y];

                if (pixel == 0)
                    continue;

                // Wall, white
                if (pixel == 0xffffff)
                {
                    // Vector2 cellPosition = new Vector2(x, y) * unitSize;
                    // Vector2 cellSize = Vector2.One * unitSize;
                    // Wall wall = new Wall(cellPosition, new Vector3(cellSize.x, 6, cellSize.y), wallMaterial);
                    // AddChild(wall);

                    wallMap.SetBit(new Vector2(x, y), true);
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

        foreach (Vector2[][] polygon in PolygonsFromBitmap(wallMap))
        {
            AddChild(new Wall(polygon, unitSize, WallMaterial));
        }

        foreach (Vector2[][] polygon in PolygonsFromBitmap(floorMap))
        {
            AddChild(new Room(polygon, unitSize, FloorMaterial));
        }

        foreach (Vector2[][] polygon in PolygonsFromBitmap(doorMap))
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

    private List<Vector2[][]> PolygonsFromBitmap(BitMap bitmap)
    {
        List<Vector2[][]> polygons = new List<Vector2[][]>();
        bool[,] visitedCell = new bool[size, size]; // visited points on the grid, not pixels
        for (int y = 0; y < size; ++y) for (int x = 0; x < size; ++x) visitedCell[x, y] = false;

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                // group connected cells together then traverse the sides to generate a Vector2[] of polygon vertices
                if (!visitedCell[x, y] && bitmap.GetBit(new Vector2(x, y)))
                {
                    int nsize = size + 2;
                    bool[,] connectedBitmap = new bool[nsize, nsize];
                    for (int yy = 0; yy < nsize; ++yy) for (int xx = 0; xx < nsize; ++xx)
                            connectedBitmap[xx, yy] = true;

                    Stack<Tuple<int, int>> stack = new Stack<Tuple<int, int>>();
                    stack.Push(new Tuple<int, int>(x, y));
                    visitedCell[x, y] = true;
                    while (stack.Count > 0)
                    {
                        Tuple<int, int> top = stack.Pop();
                        connectedBitmap[top.Item1 + 1, top.Item2 + 1] = false;

                        int[] dx = { 0, 0, 1, -1 };
                        int[] dy = { 1, -1, 0, 0 };
                        for (int i = 0; i < 4; ++i)
                        {
                            int nx = top.Item1 + dx[i]; int ny = top.Item2 + dy[i];
                            if (nx < 0 || nx >= size) continue;
                            if (ny < 0 || ny >= size) continue;

                            if (!visitedCell[nx, ny] && bitmap.GetBit(new Vector2(nx, ny)))
                            {
                                visitedCell[nx, ny] = true;
                                stack.Push(new Tuple<int, int>(nx, ny));
                            }
                        }
                    }
                    polygons.Add(PolygonFromBitmap(connectedBitmap));
                }
            }
        }
        return polygons;
    }

    private Vector2[][] PolygonFromBitmap(bool[,] bitmap)
    {
        int nsize = size + 2;
        List<Vector2[]> points = new List<Vector2[]>();
        bool[,] visitedCell = new bool[nsize, nsize]; // visited cells on the grid
        for (int y = 0; y < nsize; ++y) for (int x = 0; x < nsize; ++x) visitedCell[x, y] = false;

        for (int y = 0; y < nsize - 1; ++y)
        {
            for (int x = 0; x < nsize; ++x)
            {
                if (visitedCell[x, y]) continue;

                int[] dxx = { -1, 0, 1, 1, 1, 0, -1, -1 };
                int[] dyy = { -1, -1, -1, 0, 1, 1, 1, 0 };
                bool nextToCell = false;
                for (int j = 0; j < 8; ++j)
                {
                    int nx = x + dxx[j];
                    int ny = y + dyy[j];
                    if (nx < 0 || nx >= nsize) continue;
                    if (ny < 0 || ny >= nsize) continue;
                    if (bitmap[nx, ny]) nextToCell = true;
                }
                if (!nextToCell) continue;

                if (bitmap[x, y] && !bitmap[x, y + 1])
                {
                    points.Add(TraversePolygon(bitmap, visitedCell, x, y));
                }
            }
        }

        return points.ToArray();
    }

    private Vector2[] TraversePolygon(bool[,] bitmap, bool[,] visitedCell, int x, int y)
    {
        int nsize = size + 2;
        bool[,] visitedPoint = new bool[nsize + 1, nsize + 1]; // visited points on the grid, not pixels
        for (int yy = 0; yy < nsize + 1; ++yy) for (int xx = 0; xx < nsize + 1; ++xx) visitedPoint[xx, yy] = false;
        List<Vector2> points = new List<Vector2>();

        // start with bottom left corner of point
        Vector2 prev = new Vector2(x, y + 1);
        points.Add(prev);
        visitedPoint[x, y + 1] = true;
        visitedCell[x, y] = true;

        int cx = x, cy = y;
        // directions: 0 - east, 1 - north, 2 - west, 3 - south
        int dir = 0;

        // right, forward, left, uturn
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        // only for when doing a left turn
        int[] dx2 = { -1, 1, 1, -1 };
        int[] dy2 = { 1, 1, -1, -1 };

        bool done = false;
        while (!done)
        {
            for (int i = 0; i < 4; ++i)
            {
                int ndir = (dir + i) % 4;
                int nx = cx + dx[ndir];
                int ny = cy + dy[ndir];
                if (nx < 0 || nx >= nsize) continue;
                if (ny < 0 || ny >= nsize) continue;

                int[] dxx = { -1, 0, 1, 1, 1, 0, -1, -1 };
                int[] dyy = { -1, -1, -1, 0, 1, 1, 1, 0 };
                bool nextToCell = false;
                for (int j = 0; j < 8; ++j)
                {
                    int nxx = nx + dxx[j];
                    int nyy = ny + dyy[j];
                    if (nxx < 0 || nxx >= nsize) continue;
                    if (nyy < 0 || nyy >= nsize) continue;
                    if (bitmap[nxx, nyy]) nextToCell = true;
                }
                if (!nextToCell) continue;

                int npx = (int)prev.x + (i < 2 ? dx[ndir] : dx2[ndir]);
                int npy = (int)prev.y + (i < 2 ? dy[ndir] : dy2[ndir]);
                if (i <= 2) // only check if next path is valid if we are not doing a uturn
                {
                    // if (visitedPoint[npx, npy]) continue;  // check if the next point is alr visited
                    if (!bitmap[nx, ny]) continue;      // if the next cell is not a wall
                }
                if (points.Count > 1 && cx == x && cy == y) { done = true; break; }

                visitedCell[cx, cy] = true;

                switch (i)
                {
                    case 0: // right turn
                        break;
                    case 1: // forward
                        prev = new Vector2(prev.x + dx[ndir], prev.y + dy[ndir]);
                        points.Add(prev);
                        visitedPoint[(int)prev.x, (int)prev.y] = true;
                        break;
                    case 2: // left turn, will always be an outside turn
                        if (dir == 0)   // east to north
                        {
                            points.Add(new Vector2(prev.x + 1, prev.y));
                            visitedPoint[(int)prev.x + 1, (int)prev.y] = true;
                            prev = new Vector2(prev.x + 1, prev.y - 1);
                        }
                        else if (dir == 1)  // north to west
                        {
                            points.Add(new Vector2(prev.x, prev.y - 1));
                            visitedPoint[(int)prev.x, (int)prev.y - 1] = true;
                            prev = new Vector2(prev.x - 1, prev.y - 1);
                        }
                        else if (dir == 2)  // west to south
                        {
                            points.Add(new Vector2(prev.x - 1, prev.y));
                            visitedPoint[(int)prev.x - 1, (int)prev.y] = true;
                            prev = new Vector2(prev.x - 1, prev.y + 1);
                        }
                        else if (dir == 3)  // south to east
                        {
                            points.Add(new Vector2(prev.x, prev.y + 1));
                            visitedPoint[(int)prev.x, (int)prev.y + 1] = true;
                            prev = new Vector2(prev.x + 1, prev.y + 1);
                        }
                        points.Add(prev);
                        visitedPoint[(int)prev.x, (int)prev.y] = true;
                        break;
                    case 3: // uturn
                        if (dir == 0)   // east to west
                        {
                            points.Add(new Vector2(prev.x + 1, prev.y));
                            visitedPoint[(int)prev.x + 1, (int)prev.y] = true;
                            points.Add(new Vector2(prev.x + 1, prev.y - 1));
                            visitedPoint[(int)prev.x + 1, (int)prev.y - 1] = true;
                            prev = new Vector2(prev.x, prev.y - 1);
                        }
                        else if (dir == 1)  // north to south
                        {
                            points.Add(new Vector2(prev.x, prev.y - 1));
                            visitedPoint[(int)prev.x, (int)prev.y - 1] = true;
                            points.Add(new Vector2(prev.x - 1, prev.y - 1));
                            visitedPoint[(int)prev.x - 1, (int)prev.y - 1] = true;
                            prev = new Vector2(prev.x - 1, prev.y);
                        }
                        else if (dir == 2)  // west to east
                        {
                            points.Add(new Vector2(prev.x - 1, prev.y));
                            visitedPoint[(int)prev.x - 1, (int)prev.y] = true;
                            points.Add(new Vector2(prev.x - 1, prev.y + 1));
                            visitedPoint[(int)prev.x - 1, (int)prev.y + 1] = true;
                            prev = new Vector2(prev.x, prev.y + 1);
                        }
                        else if (dir == 3)  // south to north
                        {
                            points.Add(new Vector2(prev.x, prev.y + 1));
                            visitedPoint[(int)prev.x, (int)prev.y + 1] = true;
                            points.Add(new Vector2(prev.x + 1, prev.y + 1));
                            visitedPoint[(int)prev.x + 1, (int)prev.y + 1] = true;
                            prev = new Vector2(prev.x + 1, prev.y);
                        }
                        points.Add(prev);
                        visitedPoint[(int)prev.x, (int)prev.y] = true;
                        break;
                    default: break;
                }
                cx = nx;
                cy = ny;
                dir = (ndir + 3) % 4;

                if (npx == points[0].x && npy == points[0].y) done = true;

                break;
            }
        }

        // if the start and end are diagonally opposite of each other
        Vector2 last = points[points.Count - 1];
        if (last.x != points[0].x && last.y != points[0].y)
        {
            points.Add(new Vector2(x, last.y));
        }

        // if the start is on the left of end, means the starting cell is not part of the polygon
        if (last.x == x + 1 && last.y == y + 1)
        {
            points.Add(new Vector2(last.x, last.y - 1));
            points.Add(new Vector2(last.x - 1, last.y - 1));
        }

        return points.ToArray();
    }

    // For debugging purposes
    private void PrintBitmap(BitMap bitmap)
    {
        GD.Print("+-------------------+");
        for (int y = 0; y < size; ++y)
        {
            GD.PrintRaw("| ");
            for (int x = 0; x < size; ++x)
            {
                GD.PrintRaw(bitmap.GetBit(new Vector2(x, y)) ? 1 : 0);
                GD.PrintRaw(" ");
            }
            GD.Print("|");
        }
        GD.Print("+-------------------+");
    }
}

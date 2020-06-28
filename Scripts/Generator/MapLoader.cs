using System;
using System.Collections.Generic;
using Godot;

[Tool]
public class MapLoader : Spatial
{
    [Export]
    public bool ShowPreview
    {
        get
        {
            return showPreview;
        }
        set
        {
            showPreview = value;
            if (!showPreview) RemoveAllChildren();
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

    private bool showPreview;

    private int unitSize;
    private Material wallMaterial;
    private ShaderMaterial floorMaterial;
    private string mapPath;

    private int size;
    private bool ready = false;

    private List<LevelRegion> levelRegions;
    private RegionLabel[,] regionMap;

    public override void _Ready()
    {
        ready = true;
        RebuildMap();
    }

    private void RebuildMap()
    {
        if (!ready || (Engine.EditorHint && !ShowPreview)) return;
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

    private void ClearRegions()
    {
        levelRegions = new List<LevelRegion>();
        regionMap = new RegionLabel[size, size];
        for (int i = 0; i < size; ++i)
            for (int j = 0; j < size; ++j)
                regionMap[i, j] = new RegionLabel(RegionType.NONE, 0);
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
        ClearRegions();

        bool[,] floorMap = new bool[size, size];
        bool[,] doorMap = new bool[size, size];
        bool[,] wallMap = new bool[size, size];
        InitBitmap(floorMap, false);
        InitBitmap(doorMap, false);
        InitBitmap(wallMap, false);

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                int pixel = pixels[x, y];

                switch (pixels[x, y])
                {
                    case 0:
                        break;
                    case 0xffffff:
                        wallMap[x, y] = true;
                        break;
                    case 0xff0000:
                        doorMap[x, y] = true;
                        break;
                    case 0x00ff00:
                        floorMap[x, y] = true;
                        Translation = new Vector3((-x) * unitSize, -1, (-y) * unitSize);
                        break;
                    default:
                        floorMap[x, y] = true;
                        break;
                }
            }
        }

        foreach (RegionParseInfo parseInfo in ParseBitmap(wallMap))
        {
            Wall wall = new Wall(parseInfo.Polygon, unitSize, WallMaterial);
            AddChild(wall);
        }

        foreach (RegionParseInfo parseInfo in ParseBitmap(floorMap))
        {
            List<Vector2> tilesInRoom = new List<Vector2>();
            for (int y = 0; y < size; ++y)
                for (int x = 0; x < size; ++x)
                    if (parseInfo.Bitmap[x, y])
                        tilesInRoom.Add(new Vector2(x, y));
            Room room = new Room(parseInfo.Polygon, tilesInRoom.ToArray(), unitSize, FloorMaterial);
            UpdateRegionLabels(parseInfo.Bitmap, RegionType.ROOM, levelRegions.Count);
            levelRegions.Add(room);
            AddChild(room);
        }

        foreach (RegionParseInfo parseInfo in ParseBitmap(doorMap))
        {
            List<Vector2> tilesInRoom = new List<Vector2>();
            for (int y = 0; y < size; ++y)
                for (int x = 0; x < size; ++x)
                    if (parseInfo.Bitmap[x, y])
                        tilesInRoom.Add(new Vector2(x, y));
            AddChild(new Room(parseInfo.Polygon, tilesInRoom.ToArray(), unitSize, FloorMaterial));
            UpdateRegionLabels(parseInfo.Bitmap, RegionType.DOOR, levelRegions.Count);
            Door door = new Door(parseInfo.Polygon, unitSize);
            levelRegions.Add(door);
            AddChild(door);
        }

        ConnectRoomsWithDoors();
    }

    private void UpdateRegionLabels(bool[,] bitmap, RegionType regionType, int id)
    {
        for (int i = 0; i < size; ++i)
            for (int j = 0; j < size; ++j)
                if (bitmap[i, j])
                    regionMap[i, j] = new RegionLabel(regionType, id);
    }

    private List<RegionParseInfo> ParseBitmap(bool[,] bitmap)
    {
        List<RegionParseInfo> parseInfos = new List<RegionParseInfo>();
        bool[,] visitedCell = new bool[size, size]; // visited points on the grid, not pixels
        InitBitmap(visitedCell, false);

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                // group connected cells together then traverse the sides to generate a Vector2[] of polygon vertices
                if (!visitedCell[x, y] && bitmap[x, y])
                {
                    bool[,] connectedBitmap = new bool[size, size];
                    InitBitmap(connectedBitmap, false);

                    // DFS using stack to group all connected cells together
                    Stack<Tuple<int, int>> stack = new Stack<Tuple<int, int>>();
                    stack.Push(new Tuple<int, int>(x, y));
                    visitedCell[x, y] = true;
                    while (stack.Count > 0)
                    {
                        Tuple<int, int> top = stack.Pop();
                        connectedBitmap[top.Item1, top.Item2] = true;

                        int[] dx = { 0, 0, 1, -1 };
                        int[] dy = { 1, -1, 0, 0 };
                        for (int i = 0; i < 4; ++i)
                        {
                            int nx = top.Item1 + dx[i]; int ny = top.Item2 + dy[i];
                            if (nx < 0 || nx >= size) continue;
                            if (ny < 0 || ny >= size) continue;

                            if (!visitedCell[nx, ny] && bitmap[nx, ny])
                            {
                                visitedCell[nx, ny] = true;
                                stack.Push(new Tuple<int, int>(nx, ny));
                            }
                        }
                    }
                    parseInfos.Add(new RegionParseInfo(connectedBitmap, PolygonFromBitmap(connectedBitmap)));
                }
            }
        }
        return parseInfos;
    }

    private Vector2[][] PolygonFromBitmap(bool[,] bitmap)
    {
        // add one unit of padding at all four sides around the map
        int nsize = size + 2;
        bool[,] paddedBitmap = new bool[nsize, nsize];
        InitBitmap(paddedBitmap, false);
        for (int y = 0; y < size; ++y) for (int x = 0; x < size; ++x) paddedBitmap[x + 1, y + 1] = bitmap[x, y];

        List<Vector2[]> points = new List<Vector2[]>();
        bool[,] visitedCell = new bool[nsize, nsize]; // visited cells on the grid
        InitBitmap(visitedCell, false);

        for (int y = 0; y < nsize - 1; ++y)
        {
            for (int x = 0; x < nsize; ++x)
            {
                if (visitedCell[x, y]) continue;
                // want to start tracing when a cell is "off" and has an "on" cell below it
                if (!paddedBitmap[x, y] && paddedBitmap[x, y + 1])
                {
                    points.Add(TracePolygon(paddedBitmap, visitedCell, x, y));
                }
            }
        }

        return points.ToArray();
    }

    private Vector2[] TracePolygon(bool[,] paddedBitmap, bool[,] visitedCell, int startX, int startY)
    {
        int nsize = size + 2;
        bool[,] visitedPoint = new bool[nsize + 1, nsize + 1]; // visited points on the grid, not pixels
        InitBitmap(visitedPoint, false);
        List<Vector2> points = new List<Vector2>();

        // start with bottom left corner of point
        Vector2 prev = new Vector2(startX - 1, startY);
        AddPointToPolygon(prev, points, visitedPoint);
        visitedCell[startX, startY] = true;

        // directions: 0 - east, 1 - north, 2 - west, 3 - south
        int dir = 0;
        int cellX = startX, cellY = startY;

        // offsets for right, forward, left, uturn
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        // offsets for when doing a left turn
        int[] dx2 = { -1, 1, 1, -1 };
        int[] dy2 = { 1, 1, -1, -1 };

        bool done = false;
        while (!done)
        {
            for (int i = 0; i < 4; ++i)
            {
                int ndir = (dir + i) % 4;
                int nextCellX = cellX + dx[ndir];
                int nextCellY = cellY + dy[ndir];
                if (nextCellX < 0 || nextCellX >= nsize) continue;
                if (nextCellY < 0 || nextCellY >= nsize) continue;

                // only check if next path is valid if we are not doing a uturn
                // we are traversing along the edge of the polygon so the next cell should be part of the edge
                if (i <= 2 && paddedBitmap[nextCellX, nextCellY]) continue;

                switch (i)
                {
                    case 0: // right turn
                        break;
                    case 1: // forward
                        prev = new Vector2(prev.x + dx[ndir], prev.y + dy[ndir]);
                        AddPointToPolygon(prev, points, visitedPoint);
                        break;
                    case 2: // left turn, will always be an outside turn
                        if (dir == 0)   // east to north
                        {
                            AddPointToPolygon(new Vector2(prev.x + 1, prev.y), points, visitedPoint);
                            prev = new Vector2(prev.x + 1, prev.y - 1);
                        }
                        else if (dir == 1)  // north to west
                        {
                            AddPointToPolygon(new Vector2(prev.x, prev.y - 1), points, visitedPoint);
                            prev = new Vector2(prev.x - 1, prev.y - 1);
                        }
                        else if (dir == 2)  // west to south
                        {
                            AddPointToPolygon(new Vector2(prev.x - 1, prev.y), points, visitedPoint);
                            prev = new Vector2(prev.x - 1, prev.y + 1);
                        }
                        else if (dir == 3)  // south to east
                        {
                            AddPointToPolygon(new Vector2(prev.x, prev.y + 1), points, visitedPoint);
                            prev = new Vector2(prev.x + 1, prev.y + 1);
                        }
                        AddPointToPolygon(prev, points, visitedPoint);
                        break;
                    case 3: // uturn
                        if (dir == 0)   // east to west
                        {
                            AddPointToPolygon(new Vector2(prev.x + 1, prev.y), points, visitedPoint);
                            AddPointToPolygon(new Vector2(prev.x + 1, prev.y - 1), points, visitedPoint);
                            prev = new Vector2(prev.x, prev.y - 1);
                        }
                        else if (dir == 1)  // north to south
                        {
                            AddPointToPolygon(new Vector2(prev.x, prev.y - 1), points, visitedPoint);
                            AddPointToPolygon(new Vector2(prev.x - 1, prev.y - 1), points, visitedPoint);
                            prev = new Vector2(prev.x - 1, prev.y);
                        }
                        else if (dir == 2)  // west to east
                        {
                            AddPointToPolygon(new Vector2(prev.x - 1, prev.y), points, visitedPoint);
                            AddPointToPolygon(new Vector2(prev.x - 1, prev.y + 1), points, visitedPoint);
                            prev = new Vector2(prev.x, prev.y + 1);
                        }
                        else if (dir == 3)  // south to north
                        {
                            AddPointToPolygon(new Vector2(prev.x, prev.y + 1), points, visitedPoint);
                            AddPointToPolygon(new Vector2(prev.x + 1, prev.y + 1), points, visitedPoint);
                            prev = new Vector2(prev.x + 1, prev.y);
                        }
                        AddPointToPolygon(prev, points, visitedPoint);
                        break;
                    default: break;
                }

                visitedCell[cellX, cellY] = true;
                cellX = nextCellX;
                cellY = nextCellY;
                dir = (ndir + 3) % 4;

                // next point of polygon
                int nextPolygonPointX = (int)prev.x + (i < 2 ? dx[ndir] : dx2[ndir]);
                int nextPolygonPointY = (int)prev.y + (i < 2 ? dy[ndir] : dy2[ndir]);
                if (nextPolygonPointX == points[0].x && nextPolygonPointY == points[0].y) done = true;

                if (cellX == startX && cellY == startY) done = true;

                break;
            }
        }

        // If the start is on the left of end, means the starting cell is not part of the polygon
        // For example like this, + is start, * is end, then the cell itself is not part of the polygon
        // .--.
        // |  |
        // +--*
        Vector2 last = points[points.Count - 1];
        if (last.x == startX + 1 && last.y == startY + 1)
        {
            points.Add(new Vector2(last.x, last.y - 1));
            points.Add(new Vector2(last.x - 1, last.y - 1));
        }

        return points.ToArray();
    }

    private void InitBitmap(bool[,] bitmap, bool value)
    {
        for (int i = 0; i < bitmap.GetLength(0); ++i)
            for (int j = 0; j < bitmap.GetLength(1); ++j)
                bitmap[i, j] = value;
    }

    private void AddPointToPolygon(Vector2 point, List<Vector2> points, bool[,] visited)
    {
        points.Add(point);
        visited[(int)point.x, (int)point.y] = true;
    }

    private void ConnectRoomsWithDoors()
    {
        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                if (regionMap[x, y].Type != RegionType.ROOM) continue;
                int[] dx = { 0, 0, 1, -1 };
                int[] dy = { 1, -1, 0, 0 };
                for (int i = 0; i < 4; ++i)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    if (nx < 0 || nx >= size) continue;
                    if (ny < 0 || ny >= size) continue;

                    if (regionMap[nx, ny].Type == RegionType.DOOR)
                    {
                        Door door = levelRegions[regionMap[nx, ny].Id] as Door;
                        Room room = levelRegions[regionMap[x, y].Id] as Room;
                        room.ConnectDoor(door);
                        door.ConnectRoom(room);
                    }
                }
            }
        }
    }
}

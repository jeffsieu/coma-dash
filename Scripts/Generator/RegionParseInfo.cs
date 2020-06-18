using System;
using Godot;

public class RegionParseInfo
{
    public bool[,] Bitmap { get; private set; }
    public Vector2[][] Polygon { get; private set; }

    public RegionParseInfo(bool[,] bitmap, Vector2[][] polygon)
    {
        Bitmap = bitmap;
        Polygon = polygon;
    }
}

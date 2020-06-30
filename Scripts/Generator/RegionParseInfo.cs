using System;
using Godot;

public class RegionParseInfo
{
    public bool[,] Bitmap { get; private set; }
    public RegionShape RegionShape { get; private set; }

    public RegionParseInfo(bool[,] bitmap, RegionShape regionShape)
    {
        Bitmap = bitmap;
        RegionShape = regionShape;
    }
}

/// <summary>
/// MainPolygon represents the main shape, and HolePolygons are to be subtracted from
/// within the main shape.
/// </summary>
public class RegionShape
{
    public Vector2[] MainPolygon { get; private set; }
    public Vector2[][] HolePolygons { get; private set; }

    public RegionShape(Vector2[] main, Vector2[][] holes)
    {
        MainPolygon = main;
        HolePolygons = holes;
    }
}

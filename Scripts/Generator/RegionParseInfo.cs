using System;
using Godot;

public class RegionParseInfo
{
    public bool[,] Bitmap { get; private set; }
    public PolygonPoints PolygonPoints { get; private set; }

    public RegionParseInfo(bool[,] bitmap, PolygonPoints polygonPoints)
    {
        Bitmap = bitmap;
        PolygonPoints = polygonPoints;
    }
}

/// <summary>
/// MainPolygon represents the main shape, and HolePolygons are to be subtracted from
/// within the main shape.
/// </summary>
public class PolygonPoints
{
    public Vector2[] MainPolygon { get; private set; }
    public Vector2[][] HolePolygons { get; private set; }

    public PolygonPoints(Vector2[] main, Vector2[][] holes)
    {
        MainPolygon = main;
        HolePolygons = holes;
    }
}

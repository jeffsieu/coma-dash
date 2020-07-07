using System;
using Godot;

public class TransparentWall : LevelRegion
{
    private readonly float WALL_HEIGHT = 2.2f;
    private readonly float HIDE_BLEND_DURATION = 0.75f;
    private readonly float UNHIDE_BLEND_DURATION = 0.5f;
    private CSGPolygon wallMesh, floorMesh;
    private SpatialMaterial normalMaterial, transparentMaterial;
    private Tween tween;
    private bool hidden = false;
    public TransparentWall(RegionShape regionShape, int unitSize, SpatialMaterial normalMaterial, SpatialMaterial transparentMaterial)
    {
        RotationDegrees = new Vector3(90, 0, 0);
        Scale = unitSize * Vector3.One;

        this.normalMaterial = normalMaterial;
        this.transparentMaterial = transparentMaterial.Duplicate() as SpatialMaterial;
        ResetMaterialOpacity();

        tween = new Tween();
        AddChild(tween);

        CreateWallMesh(regionShape, unitSize);
        CreateFloorMesh(regionShape, unitSize);
    }

    private void CreateWallMesh(RegionShape regionShape, int unitSize)
    {
        wallMesh = new CSGPolygon
        {
            Polygon = regionShape.MainPolygon,
            MaterialOverride = transparentMaterial,
            Depth = WALL_HEIGHT
        };

        foreach (Vector2[] holePolygon in regionShape.HolePolygons)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = holePolygon,
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f * WALL_HEIGHT
            };
            wallMesh.AddChild(holeMesh);
        }

        wallMesh.UseCollision = true;
        wallMesh.CollisionLayer = ColLayer.Environment;
        wallMesh.CollisionMask = ColLayer.Environment;

        CSGPolygon shadowMesh = wallMesh.Duplicate() as CSGPolygon;
        shadowMesh.MaterialOverride = null;
        shadowMesh.CastShadow = GeometryInstance.ShadowCastingSetting.ShadowsOnly;

        AddChild(wallMesh);
        AddChild(shadowMesh);
    }

    private void CreateFloorMesh(RegionShape regionShape, int unitSize)
    {
        floorMesh = new CSGPolygon
        {
            Polygon = regionShape.MainPolygon,
            MaterialOverride = normalMaterial,
            Depth = 1
        };

        foreach (Vector2[] holePolygon in regionShape.HolePolygons)
        {
            CSGPolygon holeMesh = new CSGPolygon
            {
                Polygon = holePolygon,
                Operation = CSGShape.OperationEnum.Subtraction,
                Depth = 1.5f
            };
            floorMesh.AddChild(holeMesh);
        }

        floorMesh.Translation = new Vector3(0, 0, 1.0f);

        AddChild(floorMesh);
    }

    public void HideWall()
    {
        if (hidden) return;
        hidden = true;

        ResetMaterialOpacity();

        tween.RemoveAll();
        tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;
        tween.InterpolateProperty(transparentMaterial, "albedo_color:a", 1.0f, 0.75f, HIDE_BLEND_DURATION,
                                  Tween.TransitionType.Linear, Tween.EaseType.In);
        tween.Start();
    }

    public void UnhideWall()
    {
        if (!hidden) return;
        hidden = false;

        tween.RemoveAll();
        tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;
        tween.InterpolateProperty(transparentMaterial, "albedo_color:a", transparentMaterial.AlbedoColor.a,
                                  1.0f, UNHIDE_BLEND_DURATION, Tween.TransitionType.Linear, Tween.EaseType.In);
        tween.Start();
    }

    private void ResetMaterialOpacity()
    {
        Color color = transparentMaterial.AlbedoColor;
        color.a = 1.0f;
        transparentMaterial.AlbedoColor = color;
    }
}
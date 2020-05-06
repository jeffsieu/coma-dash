using Godot;
using System;

public class OutlineEffect : MeshInstance
{
    ShaderMaterial material;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        material = Mesh.SurfaceGetMaterial(0) as ShaderMaterial;
        GetTree().Root.Connect("size_changed", this, "ScreenResized");
        ScreenResized();
    }

    public void ScreenResized()
    {
        Vector2 size = OS.WindowSize;
        GD.Print(size);
        material.SetShaderParam("pixels_x", size.x);
        material.SetShaderParam("pixels_y", size.y);
    }

}

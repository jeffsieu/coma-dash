using Godot;
using System;

public class Floor : CSGMesh
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        KinematicBody player = GetParent().GetParent().GetNodeOrNull<KinematicBody>("Player");
        // GD.Print(player.GlobalTransform.origin - GlobalTransform.origin);
        // GD.Print(Mesh.GetAabb().Size);
        Vector3 playerPos = player.GlobalTransform.origin - GlobalTransform.origin;
        Vector3 size = Mesh.GetAabb().Size * GetParent<StaticBody>().Scale;
        Vector2 overlayPos = new Vector2(playerPos.x / size.x, playerPos.z / size.z);
        (Material as ShaderMaterial).SetShaderParam("player", overlayPos + new Vector2(0.5f, 0.5f));
        GD.Print(overlayPos);
    }
}

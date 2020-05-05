using Godot;
using System;

public class Door : Room
{
    public Door(Vector2[] polygon, int unitSize, ShaderMaterial material) : base(polygon, unitSize, material)
    {
    }
}

using Godot;
using System;
using System.Diagnostics;

public class RoomGenerator : Spatial
{
    readonly SpatialMaterial wallMaterial = new SpatialMaterial
    {
        AlbedoColor = Colors.Gray
    };
    readonly float wallHeight = 4.0f;
    readonly float wallThickness = 1.0f;

    public override void _Ready()
    {
        GenerateRoom(default, new Vector2(30, 20), Vector3.Forward);
    }

    /// <summary>
    /// Generates a rectangular room of a given size.
    /// </summary>
    /// <param name="origin">The center of the base of the room.</param>
    /// <param name="roomSize">The width and height of the room, inclusive of walls.</param>
    /// <param name="front">The front direction the room is facing</param>
    public void GenerateRoom(Vector3 origin, Vector2 roomSize, Vector3 front)
    {
        Debug.Assert(roomSize.Length() > 0, "Room size cannot be zero.");
        Debug.Assert(wallThickness * 2 <= Math.Min(roomSize.x, roomSize.y), "Walls cannot be thicker than half the width/height of the room (there must be space within the room).");
        front = front.Normalized();

        /* We wish to generate walls in this manner:
         *
         * FRONT
         * 
         * aaab
         * c  b
         * c  b
         * cddd
         * 
         * BACK
         * 
         * So we subtract the wall thickness from the length of the wall, and offset the center
         * of the wall to the side by half the wall thickness.
         */
        Vector3 frontBackWallSize = new Vector3(roomSize.x - wallThickness, wallHeight, wallThickness);
        Vector3 leftRightWallSize = new Vector3(roomSize.y - wallThickness, wallHeight, wallThickness);
        Vector3 left = Vector3.Up.Cross(front);

        // Base of the wall is origin offset in the direction of the wall (i.e. front) by a length of half the size of the room - half the wall's thickness.
        // Also, we need to offset the wall's center to the side by half the wall's thickness.
        PhysicsBody frontWall = GenerateWall(origin + ((roomSize.y - wallThickness) / 2) * front + (wallThickness / 2) * left, frontBackWallSize, -front);
        PhysicsBody backWall = GenerateWall(origin - ((roomSize.y - wallThickness) / 2) * front - (wallThickness / 2) * left, frontBackWallSize, front);
        PhysicsBody leftWall = GenerateWall(origin + ((roomSize.x - wallThickness) / 2) * left - (wallThickness / 2) * front, leftRightWallSize, -left);
        PhysicsBody rightWall = GenerateWall(origin - ((roomSize.x - wallThickness) / 2) * left + (wallThickness / 2) * front, leftRightWallSize, left);

        AddChild(frontWall);
        AddChild(backWall);
        AddChild(leftWall);
        AddChild(rightWall);
    }

    /// <summary>
    /// Returns a <c>PhysicsBody</c> representing a wall of a given size.
    /// </summary>
    /// <param name="wallBase">The center of the wall's base.</param>
    /// <param name="size">The length, height, and thickness of the wall.</param>
    /// <param name="normal">The direction the wall's front is facing</param>
    /// <returns></returns>
    public PhysicsBody GenerateWall(Vector3 wallBase, Vector3 size, Vector3 normal)
    {
        Vector3 wallUp = Vector3.Up;
        Mesh wallMesh = new CubeMesh();
        MeshInstance meshInstance = new MeshInstance
        {
            Mesh = wallMesh
        };
        meshInstance.SetSurfaceMaterial(0, wallMaterial);

        StaticBody wall = new StaticBody();
        CollisionShape collisionShape = new CollisionShape()
        {
            Shape = new BoxShape()
        };
        wall.AddChild(meshInstance);
        wall.AddChild(collisionShape);

        wall.Scale = size / 2;
        wall.Translation = wallBase + size.y / 2 * wallUp;
        wall.Rotation = new Vector3(0, 1, 0) * normal.AngleTo(Vector3.Forward);
        return wall;
    }
}

using Godot;
using System;
using System.Diagnostics;

public class RoomGenerator : Spatial
{
    [Export]
    public float WallHeight
    {
        get
        {
            return wallHeight;
        }
        set
        {
            wallHeight = value;
            GenerateRoom();
        }
    }
    [Export]
    public float WallThickness
    {
        get
        {
            return wallThickness;
        }
        set
        {
            wallThickness = value;
            GenerateRoom();
        }
    }

    [Export]
    public Vector2 TileSize
    {
        get
        {
            return tileSize;
        }
        set
        {
            tileSize = value;
            GenerateFloor();
        }
    }

    [Export]
    public float TileOffset
    {
        get
        {
            return tileOffset;
        }
        set
        {
            tileOffset = value;
            GenerateFloor();
        }
    }

    readonly SpatialMaterial wallMaterial = new SpatialMaterial
    {
        AlbedoColor = Colors.Gray
    };
    private Vector2 roomSize;
    private Vector3 origin;
    private Vector3 front;
    private Vector3 left;
    private Vector2 tileSize = new Vector2(5, 5);
    private float tileOffset = 0.0f;
    private float wallHeight = 4.0f;
    private float wallThickness = 1.0f;
    private readonly PhysicsBody[] walls = new PhysicsBody[4];
    private Spatial floor;

    public override void _Ready()
    {
        floor = new Spatial();
        roomSize = new Vector2(50, 30);
        origin = new Vector3(0, 0, 0);
        front = Vector3.Forward;
        left = Vector3.Up.Cross(front);
        AddChild(floor);
        GenerateRoom();
    }

    /// <summary>
    /// Generates a rectangular room of a given size.
    /// </summary>
    /// <param name="origin">The center of the base of the room.</param>
    /// <param name="roomSize">The width and height of the room, inclusive of walls.</param>
    /// <param name="front">The front direction the room is facing</param>
    public void GenerateRoom()
    {
        //Debug.Assert(roomSize.Length() > 0, "Room size cannot be zero.");
        //Debug.Assert(wallThickness * 2 <= Math.Min(roomSize.x, roomSize.y), "Walls cannot be thicker than half the width/height of the room (there must be space within the room).");
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

        foreach (PhysicsBody wall in walls)
        {
            if (wall != null)
                wall.QueueFree();
        }

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

        walls[0] = frontWall;
        walls[1] = backWall;
        walls[2] = leftWall;
        walls[3] = rightWall;
        GenerateFloor();
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

    public void GenerateFloor()
    {
        if (floor == null)
            return;
        Debug.Assert(tileOffset >= 0 && tileOffset <= 1);
        Debug.Assert(tileSize.Length() > 0);
        foreach (Node tile in floor.GetChildren())
        {
            if (tile != null)
                tile.QueueFree();
        }
        Vector2 roomSpaceSize = roomSize - 2 * wallThickness * Vector2.One;
        Vector3 startingCorner = origin - roomSpaceSize.x / 2 * left - roomSpaceSize.y / 2 * front;
        for (int x = 0; x < roomSpaceSize.x / tileSize.x; x++)
        {
            float rowTileOffset = (tileOffset * x) % 1;
            int y = 0;
            if (rowTileOffset > 0)
            {
                y = -1;
            }
            while (y < roomSpaceSize.y / tileSize.y)
            {
                Vector2 currentTileOffset = new Vector2(x * tileSize.x, (y + rowTileOffset) * tileSize.y);
                Vector2 currentTileSize = new Vector2(tileSize);
                if (currentTileOffset.y < 0)
                {
                    currentTileSize.y += currentTileOffset.y;
                    currentTileOffset.y = 0f;
                }
                if (currentTileOffset.y + tileSize.y > roomSize.y - wallThickness)
                {
                    float extraHeight = currentTileOffset.y + tileSize.y - (roomSize.y - wallThickness);
                    currentTileSize.y -= extraHeight;
                }
                if (currentTileOffset.x + tileSize.x > roomSize.x - wallThickness)
                {
                    float extraWidth = currentTileOffset.x + tileSize.x - (roomSize.x - wallThickness);
                    currentTileSize.x -= extraWidth;
                }

                Mesh tileMesh = new PlaneMesh()
                {
                    Size = currentTileSize
                };
                MeshInstance meshInstance = new MeshInstance()
                {
                    Mesh = tileMesh
                };

                Vector3 currentTileCorner = startingCorner + currentTileOffset.x * left + currentTileOffset.y * front + 0.01f * Vector3.Up;

                // Place tile in the center instead of the corner.
                Vector3 translation = currentTileCorner + (currentTileSize.x / 2) * left + (currentTileSize.y / 2) * front;
                meshInstance.Translation = translation;

                SpatialMaterial floorMaterial = new SpatialMaterial()
                {
                    AlbedoColor = Colors.White.LinearInterpolate(Colors.Black, (currentTileOffset.x / roomSize.x + currentTileOffset.y / roomSize.y) * 0.5f)
                };
                meshInstance.SetSurfaceMaterial(0, floorMaterial);

                floor.AddChild(meshInstance);

                y++;
            }
        }

    }
}

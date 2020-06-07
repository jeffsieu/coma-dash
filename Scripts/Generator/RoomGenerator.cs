using System;
using System.Diagnostics;
using Godot;

public class RoomGenerator : Spatial
{
    [Export]
    public Vector2 RoomSize
    {
        get
        {
            return roomSize;
        }
        set
        {
            roomSize = value;
            GenerateRoom();
        }
    }

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
    public Vector3 TileSize
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

    [Export(PropertyHint.Range, "0, 1")]
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

    [Export]
    public float TileSpacing
    {
        get
        {
            return tileSpacing;
        }
        set
        {
            tileSpacing = value;
            GenerateFloor();
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
            GenerateRoom();
        }
    }

    [Export]
    public Material TileMaterial
    {
        get
        {
            return tileMaterial;
        }
        set
        {
            tileMaterial = value;
            GenerateFloor();
        }
    }

    [Export]
    public Material GroutMaterial
    {
        get
        {
            return groutMaterial;
        }
        set
        {
            groutMaterial = value;
            GenerateGrout();
        }
    }

    public Vector2 RoomSpaceSize
    {
        get
        {
            return roomSize - 2 * wallThickness * Vector2.One;
        }
    }

    private Vector2 roomSize = new Vector2(50, 30);
    private float wallHeight = 4.0f;
    private float wallThickness = 1.0f;
    private Vector3 tileSize = new Vector3(5, 0.2f, 5);
    private float tileOffset = 0.0f;
    private float tileSpacing = 4.0f;
    private Material wallMaterial;
    private Material tileMaterial;
    private Material groutMaterial;
    private readonly PhysicsBody[] walls = new PhysicsBody[4];
    private readonly Spatial floor = new Spatial();

    public override void _Ready()
    {
        AddChild(floor);
        GenerateRoom();
    }

    /// <summary>
    /// Generates a rectangular room of a given size.
    /// </summary>
    public void GenerateRoom()
    {
        Debug.Assert(roomSize.Length() > 0, "Room size cannot be zero.");
        Debug.Assert(wallThickness * 2 <= Math.Min(roomSize.x, roomSize.y), "Walls cannot be thicker than half the width/height of the room (there must be space within the room).");

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
        PhysicsBody frontWall = GenerateWall(((roomSize.y - wallThickness) / 2) * Vector3.Forward + (wallThickness / 2) * Vector3.Left, frontBackWallSize, -Vector3.Forward);
        PhysicsBody backWall = GenerateWall(-((roomSize.y - wallThickness) / 2) * Vector3.Forward - (wallThickness / 2) * Vector3.Left, frontBackWallSize, Vector3.Forward);
        PhysicsBody leftWall = GenerateWall(((roomSize.x - wallThickness) / 2) * Vector3.Left - (wallThickness / 2) * Vector3.Forward, leftRightWallSize, -Vector3.Left);
        PhysicsBody rightWall = GenerateWall(-((roomSize.x - wallThickness) / 2) * Vector3.Left + (wallThickness / 2) * Vector3.Forward, leftRightWallSize, Vector3.Left);

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
    private PhysicsBody GenerateWall(Vector3 wallBase, Vector3 size, Vector3 normal)
    {
        Vector3 wallUp = Vector3.Up;
        Mesh wallMesh = new CubeMesh();
        MeshInstance meshInstance = new MeshInstance
        {
            Mesh = wallMesh
        };
        meshInstance.SetSurfaceMaterial(0, wallMaterial);

        StaticBody wall = new StaticBody();
        CollisionShape collisionShape = new CollisionShape
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
        Debug.Assert(tileOffset >= 0 && tileOffset <= 1);
        Debug.Assert(tileSize.Length() > 0);
        foreach (Node tile in floor.GetChildren())
        {
            if (tile != null)
                tile.QueueFree();
        }
        Vector3 startingCorner = -RoomSpaceSize.x / 2 * Vector3.Left - RoomSpaceSize.y / 2 * Vector3.Forward;
        for (int x = 0; x < RoomSpaceSize.x / (tileSize.x + tileSpacing); x++)
        {
            float rowTileOffset = (tileOffset * x) % 1;
            int y = 0;
            if (rowTileOffset > 0)
            {
                y = -1;
            }
            for (; y < RoomSpaceSize.y / (tileSize.z + tileSpacing); y++)
            {
                Vector2 currentTileOffset = new Vector2(x * (tileSize.x + tileSpacing), (y + rowTileOffset) * (tileSize.z + tileSpacing));
                Vector3 currentTileSize = new Vector3(tileSize);

                // Clip tiles protruding out the back wall
                if (currentTileOffset.y < 0)
                {
                    currentTileSize.z += currentTileOffset.y;
                    currentTileOffset.y = 0f;
                }

                // Clip tiles protruding out the front wall
                if (currentTileOffset.y + tileSize.z > roomSize.y - wallThickness)
                {
                    float extraHeight = currentTileOffset.y + tileSize.z - (roomSize.y - wallThickness);
                    currentTileSize.z -= extraHeight;
                }

                // Clip tiles protruding out the left wall (tiles are created from the right)
                if (currentTileOffset.x + tileSize.x > roomSize.x - wallThickness)
                {
                    float extraWidth = currentTileOffset.x + tileSize.x - (roomSize.x - wallThickness);
                    currentTileSize.x -= extraWidth;
                }

                Mesh tileMesh = new CubeMesh
                {
                    Size = currentTileSize
                };
                MeshInstance meshInstance = new MeshInstance
                {
                    Mesh = tileMesh
                };

                Vector3 currentTileCorner = startingCorner + currentTileOffset.x * Vector3.Left + currentTileOffset.y * Vector3.Forward + 0.01f * Vector3.Up;

                // Place tile in the center instead of the corner.
                Vector3 translation = currentTileCorner + (currentTileSize.x / 2) * Vector3.Left + (currentTileSize.z / 2) * Vector3.Forward;
                meshInstance.Translation = translation;
                meshInstance.SetSurfaceMaterial(0, tileMaterial);

                floor.AddChild(meshInstance);
            }
        }
        GenerateGrout();
    }

    public void GenerateGrout()
    {
        floor.GetNodeOrNull<MeshInstance>("Grout")?.QueueFree();
        Mesh groutMesh = new PlaneMesh
        {
            Size = RoomSpaceSize
        };
        MeshInstance meshInstance = new MeshInstance
        {
            Mesh = groutMesh
        };
        meshInstance.Name = "Grout";
        meshInstance.SetSurfaceMaterial(0, groutMaterial);
        floor.AddChild(meshInstance);
    }
}

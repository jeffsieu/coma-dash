using Godot;
using System.Collections.Generic;
using System.Linq;

public class Bookshelf : Spatial
{
    [Export(PropertyHint.Range, "1, 28")]
    public int Books
    {
        get
        {
            return books;
        }
        set
        {
            books = value;
            foreach (Node child in GetChildren())
                child.QueueFree();
            GenerateBooks();
        }
    }

    // Max columns = 14.47
    // Split each shelf into this many columns
    [Export(PropertyHint.Range, "1, 14")]
    public int Columns
    {
        get
        {
            return columns;
        }
        set
        {
            columns = value;
            if (books > columns * 2)
                books = columns * 2;
            foreach (Node child in GetChildren())
                child.QueueFree();
            GenerateBooks();
        }
    }

    [Export(PropertyHint.Range, "0, 1")]
    public float FaceOutChance
    {
        get
        {
            return faceOutChance;
        }
        set
        {
            faceOutChance = value;
            foreach (Node child in GetChildren())
                child.QueueFree();
            GenerateBooks();
        }
    }


    [Export]
    public Color[] BookColors
    {
        get
        {
            return bookColors;
        }
        set
        {
            bookColors = value;
            foreach (Node child in GetChildren())
                child.QueueFree();
            GenerateBooks();
        }
    }

    private Color[] bookColors =
    {
        new Color("#ff8260"),

        // Blue
        new Color("#60a2ff"),

        // Green
        new Color("#60ff80"),

        // Yellow
        new Color("#fff460"),
    };

    private int books = 7;
    private int columns = 14;
    private float faceOutChance = 0.1f;
    private RandomNumberGenerator rng;
    private List<Material> materials;
    private PackedScene bookScene;
    private PackedScene bookshelfScene;
    private bool ready = false;
    private Vector3 bookSize;

    // How deep the book can be placed on the bookshelf
    private float XMin => -1f + 0.25f + bookSize.x / 2;
    private const float XMax = 0.39f;

    private const float firstRowY = -2f + 0.125f + 0.771f;
    private const float secondRowY = 0.0625f + 0.771f;

    // z position of the inner edge of the right panel
    private const float ZMin = -3.75f;

    private float ColumnWidth => 3.75f * 2 / columns;

    private Material GetRandomMaterial()
    {
        return materials[rng.RandiRange(0, materials.Count - 1)];
    }

    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Randomize();
        materials = new List<Material>();
        foreach (Color color in bookColors)
        {
            materials.Add(new SpatialMaterial
            {
                AlbedoColor = color
            });
        }

        bookshelfScene = ResourceLoader.Load<PackedScene>("res://Scenes/Assets/Bookshelf.tscn");
        bookScene = ResourceLoader.Load<PackedScene>("res://Scenes/Assets/Book.tscn");

        Node book = bookScene.Instance();
        bookSize = book.GetNode<CollisionShape>("CollisionShape").Scale;
        book.QueueFree();

        ready = true;
        GenerateBooks();
    }

    private void GenerateBooks()
    {
        if (!ready)
            return;

        if (books > columns * 2)
            return;

        RigidBody bookshelf = bookshelfScene.Instance() as RigidBody;
        AddChild(bookshelf);

        Spatial bookContainer = new Spatial();
        AddChild(bookContainer);
        bool[] hasBook = new bool[columns * 2];
        float bookWidth = bookSize.z;

        // Divide bookshelf into n slots per row
        // Randomly place books into shelf slots
        for (int i = 0; i < books; i++)
            hasBook[i] = true;
        hasBook = hasBook.OrderBy(x => rng.Randf()).ToArray();

        int previousBook = -1;
        for (int slot = 0; slot < hasBook.Length; slot++)
        {
            int currentColumn = slot % columns;
            float y = (slot - columns >= 0) ? firstRowY : secondRowY;
            if (currentColumn == 0)
                previousBook = -1;

            if (!hasBook[slot])
            {
                continue;
            }

            // Current slot has a book, next slot in row is also book, don't create yet
            if (slot + 1 < hasBook.Length && hasBook[slot + 1] && currentColumn != columns - 1)
            {
                if (previousBook == -1)
                {
                    previousBook = slot;
                    continue;
                }
            }
            else // Current slot has book, next slot in row is not book
            {
                // If this is a lone book, simply create 1 book
                if (previousBook == -1)
                {
                    previousBook = slot;
                }

                // Create n books taking up x slots of space,
                // with all of them touching each other
                int numberOfBooks = slot - previousBook + 1;
                float widthOfBooks = numberOfBooks * bookWidth;
                int firstBookColumn = previousBook % columns;

                float lowestZInSlot = ZMin + firstBookColumn * ColumnWidth + bookWidth / 2;

                // Within a slot (which is bigger than a book),
                // a book can shift left and right as long as it is still contained within it.
                // For n books, we have n slots in which n books can shift about.

                // We randomize the position of the leftmost book such that this is still true.
                // If there is more than one book, align all books to the leftmost book
                float firstBookZ = rng.RandfRange(lowestZInSlot, lowestZInSlot + ColumnWidth * numberOfBooks - widthOfBooks);

                for (int i = 0; i < numberOfBooks; i++)
                {
                    float bookX = rng.RandfRange(XMin, XMax);
                    float bookY = y;
                    float bookZ = firstBookZ + i * bookWidth;
                    RigidBody book = MakeBook();
                    book.Translation = new Vector3(bookX, bookY, bookZ);
                    bookContainer.AddChild(book);
                }

                previousBook = -1;
                continue;
            }
        }
    }

    private RigidBody MakeBook()
    {
        RigidBody book = bookScene.Instance() as RigidBody;
        if (rng.Randf() > faceOutChance)
            book.RotationDegrees = new Vector3(0, 180, 0);
        MeshInstance meshInstance = book.GetNode<MeshInstance>("MeshInstance");
        meshInstance.SetSurfaceMaterial(0, GetRandomMaterial());

        return book;
    }
}

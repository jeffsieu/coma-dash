using Godot;

public class BossRoomBehavior : Node
{
    private Vector2 bossSpawnPosition;
    private readonly PackedScene bossScene = ResourceLoader.Load<PackedScene>("res://Scenes/Enemy/BigMeleeEnemy.tscn");

    public BossRoomBehavior(Vector2 position)
    {
        bossSpawnPosition = position;
    }

    public override void _Ready()
    {
        CreateBoss();
    }

    public void CreateBoss()
    {
        if (Engine.EditorHint) return;

        Enemy boss = bossScene.Instance() as Enemy;
        Level level = GetTree().Root.GetNodeOrNull<Level>("Level");
        Spatial enemyContainer = level.GetNode<Spatial>("Enemies");
        enemyContainer.AddChild(boss);

        Transform transform = boss.GlobalTransform;
        transform.origin = new Vector3(bossSpawnPosition.x, 0, bossSpawnPosition.y);

        boss.GlobalTransform = transform;
        boss.Connect("died", level, "Win");
    }
}

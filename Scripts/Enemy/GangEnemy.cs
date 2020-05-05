using Godot;

public class GangEnemy : Spatial
{
    private GangMember member1;
    private GangMember member2;
    private GangMember member3;
    private GangMember member4;

    private Vector3 origin;
    private Player player;

    public override void _Ready()
    {
        member1 = GetNode<GangMember>("Member1");
        member2 = GetNode<GangMember>("Member2");
        member3 = GetNode<GangMember>("Member3");
        member4 = GetNode<GangMember>("Member4");

        player = GetTree().Root.GetNode("Level").GetNodeOrNull<Player>("Player");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        member1.FormationPosition = GlobalTransform.origin + new Vector3(4, 0, 0);
        member2.FormationPosition = GlobalTransform.origin + new Vector3(-4, 0, 0);
        member3.FormationPosition = GlobalTransform.origin + new Vector3(0, 0, -4);
        member4.FormationPosition = GlobalTransform.origin + new Vector3(0, 0, 4);
        Translate((player.GlobalTransform.origin - GlobalTransform.origin).Normalized() * 1f * delta);
    }
}

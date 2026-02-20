using Godot;

public partial class EnemyManager : Node
{
    [Export] private TileMapLayer blockedLayer;

    public override void _Ready()
    {
        foreach (Node enemy in GetTree().GetNodesInGroup("enemy"))
        {
            var health = enemy.GetNodeOrNull<Health>("Health");
            if (health != null)
                health.Died += CheckAllEnemiesDefeated;
        }
    }

    private void CheckAllEnemiesDefeated()
    {
        CallDeferred(nameof(Check));
    }

    private void Check()
    {
        int remaining = GetTree().GetNodesInGroup("enemy").Count;
        if (remaining == 0)
        {
            blockedLayer.Visible = false;
            blockedLayer.CollisionEnabled = false;
        }
    }
}

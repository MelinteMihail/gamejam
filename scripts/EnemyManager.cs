using Godot;

public partial class EnemyManager : Node
{
    [Export] private TileMapLayer blockedLayer;
    [Export] private bool spawnPortalOnClear = false;
    [Export] private SpriteFrames portalFrames;
    [Export] private Node2D portalSpawnPoint;
    private int enemyCount = 0;

    public override void _Ready()
    {
        CallDeferred(nameof(RegisterEnemies));
    }

    private void RegisterEnemies()
    {
        var enemies = GetTree().GetNodesInGroup("enemy");
        var bosses = GetTree().GetNodesInGroup("boss");

        enemyCount = enemies.Count + bosses.Count;
        GD.Print($"EnemyManager found {enemies.Count} enemies and {bosses.Count} bosses");

        foreach (Node enemy in enemies)
        {
            var health = enemy.GetNodeOrNull<Health>("Health");
            if (health != null)
                health.Died += OnEnemyDied;
        }

        foreach (Node boss in bosses)
        {
            var health = boss.GetNodeOrNull<Health>("Health");
            if (health != null)
                health.Died += OnEnemyDied;
        }
    }

    private void OnEnemyDied()
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            if (blockedLayer != null)
            {
                blockedLayer.Visible = false;
                blockedLayer.CollisionEnabled = false;
            }

            if (spawnPortalOnClear)
                SpawnPortal();
        }
    }

    private void SpawnPortal()
    {
        var sprite = new AnimatedSprite2D();
        sprite.SpriteFrames = portalFrames;

        GetParent().AddChild(sprite);

        if (portalSpawnPoint != null && IsInstanceValid(portalSpawnPoint))
            sprite.GlobalPosition = portalSpawnPoint.GlobalPosition;

        sprite.Play("default");
    }
}
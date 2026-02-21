using Godot;
using System;

public partial class EnemyManager : Node
{
    [Export] private TileMapLayer blockedLayer;
    [Export] private bool spawnPortalOnClear = false;
    [Export] private SpriteFrames portalFrames;
    [Export] private Node2D portalSpawnPoint;
    [Export] private Area2D exitCheckpoint;
    [Export] private string NextScene = "";
    [Export] private bool isForest3 = false;

    private int enemyCount = 0;
    private AnimatedSprite2D spawnedPortal;

    public override void _Ready()
    {
        if (exitCheckpoint != null)
        {
            exitCheckpoint.Visible = false;
            exitCheckpoint.Monitoring = false;
            exitCheckpoint.BodyEntered += OnExitEntered;
        }
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

            if (exitCheckpoint != null)
            {
                exitCheckpoint.Visible = true;
                exitCheckpoint.Monitoring = true;
            }

            if (isForest3)
                CallDeferred(nameof(StartEndingSequence));
        }
    }

    private void SpawnPortal()
    {
        spawnedPortal = new AnimatedSprite2D();
        spawnedPortal.SpriteFrames = portalFrames;
        GetParent().AddChild(spawnedPortal);
        if (portalSpawnPoint != null && IsInstanceValid(portalSpawnPoint))
            spawnedPortal.GlobalPosition = portalSpawnPoint.GlobalPosition;
        spawnedPortal.Play("default");
    }

    private async void StartEndingSequence()
    {
        GD.Print("Starting ending sequence...");

        LockInput.inputLocked = true;

        var player = GetTree().GetFirstNodeInGroup("player") as Player;

        if (player != null)
        {
            player.Velocity = Vector2.Zero;
            player.SetPhysicsProcess(false);
            player.SetProcess(false);
        }

        var dialogue = GetTree().GetFirstNodeInGroup("Dialogue") as Dialogue;
        GD.Print($"Dialogue found: {dialogue != null}");
        if (dialogue != null)
        {
            dialogue.ShowDialogue(new (string, string)[]
            {
                ("Hero", "A portal..."),
                ("Hero", "It's time to go home."),
            });

            await ToSignal(dialogue, Dialogue.SignalName.DialogueClosed);
            GD.Print("Dialogue closed, walking to portal...");
        }

        if (player != null && portalSpawnPoint != null)
        {
            float speed = 80f;

            while (player.GlobalPosition.DistanceTo(portalSpawnPoint.GlobalPosition) > 12f)
            {
                Vector2 dir = (portalSpawnPoint.GlobalPosition - player.GlobalPosition).Normalized();
                player.Velocity = dir * speed;

                if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
                    player.currentPlayerDirection = dir.X > 0 ? Player.PlayerEnumDirection.Right : Player.PlayerEnumDirection.Left;
                else
                    player.currentPlayerDirection = dir.Y > 0 ? Player.PlayerEnumDirection.Down : Player.PlayerEnumDirection.Up;

                player.AnimatePlayer();
                player.MoveAndSlide();
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            player.Velocity = Vector2.Zero;
            player.AnimatePlayer();
            await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        }

        var canvasLayer = new CanvasLayer();
        GetTree().CurrentScene.AddChild(canvasLayer);

        var fadeOverlay = new ColorRect();
        fadeOverlay.Color = new Color(0, 0, 0, 0);
        fadeOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        fadeOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
        canvasLayer.AddChild(fadeOverlay);

        var tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "color:a", 1.0f, 1.5f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.In);

        await ToSignal(tween, Tween.SignalName.Finished);

        GetTree().ChangeSceneToFile("res://scenes/end_screen.tscn");
    }

    private void OnExitEntered(Node2D body)
    {
        if (!body.IsInGroup("player")) return;
        if (string.IsNullOrEmpty(NextScene)) return;

        LoadingScreen.NextScenePath = NextScene;
        CallDeferred("ChangeScene");
    }

    private void ChangeScene()
    {
        GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }
}
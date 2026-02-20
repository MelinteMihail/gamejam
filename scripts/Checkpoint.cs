using Godot;

public partial class Checkpoint : Area2D
{
    public static bool ComingFromTown = false;
    public string NextScene = "res://scenes/outside.tscn";
    private bool activated = false;

    public override void _Ready()
    {
        BodyEntered += OnPlayerEntered;
    }

    private void OnPlayerEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.SetCheckpoint(GlobalPosition);

            if (!string.IsNullOrEmpty(NextScene) && QuestChain.Instance?.CanLeaveTown() == true)
            {
                ComingFromTown = true;
                LoadingScreen.NextScenePath = NextScene;
                CallDeferred("ChangeScene");
            }
            else
            {
                activated = false;
            }
        }
    }

    private void ChangeScene()
    {
        GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }
}
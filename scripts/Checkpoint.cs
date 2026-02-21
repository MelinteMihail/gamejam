using Godot;

public partial class Checkpoint : Area2D
{
    public static bool ComingFromTown = false;

    public enum CheckpointType
    {
        Default,
        TownExit,
        ForestEntrance,
    }

    [Export]
    public CheckpointType Type = CheckpointType.Default;

    [Export]
    public string NextScene = "";

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

            switch (Type)
            {
                case CheckpointType.TownExit:
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
                    break;

                case CheckpointType.ForestEntrance:
                    if (QuestChain.Instance?.CurrentStage != QuestChain.StoryStage.GoToForest)
                        return;
                    QuestChain.Instance?.OnEnteredForest();
                    if (!string.IsNullOrEmpty(NextScene))
                    {
                        LoadingScreen.NextScenePath = NextScene;
                        CallDeferred("ChangeScene");
                    }
                    break;

                case CheckpointType.Default:
                    if (!string.IsNullOrEmpty(NextScene))
                    {
                        LoadingScreen.NextScenePath = NextScene;
                        CallDeferred("ChangeScene");
                    }
                    break;
            }
        }
    }

    private void ChangeScene()
    {
        GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }
}
using Godot;

public partial class DeathScreen : CanvasLayer
{
    [Export] private Label youDiedLabel;
    [Export] private ColorRect fadeOverlay;
    [Export] private float holdDuration = 1.5f;

    public override void _Ready()
    {
        fadeOverlay.Modulate = new Color(0, 0, 0, 1);
        youDiedLabel.Modulate = new Color(1, 1, 1, 0);

        var tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "modulate:a", 0.0f, 0.5f);        // fade in scene
        tween.TweenProperty(youDiedLabel, "modulate:a", 1.0f, 0.8f);       // YOU DIED fades in
        tween.TweenInterval(holdDuration);                                   // hold
        tween.TweenProperty(youDiedLabel, "modulate:a", 0.0f, 0.5f);       // YOU DIED fades out
        tween.TweenProperty(fadeOverlay, "modulate:a", 1.0f, 0.6f);        // fade to black
        tween.TweenCallback(Callable.From(GoToLoadingScreen));              // then switch
    }

    private void GoToLoadingScreen()
    {
        LoadingScreen.NextScenePath = "res://scenes/game.tscn";
        GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }
}
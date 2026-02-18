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
        tween.TweenProperty(fadeOverlay, "modulate:a", 0.0f, 0.5f);
        tween.TweenProperty(youDiedLabel, "modulate:a", 1.0f, 0.8f);
        tween.TweenInterval(holdDuration);
        tween.TweenProperty(youDiedLabel, "modulate:a", 0.0f, 0.5f);       
        tween.TweenProperty(fadeOverlay, "modulate:a", 1.0f, 0.6f);
        tween.TweenCallback(Callable.From(GoToLoadingScreen));
    }

    private void GoToLoadingScreen()
    {
        LoadingScreen.NextScenePath = "res://scenes/game.tscn";
        GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }
}
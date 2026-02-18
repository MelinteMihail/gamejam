using Godot;
using System;
public partial class LoadingScreen : CanvasLayer
{
    public static string NextScenePath { get; set; } = "res://scenes/player.tscn";
    [Export] private ProgressBar progressBar;
    [Export] private Label statusLabel;
    [Export] private ColorRect fadeOverlay;
    private bool loading = false;
    private bool switching = false;
    private float minTime = 1.5f;
    private float elapsed = 0.0f;
    private float displayProgress = 0f;
    public override void _Ready()
    {
        var tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "modulate:a", 0.0f, 0.4f);
        if (string.IsNullOrEmpty(NextScenePath))
        {
            GD.PrintErr("LoadingScreen: NextScenePath was not set!");
            return;
        }
        ResourceLoader.LoadThreadedRequest(NextScenePath);
        loading = true;
    }
    public override void _Process(double delta)
    {
        if (!loading) return;
        elapsed += (float)delta;
        var progress = new Godot.Collections.Array();
        var status = ResourceLoader.LoadThreadedGetStatus(NextScenePath, progress);
        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InProgress:
                float realPct = (float)progress[0] * 100f;
                displayProgress = Mathf.MoveToward(displayProgress, realPct, (float)delta * 25f);
                progressBar.Value = displayProgress;
                statusLabel.Text = $"Loading... {(int)displayProgress}%";
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                if (switching) break;
                displayProgress = Mathf.MoveToward(displayProgress, 100f, (float)delta * 50f);
                progressBar.Value = displayProgress;
                statusLabel.Text = $"Loading... {(int)displayProgress}%";
                if (displayProgress >= 100f)
                {
                    switching = true;
                    statusLabel.Text = "Loading complete!";
                    GoToSceneAfterDelay(1f);
                }
                break;
            case ResourceLoader.ThreadLoadStatus.Failed:
                statusLabel.Text = "Failed to load scene!";
                loading = false;
                break;
        }
    }
    private async void GoToSceneAfterDelay(float delay)
    {
        await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
        var tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "modulate:a", 1.0f, 0.4f);
        await ToSignal(tween, Tween.SignalName.Finished);
        var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(NextScenePath);
        GetTree().ChangeSceneToPacked(packedScene);
    }
}
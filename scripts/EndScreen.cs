using Godot;
using System.Threading.Tasks;

public partial class EndScreen : Control
{
    [Export] 
    private float initialDelay = 5.0f;
    [Export] 
    private float fadeInTime = 1.5f;
    [Export] 
    private float holdTime = 3.0f;
    [Export] 
    private float fadeOutTime = 1.0f;
    [Export] 
    private string nextScenePath = "res://scenes/main_menu.tscn";

    private Label label;

    public override void _Ready()
    {
        label = GetNode<Label>("CenterContainer/Label");

        RunSequence();
    }

    private async void RunSequence()
    {
        await ToSignal(GetTree().CreateTimer(initialDelay), SceneTreeTimer.SignalName.Timeout);

        var tween = CreateTween();
        tween.TweenProperty(label, "modulate:a", 1.0f, fadeInTime)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);
        await ToSignal(tween, Tween.SignalName.Finished);

        await ToSignal(GetTree().CreateTimer(holdTime), SceneTreeTimer.SignalName.Timeout);

        tween = CreateTween();
        tween.TweenProperty(label, "modulate:a", 0.0f, fadeOutTime)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.In);
        await ToSignal(tween, Tween.SignalName.Finished);

        GetTree().ChangeSceneToFile(nextScenePath);
    }
}
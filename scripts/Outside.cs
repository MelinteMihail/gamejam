using Godot;
using System.Threading.Tasks;

public partial class Outside : Node2D
{
    private ColorRect fadeOverlay;

    public override void _Ready()
    {
        fadeOverlay = GetNode<ColorRect>("UI/FadeOverlay");
        FadeIn();
    }

    private async void FadeIn()
    {
        var tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "color:a", 0.0f, 2.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);

        await ToSignal(tween, Tween.SignalName.Finished);

        fadeOverlay.QueueFree();
    }
}
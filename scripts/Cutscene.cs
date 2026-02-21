using Godot;
using System.Threading.Tasks;

public partial class Cutscene : Node2D
{
    [Export] private Node2D leftTarget;
    [Export] private Node2D upTarget;
    [Export] private float walkSpeed = 80f;
    [Export] private string outsideScenePath = "res://scenes/outside.tscn";
    [Export] private AudioStreamPlayer2D ambientSound;

    private Player player;
    private AnimatedSprite2D otherSprite;
    private ColorRect fadeOverlay;
    private CanvasLayer canvasLayer;

    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        otherSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Hide lantern pivot
        if (player.LanternPivot != null)
            player.LanternPivot.Visible = false;

        // Disable lantern light directly on player
        var lanternLight = player.GetNodeOrNull<Light2D>("LanternLight");
        if (lanternLight != null)
            lanternLight.Enabled = false;

        // Stop player from overriding animations
        player.SetProcess(false);
        player.SetPhysicsProcess(false);

        // Create CanvasLayer so fade renders on top of everything
        canvasLayer = new CanvasLayer();
        AddChild(canvasLayer);

        fadeOverlay = new ColorRect();
        fadeOverlay.Color = new Color(0, 0, 0, 1);
        fadeOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        fadeOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
        canvasLayer.AddChild(fadeOverlay);

        LockInput.inputLocked = true;

        // Play simple idle animation immediately
        otherSprite.Play("wardrobe_simple");

        player.Sprite.Play("cutscene_idle_left");
        StartCutscene();
    }

    private async void StartCutscene()
    {
        // Fade in from black
        var tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "color:a", 0.0f, 2.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);
        await ToSignal(tween, Tween.SignalName.Finished);

        // Play ambient sound after fade in
        ambientSound?.Play();

        // Idle left briefly
        player.Sprite.Play("cutscene_idle_left");
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        // Dialogue before moving
        var dialogue = GetTree().GetFirstNodeInGroup("Dialogue") as Dialogue;
        if (dialogue != null)
        {
            dialogue.ShowDialogue(new (string, string)[]
            {
                ("Hero", "What's that rustling noise?"),
                ("Hero", "Let me go check."),
            });
            await ToSignal(dialogue, Dialogue.SignalName.DialogueClosed);
        }

        // Stop sound after dialogue ends
        ambientSound?.Stop();

        // Walk left
        player.Sprite.Play("cutscene_walk_left");
        await MoveTo(leftTarget.GlobalPosition);

        // Walk up
        player.Sprite.Play("cutscene_walk_up");
        await MoveTo(upTarget.GlobalPosition);

        // Show only frame 0 of idle_up
        player.Sprite.Play("cutscene_idle_up");
        player.Sprite.Pause();
        player.Sprite.Frame = 0;
        await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);

        // Start wardrobe animation and delete player at the same time
        otherSprite.Play("wardrobe");
        player.QueueFree();

        await ToSignal(otherSprite, AnimatedSprite2D.SignalName.AnimationFinished);

        // Fade out to black
        tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "color:a", 1.0f, 2.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.In);
        await ToSignal(tween, Tween.SignalName.Finished);

        LockInput.inputLocked = false;
        GetTree().ChangeSceneToFile(outsideScenePath);
    }

    private async Task MoveTo(Vector2 target)
    {
        while (player != null && player.GlobalPosition.DistanceTo(target) > 4f)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            float delta = (float)GetProcessDeltaTime();
            Vector2 direction = (target - player.GlobalPosition).Normalized();
            player.GlobalPosition += direction * walkSpeed * delta;
        }
    }
}
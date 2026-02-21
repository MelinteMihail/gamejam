using Godot;

public partial class LanternPickup : Area2D
{
    [Export] private AnimatedSprite2D sprite;
    private Player player = null;
    private Label interactableText;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        var lanternState = GetNodeOrNull<LanternState>("/root/LanternState");
        if (lanternState != null && lanternState.HasLantern)
        {
            QueueFree();
            return;
        }

        sprite.Play("lantern_flame");
        interactableText = GetNode<Label>("Label");
        interactableText.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (player != null && Input.IsActionJustPressed("interact"))
        {
            var lanternState = GetNode<LanternState>("/root/LanternState");
            lanternState.HasLantern = true;
            player.lampLight.Energy = 2.5f;
            player.EnableLantern();
            QuestChain.Instance?.OnLanternPickedUp();
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            player = body as Player;
            if (interactableText != null)
                interactableText.Visible = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            player = null;
            if (interactableText != null)
                interactableText.Visible = false;
        }
    }
}
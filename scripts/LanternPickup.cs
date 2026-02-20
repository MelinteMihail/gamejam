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
        sprite.Play("lantern_flame");
        interactableText = GetNode<Label>("Label");
        interactableText.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (player != null && Input.IsActionJustPressed("interact"))
        {
            var gameState = GetNode<LanternState>("/root/LanternState");
            gameState.HasLantern = true;
            player.EnableLantern();
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            player = body as Player;
            interactableText.Visible = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            player = null;
            interactableText.Visible = false;
        }
    }
}

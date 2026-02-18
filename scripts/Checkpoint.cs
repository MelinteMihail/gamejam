using Godot;

public partial class Checkpoint : Area2D
{
    private bool activated = false;

    public override void _Ready()
    {
        BodyEntered += OnPlayerEntered;
    }

    private void OnPlayerEntered(Node2D body)
    {
        if (activated) return;

        if (body is Player player)
        {
            player.SetCheckpoint(GlobalPosition);
            activated = true;

            GD.Print("Set checkpoint at: " + GlobalPosition);
        }
    }
}

using Godot;

public partial class Guard : Node2D
{
    [Export] 
    private Label interactLabel;

    private GuardState guardState;
    private Dialogue dialogue;

    private string NextScenePath = "res://scenes/town.tscn";
    private bool shouldTransitionAfterDialogue = false;
    private bool playerNearby = false;

    private (string speaker, string text)[] firstTimeDialogue = new[]
    {
        ("Guard", "Halt! Who goes there?"),
        ("Guard", "I've never seen you around here before."),
        ("Guard", "Pass through, but stay out of trouble.")
    };

    private (string speaker, string text)[] repeatDialogue = new[]
    {
        ("Guard", "Move along."),
    };

    public override void _Ready()
    {
        SetProcessInput(true);

        guardState = GetNode<GuardState>("/root/GuardState");
        dialogue = GetTree().GetFirstNodeInGroup("Dialogue") as Dialogue;

        var area = GetNode<Area2D>("Interactable Area");
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;

        dialogue.DialogueClosed += OnDialogueClosed;

        interactLabel.Text = "Press [E] to interact";
        interactLabel.Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("interact"))
            GD.Print($"Interact pressed. playerNearby={playerNearby}, dialogueVisible={dialogue.Visible}");

        if (playerNearby && Input.IsActionJustPressed("interact") && !dialogue.Visible)
        {
            GD.Print("Opening dialogue...");
            if (!guardState.HasSpokenToGuard)
            {
                dialogue.ShowDialogue(firstTimeDialogue);
                guardState.HasSpokenToGuard = true;
                shouldTransitionAfterDialogue = true;
            }
            else
            {
                dialogue.ShowDialogue(repeatDialogue);
                shouldTransitionAfterDialogue = true;
            }
            GetViewport()?.SetInputAsHandled(); // consume the input so Dialogue._Input doesn't also fire
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            playerNearby = true;
            interactLabel.Show();
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            playerNearby = false;
            interactLabel.Hide();
        }
    }

    private void OnDialogueClosed()
    {
        if (shouldTransitionAfterDialogue)
        {
            LoadingScreen.NextScenePath = NextScenePath;
            GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
        }
    }
}
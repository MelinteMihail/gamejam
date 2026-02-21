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
        ("Hero", "Just - wait! I don’t even know where I am!"),
        ("Guard", "So it’s true. You’re the destined hero."),
        ("Hero", "What do you mean?"),
        ("Guard", "Pass through, Arthur will tell you more. ")
    };

    private (string speaker, string text)[] repeatDialogue = new[]
    {
        ("Guard", "Move along."),
    };

    private bool CanInteract()
    {
        var lanternState = GetNodeOrNull<LanternState>("/root/LanternState");
        bool hasLantern = lanternState != null && lanternState.HasLantern;
        if (!hasLantern) return false;

        var stage = QuestChain.Instance?.CurrentStage;

        if (!guardState.HasSpokenToGuard)
            return stage == QuestChain.StoryStage.GoToTown;

        return stage != QuestChain.StoryStage.PickupLantern;
    }

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
        if (playerNearby && Input.IsActionJustPressed("interact") && !dialogue.Visible)
        {
            if (!CanInteract()) return;

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
            GetViewport()?.SetInputAsHandled();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            playerNearby = true;
            if (CanInteract())
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
            QuestChain.Instance?.OnEnteredTown();
            LoadingScreen.NextScenePath = NextScenePath;
            GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
        }
    }
}
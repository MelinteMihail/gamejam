using Godot;

public partial class IntroDialogue : Node
{
    private bool triggered = false;

    private (string speaker, string text)[] introLines = new[]
    {
        ("Hero", "Huh... My head hurts.. Where am I?"),
        ("Hero", "There's a town in the distance."),
        ("Hero", "I should go there."),
    };

    public override void _Ready()
    {
        var shown = GetNodeOrNull<Node>("/root/IntroShown");
        if (shown != null) return;

        CallDeferred(nameof(ShowIntro));
    }

    private void ShowIntro()
    {
        var dialogue = GetTree().GetFirstNodeInGroup("Dialogue") as Dialogue;
        if (dialogue == null) return;

        var marker = new Node();
        marker.Name = "IntroShown";
        GetTree().Root.AddChild(marker);

        dialogue.ShowDialogue(introLines);
    }
}
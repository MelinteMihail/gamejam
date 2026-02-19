using Godot;
using System;
public partial class QuestCollectible : Area2D
{
    [Export]
    public string QuestItemType = "herbs";

    private bool hasBeenCollected = false;
    private bool playerInRange = false;
    private Label hint;

    public override void _Ready()
    {
        BodyEntered += OnPlayerEntered;
        BodyExited += OnPlayerExited;

        hint = new Label();
        hint.Text = "[E] to collect";
        hint.AddThemeFontSizeOverride("font_size", 8);
        hint.Position = new Vector2(-30, -30);
        hint.Visible = false;
        AddChild(hint);
    }

    public override void _Process(double delta)
    {
        if (playerInRange && !hasBeenCollected && Input.IsActionJustPressed("interact"))
            CollectItem();
    }

    private void OnPlayerEntered(Node2D body)
    {
        if (body.Name == "Player" || body.GetType().Name == "Player")
        {
            playerInRange = true;
            hint.Visible = true;
        }
    }

    private void OnPlayerExited(Node2D body)
    {
        if (body.Name == "Player" || body.GetType().Name == "Player")
        {
            playerInRange = false;
            hint.Visible = false;
        }
    }

    private void CollectItem()
    {
        if (QuestManager.Instance == null) return;
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        bool anyQuestMatched = false;
        foreach (var quest in activeQuests)
        {
            if (quest.ProgressType == QuestItemType && !quest.IsCompleted())
            {
                quest.AddProgress(1);
                anyQuestMatched = true;
                GD.Print($"Quest progress: {quest.GetProgressText()}");
                if (quest.IsCompleted())
                    GD.Print($"Quest completed: {quest.QuestTitle}");
            }
        }
        if (anyQuestMatched)
        {
            hasBeenCollected = true;
            QueueFree();
        }
    }
}
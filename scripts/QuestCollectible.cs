using Godot;
using System;

public partial class QuestCollectible : Area2D
{
    [Export]
    public string QuestItemType = "herbs";

    private bool hasBeenCollected = false;
    private bool playerInRange = false;

    public override void _Ready()
    {
        BodyEntered += OnPlayerEntered;
        BodyExited += OnPlayerExited;
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
        }
    }
    
    private void OnPlayerExited(Node2D body)
    {
        if (body.Name == "Player" || body.GetType().Name == "Player")
            playerInRange = false;
    }

    private void CollectItem()
    {
        if (QuestManager.Instance == null) return;

        var activeQuests = QuestManager.Instance.GetActiveQuests();
        bool hasMatchingQuest = false;

        foreach (var quest in activeQuests)
        {
            if (quest.ProgressType == QuestItemType && !quest.IsCompleted())
            {
                hasMatchingQuest = true;
                quest.AddProgress(1);
                GD.Print($"Quest progress: {quest.GetProgressText()}");
                if (quest.IsCompleted())
                    GD.Print($"Quest completed: {quest.QuestTitle}");
            }
        }

        if (hasMatchingQuest)
            QueueFree();
        else
            GD.Print("No active quest for this item.");
    }
}
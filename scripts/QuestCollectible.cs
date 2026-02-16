using Godot;
using System;

public partial class QuestCollectible : Area2D
{
    [Export]
    public string QuestItemType = "herbs";

    private bool hasBeenCollected = false;

    public override void _Ready()
    {
        BodyEntered += OnPlayerEntered;
    }

    private void OnPlayerEntered(Node2D body)
    {
        if (hasBeenCollected)
            return;

        if (body.Name == "Player" || body.GetType().Name == "Player")
        {
            hasBeenCollected = true;

            if (QuestManager.Instance != null)
            {
                var activeQuests = QuestManager.Instance.GetActiveQuests();

                foreach (var quest in activeQuests)
                {
                    if (quest.ProgressType == QuestItemType && !quest.IsCompleted())
                    {
                        quest.AddProgress(1);
                        GD.Print($"Quest progress: {quest.GetProgressText()}");

                        if (quest.IsCompleted())
                        {
                            GD.Print($"Quest completed: {quest.QuestTitle}");
                            QuestManager.Instance.CompleteQuest(quest);
                        }
                    }
                }
            }

            QueueFree();
        }
    }
}
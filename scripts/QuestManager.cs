using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class QuestManager : Node
{
    public static QuestManager Instance { get; private set; }

    [Signal]
    public delegate void QuestAddedEventHandler(QuestData questData);

    [Signal]
    public delegate void QuestCompletedEventHandler(QuestData questData);

    private List<QuestData> activeQuests = new List<QuestData>();
    private List<string> completedQuestIds = new List<string>();

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }

    public void AddQuest(QuestData questData)
    {
        if (questData == null || string.IsNullOrEmpty(questData.QuestId))
        {
            GD.PrintErr("QuestManager: Cannot add quest - QuestData is null or QuestId is empty");
            return;
        }

        if (completedQuestIds.Contains(questData.QuestId))
        {
            GD.Print($"Quest already completed: {questData.QuestTitle} (ID: {questData.QuestId})");
            return;
        }

        bool alreadyExists = activeQuests.Any(q => q.QuestId == questData.QuestId);

        if (!alreadyExists)
        {
            activeQuests.Add(questData);
            EmitSignal(SignalName.QuestAdded, questData);
            GD.Print($"Quest added: {questData.QuestTitle} (ID: {questData.QuestId})");
        }
        else
        {
            GD.Print($"Quest already active: {questData.QuestTitle} (ID: {questData.QuestId})");
        }
    }

    public void CompleteQuest(QuestData questData)
    {
        if (questData == null)
            return;

        var questToRemove = activeQuests.FirstOrDefault(q => q.QuestId == questData.QuestId);

        if (questToRemove != null)
        {
            activeQuests.Remove(questToRemove);
            completedQuestIds.Add(questData.QuestId); // Mark as completed
            EmitSignal(SignalName.QuestCompleted, questToRemove);
            GD.Print($"Quest completed: {questToRemove.QuestTitle} (ID: {questToRemove.QuestId})");
        }
    }

    public List<QuestData> GetActiveQuests()
    {
        return new List<QuestData>(activeQuests);
    }

    public bool HasQuest(string questId)
    {
        return activeQuests.Any(q => q.QuestId == questId);
    }

    public bool HasQuest(QuestData questData)
    {
        if (questData == null)
            return false;
        return activeQuests.Any(q => q.QuestId == questData.QuestId);
    }

    public bool IsQuestCompleted(string questId)
    {
        return completedQuestIds.Contains(questId);
    }

    public int GetQuestCount()
    {
        return activeQuests.Count;
    }

    public QuestData GetQuestById(string questId)
    {
        return activeQuests.FirstOrDefault(q => q.QuestId == questId);
    }
}
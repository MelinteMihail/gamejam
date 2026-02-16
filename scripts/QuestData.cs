using Godot;
using System;

[GlobalClass]
public partial class QuestData : Resource
{
    [Export]
    public string QuestId = "";

    [Export]
    public string QuestTitle = "Default Quest Title";

    [Export]
    public string[] DialogPages = new string[]
    {
        "Hello, adventurer! I have a task for you.",
        "I need you to collect 10 herbs from the forest.",
    };

    [Export]
    public string AcceptedMessage = "Quest already accepted.";

    [Export]
    public string CompletionMessage = "Thank you for completing my quest!";

    [Export]
    public string QuestDescription = "Collect herbs";

    [Export]
    public int TargetProgress = 10;

    [Export]
    public string ProgressType = "herbs";

    public int CurrentProgress = 0;

    public QuestData Duplicate()
    {
        QuestData copy = new QuestData();
        copy.QuestId = this.QuestId;
        copy.QuestTitle = this.QuestTitle;
        copy.DialogPages = (string[])this.DialogPages.Clone();
        copy.AcceptedMessage = this.AcceptedMessage;
        copy.CompletionMessage = this.CompletionMessage;
        copy.QuestDescription = this.QuestDescription;
        copy.TargetProgress = this.TargetProgress;
        copy.ProgressType = this.ProgressType;
        copy.CurrentProgress = 0;
        return copy;
    }

    public string GetProgressText()
    {
        return $"{CurrentProgress}/{TargetProgress} {ProgressType}";
    }

    public bool IsCompleted()
    {
        return CurrentProgress >= TargetProgress;
    }

    public void AddProgress(int amount = 1)
    {
        CurrentProgress += amount;
        if (CurrentProgress > TargetProgress)
        {
            CurrentProgress = TargetProgress;
        }
    }
}
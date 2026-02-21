using Godot;

public partial class QuestChain : Node
{
    public static QuestChain Instance { get; private set; }

    public enum StoryStage
    {
        PickupLantern,
        GoToTown,
        TalkToCivilians,
        DoQuests,
        BuyArmor,
        GoToForest,
        Done
    }

    public StoryStage CurrentStage = StoryStage.PickupLantern;
    public int CiviliansSpokenTo = 0;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else QueueFree();
    }

    public void Reset()
    {
        CurrentStage = StoryStage.PickupLantern;
        CiviliansSpokenTo = 0;
        GD.Print("QuestChain reset");
    }

    public void OnLanternPickedUp()
    {
        if (CurrentStage != StoryStage.PickupLantern) return;
        CurrentStage = StoryStage.GoToTown;
        GD.Print("Stage: GoToTown");
    }

    public void OnEnteredTown()
    {
        if (CurrentStage != StoryStage.GoToTown) return;
        CurrentStage = StoryStage.TalkToCivilians;
        CiviliansSpokenTo = 0;
        GD.Print("Stage: TalkToCivilians");
    }

    public void OnCivilianSpokenTo()
    {
        if (CurrentStage != StoryStage.TalkToCivilians) return;
        CiviliansSpokenTo++;
        GD.Print($"Civilians spoken to: {CiviliansSpokenTo}/3");
        if (CiviliansSpokenTo >= 3)
        {
            CurrentStage = StoryStage.DoQuests;
            GD.Print("Stage: DoQuests");
        }
    }

    public void OnQuestsTurnedIn()
    {
        if (CurrentStage != StoryStage.DoQuests) return;
        bool herbsDone = QuestManager.Instance.IsQuestCompleted("herbs");
        bool mobsDone = QuestManager.Instance.IsQuestCompleted("mobs");
        if (herbsDone && mobsDone)
        {
            CurrentStage = StoryStage.BuyArmor;
            GD.Print("Stage: BuyArmor");
        }
    }

    public void OnArmorBought()
    {
        if (CurrentStage != StoryStage.BuyArmor) return;
        CurrentStage = StoryStage.GoToForest;
        GD.Print("Stage: GoToForest");
    }

    public void OnEnteredForest()
    {
        if (CurrentStage != StoryStage.GoToForest) return;
        CurrentStage = StoryStage.Done;
        GD.Print("Stage: Done");
    }

    public bool CanLeaveTown()
    {
        return CurrentStage == StoryStage.DoQuests ||
               CurrentStage == StoryStage.BuyArmor ||
               CurrentStage == StoryStage.GoToForest ||
               CurrentStage == StoryStage.Done;
    }

    public bool CanEnterTown()
    {
        return CurrentStage == StoryStage.GoToTown ||
               CurrentStage == StoryStage.TalkToCivilians ||
               CurrentStage == StoryStage.DoQuests ||
               CurrentStage == StoryStage.BuyArmor ||
               CurrentStage == StoryStage.GoToForest ||
               CurrentStage == StoryStage.Done;
    }
}
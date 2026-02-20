using Godot;
using System;

public partial class Quest : Node2D
{
    [Export]
    public QuestData QuestDataTemplate;

    private QuestData questInstance;
    private QuestDialog questDialog;
    private Node2D player;
    private int currentPage = 0;
    private bool questAccepted = false;
    private bool isDialogActive = false;

    public override void _Ready()
    {
        GD.Print($"Quest _Ready for NPC: {GetParent().Name}");

        player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (player == null)
            GD.PrintErr("Quest: Could not find Player in group 'player'");

        questDialog = GetTree().GetFirstNodeInGroup("QuestDialog") as QuestDialog;
        if (questDialog == null)
            GD.PrintErr("Quest: Could not find QuestDialog in group 'QuestDialog'");
        else
            GD.Print("Quest: Found QuestDialog successfully");

        if (QuestDataTemplate != null)
        {
            questInstance = QuestDataTemplate.Duplicate();
            questInstance.NpcName = GetParent().Name;
            GD.Print($"Quest: Created instance with ID: {questInstance.QuestId}, NPC: {questInstance.NpcName}");
        }
        else
        {
            GD.PrintErr("Quest: QuestDataTemplate is null! Assign a QuestData resource in the inspector.");
        }
    }

    public void StartQuest()
    {
        LockInput.inputLocked = true;

        GD.Print($"StartQuest called for: {questInstance?.QuestId}");

        if (questDialog == null || questInstance == null)
        {
            GD.PrintErr("Quest: Cannot start quest - missing QuestDialog or QuestDataTemplate");
            return;
        }

        if (QuestManager.Instance != null)
        {
            if (QuestManager.Instance.IsQuestCompleted(questInstance.QuestId))
            {
                GD.Print("Quest already completed");
                questDialog.ShowDialog(questInstance.QuestTitle, questInstance.CompletionMessage, new string[] { "Leave" });
                ConnectToDialog();
                return;
            }
            var activeQuest = QuestManager.Instance.GetQuestById(questInstance.QuestId);
            if (activeQuest != null && activeQuest.IsCompleted())
            {
                GD.Print("Quest ready to turn in!");
                questDialog.ShowDialog(questInstance.QuestTitle, "You've completed my quest! Thank you!", new string[] { "Turn In" });
                ConnectToDialog();
                return;
            }
            else if (QuestManager.Instance.HasQuest(questInstance.QuestId))
            {
                GD.Print("Quest already active but not complete");
                questDialog.ShowDialog(questInstance.QuestTitle, "You've already accepted this quest. Check your quest log!", new string[] { "Leave" });
                ConnectToDialog();
                return;
            }
        }

        if (questAccepted)
        {
            GD.Print("Quest locally accepted");
            questDialog.ShowDialog(questInstance.QuestTitle, questInstance.AcceptedMessage, new string[] { "Leave" });
            ConnectToDialog();
            return;
        }

        GD.Print("Starting new quest dialog");
        currentPage = 0;
        ConnectToDialog();
        ShowCurrentPage();
    }

    private void ConnectToDialog()
    {
        if (questDialog != null && !isDialogActive)
        {
            GD.Print($"Connecting to dialog for: {questInstance.QuestId}");
            isDialogActive = true;
            questDialog.OptionSelected += OnOptionSelected;
            questDialog.DialogClosed += OnDialogClosed;
        }
    }

    private void DisconnectFromDialog()
    {
        if (questDialog != null && isDialogActive)
        {
            GD.Print($"Disconnecting from dialog for: {questInstance.QuestId}");
            isDialogActive = false;
            questDialog.OptionSelected -= OnOptionSelected;
            questDialog.DialogClosed -= OnDialogClosed;
        }
    }

    private void OnDialogClosed()
    {
        GD.Print($"OnDialogClosed called for: {questInstance.QuestId}");
        DisconnectFromDialog();
    }

    private void ShowCurrentPage()
    {
        if (questDialog == null || questInstance == null)
            return;

        GD.Print($"Showing page {currentPage}");

        if (currentPage < questInstance.DialogPages.Length - 1)
        {
            questDialog.ShowDialog(questInstance.QuestTitle, questInstance.DialogPages[currentPage], new string[] { "Continue" });
        }
        else
        {
            questDialog.ShowDialog(questInstance.QuestTitle, questInstance.DialogPages[currentPage], new string[] { "Accept", "Decline" });
        }
    }

    private void OnOptionSelected(int optionIndex)
    {
        GD.Print($"OnOptionSelected called: index={optionIndex}, isDialogActive={isDialogActive}, questId={questInstance?.QuestId}");

        if (!isDialogActive || questInstance == null)
        {
            GD.Print("Ignoring option - dialog not active or quest null");
            return;
        }

        if (QuestManager.Instance != null)
        {
            var activeQuest = QuestManager.Instance.GetQuestById(questInstance.QuestId);
            if (activeQuest != null && activeQuest.IsCompleted() && optionIndex == 0)
            {
                GD.Print("Turning in completed quest!");
                OnQuestTurnedIn();
                return;
            }
        }

        if (currentPage < questInstance.DialogPages.Length - 1)
        {
            GD.Print("Moving to next page");
            currentPage++;
            ShowCurrentPage();
        }
        else
        {
            if (optionIndex == 0)
            {
                GD.Print("Quest accepted!");
                questAccepted = true;
                LockInput.inputLocked = false;
                OnQuestAccepted();
            }
            else
            {
                GD.Print("Quest declined!");
                questAccepted = false;
                LockInput.inputLocked = false;
                OnQuestDeclined();
            }
        }
    }

    private void OnQuestAccepted()
    {
        GD.Print($"OnQuestAccepted called for: {questInstance.QuestId}");
        if (QuestManager.Instance != null && questInstance != null)
        {
            QuestManager.Instance.AddQuest(questInstance);
            QuestChain.Instance?.OnCivilianSpokenTo();
            GD.Print("Quest added to manager");
        }
        else
        {
            GD.PrintErr("Quest: Cannot accept quest - QuestManager not found or questInstance is null");
        }
    }

    private void OnQuestDeclined()
    {
        GD.Print("Quest declined");
    }

    private void OnQuestTurnedIn()
    {
        if (QuestManager.Instance != null && questInstance != null)
        {
            if (Coin.Instance != null)
                Coin.Instance.AddCoins(questInstance.QuestReward);

            GD.Print($"Quest turned in: {questInstance.QuestTitle}");

            QuestManager.Instance.CompleteQuest(questInstance);
            QuestChain.Instance?.OnQuestsTurnedIn();
            questAccepted = false;
            LockInput.inputLocked = false;
        }
    }

    public void UpdateProgress(int amount = 1)
    {
        if (questAccepted && questInstance != null)
        {
            questInstance.AddProgress(amount);

            if (questInstance.IsCompleted())
            {
                GD.Print($"Quest objective complete! Return to {questInstance.NpcName}");
            }
        }
    }

    private void OnQuestCompleted()
    {
        if (QuestManager.Instance != null && questInstance != null)
        {
            QuestManager.Instance.CompleteQuest(questInstance);
            questAccepted = false;
        }
    }

    public QuestData GetQuestInstance()
    {
        return questInstance;
    }
}
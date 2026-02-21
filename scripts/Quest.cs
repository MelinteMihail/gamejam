using Godot;
using System;

public partial class Quest : Node2D
{
    [Export]
    public QuestData QuestDataTemplate;

    private QuestData questInstance;
    private QuestDialog questDialog;
    private Dialogue dialogue;
    private Node2D player;
    private int currentPage = 0;
    private bool questAccepted = false;
    private bool isDialogActive = false;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        questDialog = GetTree().GetFirstNodeInGroup("QuestDialog") as QuestDialog;
        dialogue = GetTree().GetFirstNodeInGroup("Dialogue") as Dialogue;

        if (QuestDataTemplate != null)
        {
            questInstance = QuestDataTemplate.Duplicate();
            questInstance.NpcName = GetParent().Name;
        }
        else
        {
            GD.PrintErr("Quest: QuestDataTemplate is null!");
        }
    }

    public void StartQuest()
    {
        if (dialogue != null && dialogue.Visible)
            return;
        if (isDialogActive)
            return;

        LockInput.inputLocked = true;

        if (questDialog == null || questInstance == null)
        {
            GD.PrintErr("Quest: Cannot start quest - missing QuestDialog or QuestInstance");
            return;
        }

        if (QuestManager.Instance != null)
        {
            if (QuestManager.Instance.IsQuestCompleted(questInstance.QuestId))
            {
                questDialog.ShowDialog(questInstance.QuestTitle, questInstance.CompletionMessage, new string[] { "Leave" });
                isDialogActive = true;
                questDialog.OptionSelected += OnCompletedLeave;
                questDialog.DialogClosed += OnCompletedDialogClosed;
                return;
            }

            var activeQuest = QuestManager.Instance.GetQuestById(questInstance.QuestId);
            if (activeQuest != null && activeQuest.IsCompleted())
            {
                questDialog.ShowDialog(questInstance.QuestTitle, "You've completed my quest! Thank you!", new string[] { "Turn In" });
                ConnectToDialog();
                return;
            }

            if (QuestManager.Instance.HasQuest(questInstance.QuestId))
            {
                questDialog.ShowDialog(questInstance.QuestTitle, questInstance.AcceptedMessage, new string[] { "Leave" });
                ConnectToDialog();
                return;
            }
        }

        currentPage = 0;
        ConnectToDialog();
        ShowCurrentPage();
    }

    private void OnCompletedLeave(int optionIndex)
    {
        questDialog.OptionSelected -= OnCompletedLeave;
        questDialog.DialogClosed -= OnCompletedDialogClosed;
        isDialogActive = false;
        LockInput.inputLocked = false;
    }

    private void OnCompletedDialogClosed()
    {
        questDialog.OptionSelected -= OnCompletedLeave;
        questDialog.DialogClosed -= OnCompletedDialogClosed;
        isDialogActive = false;
        LockInput.inputLocked = false;
    }

    private void ConnectToDialog()
    {
        if (questDialog != null && !isDialogActive)
        {
            isDialogActive = true;
            questDialog.OptionSelected += OnOptionSelected;
            questDialog.DialogClosed += OnDialogClosed;
        }
    }

    private void DisconnectFromDialog()
    {
        if (questDialog != null && isDialogActive)
        {
            isDialogActive = false;
            questDialog.OptionSelected -= OnOptionSelected;
            questDialog.DialogClosed -= OnDialogClosed;
        }
    }

    private void OnDialogClosed()
    {
        DisconnectFromDialog();
        LockInput.inputLocked = false;
    }

    private void ShowCurrentPage()
    {
        if (questDialog == null || questInstance == null)
            return;

        if (currentPage < questInstance.DialogPages.Length - 1)
            questDialog.ShowDialog(questInstance.QuestTitle, questInstance.DialogPages[currentPage], new string[] { "Continue" });
        else
            questDialog.ShowDialog(questInstance.QuestTitle, questInstance.DialogPages[currentPage], new string[] { "Accept", "Decline" });
    }

    private void OnOptionSelected(int optionIndex)
    {
        if (!isDialogActive || questInstance == null)
            return;

        if (QuestManager.Instance != null)
        {
            var activeQuest = QuestManager.Instance.GetQuestById(questInstance.QuestId);
            if (activeQuest != null && activeQuest.IsCompleted() && optionIndex == 0)
            {
                DisconnectFromDialog();
                OnQuestTurnedIn();
                return;
            }
        }

        bool isLeaveButton = currentPage == 0 && questInstance.DialogPages.Length == 0;
        if (questDialog != null && optionIndex == 0 && QuestManager.Instance?.HasQuest(questInstance.QuestId) == true
            && !(QuestManager.Instance?.GetQuestById(questInstance.QuestId)?.IsCompleted() ?? false))
        {
            DisconnectFromDialog();
            LockInput.inputLocked = false;
            return;
        }

        if (currentPage < questInstance.DialogPages.Length - 1)
        {
            currentPage++;
            ShowCurrentPage();
        }
        else
        {
            if (optionIndex == 0)
            {
                questAccepted = true;
                DisconnectFromDialog();
                LockInput.inputLocked = false;
                OnQuestAccepted();
            }
            else
            {
                questAccepted = false;
                DisconnectFromDialog();
                LockInput.inputLocked = false;
                OnQuestDeclined();
            }
        }
    }

    private void OnQuestAccepted()
    {
        if (QuestManager.Instance != null && questInstance != null)
        {
            QuestManager.Instance.AddQuest(questInstance);
            QuestChain.Instance?.OnCivilianSpokenTo();
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

            QuestManager.Instance.CompleteQuest(questInstance);
            QuestChain.Instance?.OnQuestsTurnedIn();
            questAccepted = false;
            LockInput.inputLocked = false;

            if (questInstance.QuestId == "mobs")
            {
                dialogue?.ShowDialogue(new (string, string)[]
                {
                    ("Hero", "I’ve dealt with the monsters. Now… are you going to tell me what’s going on?"),
                    ("Arthur", "You truly are the destined hero. Our legends spoke of you."),
                    ("Arthur", "The one who would rid this land of the dangers lurking in the forest."),
                    ("Hero", "That's not what I asked. How do I get back home?"),
                    ("Arthur", "The way home will reveal itself once the threats to this realm are gone. Until then, your path lies here.")
                });
            }
        }
    }

    public void UpdateProgress(int amount = 1)
    {
        if (questAccepted && questInstance != null)
        {
            questInstance.AddProgress(amount);
            if (questInstance.IsCompleted())
                GD.Print($"Quest objective complete! Return to {questInstance.NpcName}");
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
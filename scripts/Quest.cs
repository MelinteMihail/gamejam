using Godot;
using System;

public partial class Quest : Node2D
{
    [Export]
    public QuestData QuestDataTemplate;

    [Export]
    public NodePath PlayerPath = "/root/game/Player";

    [Export]
    public NodePath QuestDialogPath = "/root/game/UI/QuestDialog";

    private QuestData questInstance;
    private QuestDialog questDialog;
    private Node2D player;
    private int currentPage = 0;
    private bool questAccepted = false;
    private bool isDialogActive = false;

    public override void _Ready()
    {
        GD.Print($"Quest _Ready for NPC: {GetParent().Name}");

        if (!PlayerPath.IsEmpty)
        {
            player = GetNodeOrNull<Node2D>(PlayerPath);
            if (player == null)
            {
                GD.PrintErr($"Quest: Could not find Player at path: {PlayerPath}");
            }
        }

        if (!QuestDialogPath.IsEmpty)
        {
            questDialog = GetNodeOrNull<QuestDialog>(QuestDialogPath);
            if (questDialog == null)
            {
                GD.PrintErr($"Quest: Could not find QuestDialog at path: {QuestDialogPath}");
            }
            else
            {
                GD.Print($"Quest: Found QuestDialog successfully");
            }
        }

        if (QuestDataTemplate != null)
        {
            questInstance = QuestDataTemplate.Duplicate();
            GD.Print($"Quest: Created instance with ID: {questInstance.QuestId}");
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
            else if (QuestManager.Instance.HasQuest(questInstance.QuestId))
            {
                GD.Print("Quest already active");
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

    public void UpdateProgress(int amount = 1)
    {
        if (questAccepted && questInstance != null)
        {
            questInstance.AddProgress(amount);

            if (questInstance.IsCompleted())
            {
                OnQuestCompleted();
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
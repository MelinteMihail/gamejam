using Godot;
using System;

public partial class Quest : Node2D
{
    [Export]
    public QuestData QuestData;

    private QuestDialog questDialog;
    private Player player;
    private int currentPage = 0;
    private bool questAccepted = false;

    public override void _Ready()
    {
        player = GetNode<Player>("/root/game/Player");

        questDialog = GetNode<QuestDialog>("/root/game/UI/QuestDialog");
        questDialog.OptionSelected += OnOptionSelected;
    }

    public void StartQuest()
    {
        if (questAccepted)
        {
            questDialog.ShowDialog(QuestData.QuestTitle, QuestData.AcceptedMessage, new string[] { "Leave" });
            return;
        }

        currentPage = 0;
        ShowCurrentPage();
    }
    private void ShowCurrentPage()
    {

        if (currentPage < QuestData.DialogPages.Length - 1)
        {
            questDialog.ShowDialog(QuestData.QuestTitle, QuestData.DialogPages[currentPage], new string[] { "Continue" });
        }
        else
        {
            questDialog.ShowDialog(QuestData.QuestTitle, QuestData.DialogPages[currentPage], new string[] { "Accept", "Decline" });
        }
    }

    private void OnOptionSelected(int optionIndex)
    {

        if (currentPage < QuestData.DialogPages.Length - 1)
        {
            currentPage++;
            ShowCurrentPage();
        }
        else
        {
            if (optionIndex == 0)
            {
                questAccepted = true;

                OnQuestAccepted();
            }
            else
            {
                questAccepted = false;

                OnQuestDeclined();
            }
        }
    }

    private void OnQuestAccepted()
    {
    }

    private void OnQuestDeclined()
    {
    }
}
using Godot;
using System;

public partial class QuestUI : Control
{
    private VBoxContainer questListContainer;
    private Label questCountLabel;
    private bool isExpanded = true;
    private Timer updateTimer;

    public override void _Ready()
    {
        var panel = new PanelContainer();
        AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 25);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        panel.AddChild(margin);

        var vbox = new VBoxContainer();
        margin.AddChild(vbox);

        var header = new HBoxContainer();
        vbox.AddChild(header);

        var titleLabel = new Label();
        titleLabel.Text = "Active Quests";
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0));
        titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(titleLabel);

        questCountLabel = new Label();
        questCountLabel.Text = "(0)";
        header.AddChild(questCountLabel);

        var toggleButton = new Button();
        toggleButton.Text = "[-]";
        toggleButton.CustomMinimumSize = new Vector2(30, 0);
        toggleButton.Pressed += ToggleQuestList;
        header.AddChild(toggleButton);

        questListContainer = new VBoxContainer();
        vbox.AddChild(questListContainer);

        SetAnchorsPreset(LayoutPreset.TopRight);
        Position = new Vector2(-20, 20);

        updateTimer = new Timer();
        updateTimer.WaitTime = 0.5f;
        updateTimer.Autostart = true;
        updateTimer.Timeout += RefreshQuestList;
        AddChild(updateTimer);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.QuestAdded += OnQuestAdded;
            QuestManager.Instance.QuestCompleted += OnQuestCompleted;
        }
        else
        {
            CallDeferred(MethodName.ConnectToQuestManager);
        }

        RefreshQuestList();
    }

    private void ConnectToQuestManager()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.QuestAdded += OnQuestAdded;
            QuestManager.Instance.QuestCompleted += OnQuestCompleted;
            RefreshQuestList();
        }
    }

    private void ToggleQuestList()
    {
        isExpanded = !isExpanded;
        questListContainer.Visible = isExpanded;
    }

    private void OnQuestAdded(QuestData questData)
    {
        RefreshQuestList();
    }

    private void OnQuestCompleted(QuestData questData)
    {
        RefreshQuestList();
    }

    private void RefreshQuestList()
    {
        foreach (Node child in questListContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (QuestManager.Instance == null)
            return;

        var activeQuests = QuestManager.Instance.GetActiveQuests();
        questCountLabel.Text = $"({activeQuests.Count})";

        foreach (var quest in activeQuests)
        {
            var questItem = new PanelContainer();
            questItem.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f),
                CornerRadiusTopLeft = 5,
                CornerRadiusTopRight = 5,
                CornerRadiusBottomLeft = 5,
                CornerRadiusBottomRight = 5
            });

            var itemMargin = new MarginContainer();
            itemMargin.AddThemeConstantOverride("margin_left", 5);
            itemMargin.AddThemeConstantOverride("margin_right", 5);
            itemMargin.AddThemeConstantOverride("margin_top", 5);
            itemMargin.AddThemeConstantOverride("margin_bottom", 5);
            questItem.AddChild(itemMargin);

            var questVBox = new VBoxContainer();
            itemMargin.AddChild(questVBox);

            var titleLabel = new Label();
            titleLabel.Text = quest.QuestTitle;
            titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.6f));
            titleLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            titleLabel.CustomMinimumSize = new Vector2(200, 0);
            questVBox.AddChild(titleLabel);

            var progressLabel = new Label();
            progressLabel.Text = quest.GetProgressText();

            if (quest.IsCompleted())
            {
                progressLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0)); // Green
            }
            else
            {
                progressLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f)); // Light gray
            }

            questVBox.AddChild(progressLabel);

            questListContainer.AddChild(questItem);
        }
    }
}
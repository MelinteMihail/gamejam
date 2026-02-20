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
        margin.AddThemeConstantOverride("margin_left", 5);
        margin.AddThemeConstantOverride("margin_right", 5);
        margin.AddThemeConstantOverride("margin_top", 5);
        margin.AddThemeConstantOverride("margin_bottom", 5);
        panel.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 2);
        margin.AddChild(vbox);

        var header = new HBoxContainer();
        vbox.AddChild(header);

        var titleLabel = new Label();
        titleLabel.Text = "Active Quests";
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0));
        titleLabel.AddThemeFontSizeOverride("font_size", 10);
        titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(titleLabel);

        questCountLabel = new Label();
        questCountLabel.Text = "(0)";
        questCountLabel.AddThemeFontSizeOverride("font_size", 10);
        header.AddChild(questCountLabel);

        var toggleButton = new Button();
        toggleButton.Text = "[-]";
        toggleButton.CustomMinimumSize = new Vector2(20, 0);
        toggleButton.AddThemeFontSizeOverride("font_size", 10);
        toggleButton.Pressed += ToggleQuestList;
        header.AddChild(toggleButton);

        questListContainer = new VBoxContainer();
        questListContainer.AddThemeConstantOverride("separation", 2);
        vbox.AddChild(questListContainer);

        SetAnchorsPreset(LayoutPreset.TopRight);
        Position = new Vector2(0, 100);

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
        if (!IsInstanceValid(questListContainer))
            return;

        foreach (Node child in questListContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (QuestManager.Instance == null)
            return;

        var activeQuests = QuestManager.Instance.GetActiveQuests();
        questCountLabel.Text = $"({activeQuests.Count})";

        Visible = activeQuests.Count > 0;

        foreach (var quest in activeQuests)
        {
            var questItem = new PanelContainer();
            questItem.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f),
                CornerRadiusTopLeft = 3,
                CornerRadiusTopRight = 3,
                CornerRadiusBottomLeft = 3,
                CornerRadiusBottomRight = 3
            });

            var itemMargin = new MarginContainer();
            itemMargin.AddThemeConstantOverride("margin_left", 4);
            itemMargin.AddThemeConstantOverride("margin_right", 4);
            itemMargin.AddThemeConstantOverride("margin_top", 3);
            itemMargin.AddThemeConstantOverride("margin_bottom", 3);
            questItem.AddChild(itemMargin);

            var questVBox = new VBoxContainer();
            questVBox.AddThemeConstantOverride("separation", 1);
            itemMargin.AddChild(questVBox);

            var titleLabel = new Label();
            titleLabel.Text = quest.QuestTitle;
            titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.6f));
            titleLabel.AddThemeFontSizeOverride("font_size", 10);
            titleLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            titleLabel.CustomMinimumSize = new Vector2(120, 0);
            questVBox.AddChild(titleLabel);

            var progressLabel = new Label();
            progressLabel.Text = quest.GetProgressText();
            progressLabel.AddThemeFontSizeOverride("font_size", 9);

            if (quest.IsCompleted())
                progressLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
            else
                progressLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));

            questVBox.AddChild(progressLabel);
            questListContainer.AddChild(questItem);
        }
    }
}
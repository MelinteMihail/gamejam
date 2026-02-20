using Godot;

public partial class QuestSpawnGroup : Node2D
{
    [Export]
    public string TrackedQuestId = "";

    public override void _Ready()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted(TrackedQuestId))
            QueueFree();
    }

    public override void _Process(double delta)
    {
        if (QuestManager.Instance == null)
            return;

        bool questActive = QuestManager.Instance.HasQuest(TrackedQuestId)
                        && !QuestManager.Instance.IsQuestCompleted(TrackedQuestId);

        Visible = questActive;

        foreach (Node child in GetChildren())
        {
            if (child is Node2D node)
            {
                node.SetProcess(questActive);
                node.SetPhysicsProcess(questActive);
            }
            if (child is CollisionObject2D col)
            {
                col.ProcessMode = questActive
                    ? ProcessModeEnum.Inherit
                    : ProcessModeEnum.Disabled;
            }
        }
    }
}
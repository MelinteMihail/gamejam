using Godot;

public partial class QuestArea : Node2D
{
    [Export] 
    public string TrackedQuestId = "";

    private Sprite2D arrow;

    public override void _Ready()
    {
        arrow = GetNode<Sprite2D>("Arrow");

        Tween tween = CreateTween().SetLoops();
        tween.TweenProperty(arrow, "position", new Vector2(0, -10), 0.5f);
        tween.TweenProperty(arrow, "position", new Vector2(0, 0), 0.5f);
    }

    public override void _Process(double delta)
    {
        if (QuestManager.Instance == null) return;

        Visible = QuestManager.Instance.HasQuest(TrackedQuestId)
               && !QuestManager.Instance.IsQuestCompleted(TrackedQuestId);
    }
}
using Godot;
using System.Collections.Generic;

public partial class QuestArrowManager : Control
{
    [Export] 
    private Player player;

    private Dictionary<string, Vector2> questTargets = new()
    {
        { "herbs", new Vector2(-101, -121) },
        { "mobs", new Vector2(151, -15) }
    };

    private Dictionary<string, Vector2> npcPositions = new()
    {
        { "herbs", new Vector2(-127, -15) },
        { "mobs", new Vector2(-203, -16) }
    };

    private Dictionary<string, Sprite2D> activeArrows = new();
    private Texture2D arrowTexture;

    public override void _Ready()
    {
        arrowTexture = GD.Load<Texture2D>("res://assets/extras/arrow.png");
        if (arrowTexture == null)
            GD.PrintErr("Arrow texture not found!");
    }

    public override void _Process(double delta)
    {
        if (QuestManager.Instance == null) return;

        var activeQuests = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in activeQuests)
        {
            if (!activeArrows.ContainsKey(quest.QuestId) && questTargets.ContainsKey(quest.QuestId))
            {
                var arrow = new Sprite2D();
                arrow.Texture = arrowTexture;
                AddChild(arrow);
                activeArrows[quest.QuestId] = arrow;
            }

            GD.Print($"Quest ID: '{quest.QuestId}'");

        }

        foreach (var questId in new List<string>(activeArrows.Keys))
        {
            if (!QuestManager.Instance.HasQuest(questId))
            {
                activeArrows[questId].QueueFree();
                activeArrows.Remove(questId);
            }
        }

        foreach (var (questId, arrow) in activeArrows)
        {
            Vector2 target;
            var quest = QuestManager.Instance.GetQuestById(questId);

            if (quest != null && quest.IsCompleted())
                target = npcPositions[questId];
            else
                target = questTargets[questId];

            Vector2 direction = (target - player.GlobalPosition).Normalized();
            float orbitRadius = 80f;
            Vector2 screenCenter = GetViewport().GetVisibleRect().Size / 2;
            arrow.Position = screenCenter + direction * orbitRadius;
            arrow.Rotation = direction.Angle();
            arrow.Scale = new Vector2(2, 2);
        }

        GD.Print($"Active quests: {activeQuests.Count}, Active arrows: {activeArrows.Count}, Texture: {arrowTexture != null}");
    }
}
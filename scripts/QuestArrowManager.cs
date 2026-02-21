using Godot;
using System.Collections.Generic;

public class QuestArrowTarget
{
    public string GroupName;
    public string FallbackGroup;
}

public partial class QuestArrowManager : Control
{
    private Player player;
    private Texture2D arrowTexture;

    private Dictionary<string, QuestArrowTarget> arrowConfig = new()
    {
        { "herbs",  new QuestArrowTarget { GroupName = "HerbArea",       FallbackGroup = "HerbNPC" } },
        { "mobs",   new QuestArrowTarget { GroupName = "MobArea",        FallbackGroup = "MobNPC" } },
        { "armor",  new QuestArrowTarget { GroupName = "BlacksmithArea", FallbackGroup = "TownExit" } },
        { "forest", new QuestArrowTarget { GroupName = "TownExit",       FallbackGroup = "ForestEntrance" } },
    };

    private Dictionary<string, Vector2> resolvedPositions = new();
    private Dictionary<string, Sprite2D> activeArrows = new();

    public override void _Ready()
    {
        arrowTexture = GD.Load<Texture2D>("res://assets/extras/arrow.png");
        if (arrowTexture == null)
            GD.PrintErr("QuestArrowManager: Arrow texture not found!");

        player = GetTree().GetFirstNodeInGroup("player") as Player;
    }

    public override void _Process(double delta)
    {
        if (player == null)
            player = GetTree().GetFirstNodeInGroup("player") as Player;

        if (player == null || QuestManager.Instance == null)
            return;

        ResolvePositions();

        RemoveArrow("exit");
        RemoveArrow("forest_enter");

        var activeQuests = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in activeQuests)
        {
            if (!arrowConfig.ContainsKey(quest.QuestId)) continue;
            if (!activeArrows.ContainsKey(quest.QuestId))
            {
                var arrow = new Sprite2D();
                arrow.Texture = arrowTexture;
                AddChild(arrow);
                activeArrows[quest.QuestId] = arrow;
            }
        }

        foreach (var questId in new List<string>(activeArrows.Keys))
        {
            if (!QuestManager.Instance.HasQuest(questId))
                RemoveArrow(questId);
        }

        foreach (var (questId, arrow) in activeArrows)
        {
            if (!arrowConfig.ContainsKey(questId)) continue;

            var config = arrowConfig[questId];
            var quest = QuestManager.Instance.GetQuestById(questId);

            string targetGroup = (quest != null && quest.IsCompleted())
                ? config.FallbackGroup
                : config.GroupName;

            if (!resolvedPositions.TryGetValue(targetGroup, out Vector2 target))
                continue;

            PointArrow(arrow, target);
        }
    }

    private void ResolvePositions()
    {
        resolvedPositions.Clear();
        var allGroups = new HashSet<string>();
        foreach (var config in arrowConfig.Values)
        {
            allGroups.Add(config.GroupName);
            allGroups.Add(config.FallbackGroup);
        }
        allGroups.Add("TownExit");
        allGroups.Add("ForestEntrance");

        foreach (var groupName in allGroups)
        {
            var node = GetTree().GetFirstNodeInGroup(groupName) as Node2D;
            if (node != null)
                resolvedPositions[groupName] = node.GlobalPosition;
        }
    }

    private void ShowSingleArrow(string key, string targetGroup)
    {
        if (!activeArrows.ContainsKey(key))
        {
            var arrow = new Sprite2D();
            arrow.Texture = arrowTexture;
            AddChild(arrow);
            activeArrows[key] = arrow;
        }

        if (resolvedPositions.TryGetValue(targetGroup, out Vector2 target))
            PointArrow(activeArrows[key], target);
    }

    private void RemoveArrow(string key)
    {
        if (activeArrows.ContainsKey(key))
        {
            activeArrows[key].QueueFree();
            activeArrows.Remove(key);
        }
    }

    private void PointArrow(Sprite2D arrow, Vector2 target)
    {
        if (player == null) return;

        Vector2 direction = (target - player.GlobalPosition).Normalized();
        float orbitRadius = 80f;

        arrow.GlobalPosition = player.GlobalPosition + direction * orbitRadius;
        arrow.Rotation = direction.Angle();
        arrow.Scale = new Vector2(2, 2);
    }
}
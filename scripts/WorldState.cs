using Godot;
using System.Collections.Generic;

public partial class WorldState : Node
{
    public HashSet<string> CollectedItems = new();
    public HashSet<string> DefeatedEnemies = new();
}
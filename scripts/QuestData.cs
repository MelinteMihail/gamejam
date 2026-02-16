using Godot;
using System;

[GlobalClass]
public partial class QuestData : Resource
{
    [Export]
    public string QuestTitle = "Default Quest Title";
    [Export]
    public string[] DialogPages = new string[]
    {
        "Hello, adventurer! I have a task for you.",
        "I need you to collect 10 herbs from the forest.",
    };
    [Export]
    public string AcceptedMessage = "Quest already accepted.";
}

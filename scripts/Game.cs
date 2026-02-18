using Godot;
using System;

public partial class Game : Node2D
{
	[Export]
	private Dialogue dialogue;
	public override void _Ready()
	{
		var button = GetNode<Button>("Button");
		button.Pressed += OnButtonPressed;

        dialogue.ShowDialogue(new[]
		{
			("", "The room feels heavy..."),
			("John", "I need to get up.")
		});
    }


	private void OnButtonPressed()
	{
		LoadingScreen.NextScenePath = "res://scenes/game.tscn";
		GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }

}

using Godot;
using System;

public partial class Game : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var button = GetNode<Button>("Button");
		button.Pressed += OnButtonPressed;
    }


	private void OnButtonPressed()
	{
		LoadingScreen.NextScenePath = "res://scenes/game.tscn";
		GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }

}

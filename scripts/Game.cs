using Godot;
using System;

public partial class Game : Node2D
{
	[Export]
	private Dialogue dialogue;
	[Export]
	private Marker2D pointA;
	[Export]
	private Marker2D pointB;
	[Export]
	private Player player;

	public override void _Ready()
	{
		var button = GetNode<Button>("Button");
		button.Pressed += OnButtonPressed;

		//LockInput.inputLocked = true;
		//player.GlobalPosition = pointA.GlobalPosition;
		//player.Sprite.Play("walk_right");

		//Tween tween = CreateTween();
		//tween.TweenProperty(player, "global_position", pointB.GlobalPosition, 2.0f);

		//tween.Finished += () =>
		//{
		//	player.Sprite.Play("idle_right");
		//	LockInput.inputLocked = false;
		//};

		dialogue.ShowDialogue(new[]
		{
			("Narrator", "The room feels heavy..."),
			("John", "I need to get up.")
		});
	}


	private void OnButtonPressed()
	{
		LoadingScreen.NextScenePath = "res://scenes/game.tscn";
		GetTree().ChangeSceneToFile("res://scenes/loading_screen.tscn");
    }

}

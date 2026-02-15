using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 300.0f;
	[Export]
	public AnimatedSprite2D Sprite;

	public enum EnumDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	public Vector2 GetInputDirection()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * Speed;

		return inputDirection;
	}

	public void AnimatePlayer()
	{
		Vector2 lastPosition;
		switch (GetInputDirection())
		{
			case Vector2(0, -1):
				Sprite.Play("walk_up");
				lastPosition = GetInputDirection();
				break;
			case Vector2(0, 1):
				Sprite.Play("walk_down");
				break;
			case Vector2(-1, 0):
				Sprite.FlipH = true;
				Sprite.Play("walk_right");
				break;
			case Vector2(1, 0):
				Sprite.FlipH = false;
				Sprite.Play("walk_right");
				break;
			default:
				Sprite.Play("idle_down");
				break;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInputDirection();
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		AnimatePlayer();
	}

}

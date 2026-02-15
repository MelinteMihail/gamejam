using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public float Enemy_Speed = 100.0f;
	public CharacterBody2D Player;

    public override void _Ready()
	{
		var area2D = GetNode<Area2D>("Follow_area");
        area2D.BodyEntered += OnBodyEntered;
        area2D.BodyExited += OnBodyExited;
    }
	private void OnBodyEntered(Node body)
	{
		if (body.IsInGroup("player"))
		{
			Player = body as CharacterBody2D;
		}
	}
	private void OnBodyExited(Node body)
	{
		if (body == Player)
		{
			Player = null;
		}
    }
	public override void _PhysicsProcess(double delta)
	{
		if(Player != null)
		{
			Vector2 direction = (Player.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * Enemy_Speed;
			MoveAndSlide();
		}
		else
		{
			Velocity = Vector2.Zero;
        }
    }

    public override void _Process(double delta)
	{
	}
}

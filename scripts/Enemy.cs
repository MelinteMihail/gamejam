using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public CollisionShape2D CollisionShape;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}

using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public float Enemy_Speed = 100.0f;
	
	private CharacterBody2D Player;
	private Area2D area2D;
	private Health health;

    public override void _Ready()
	{
        AddToGroup("enemy");

        area2D = GetNode<Area2D>("FollowArea");
        area2D.BodyEntered += OnBodyEntered;
        area2D.BodyExited += OnBodyExited;

        health = GetNode<Health>("Health");

        health.Died += OnEnemyDied;
        health.HealthChanged += OnEnemyHealthChanged;

    }
    private void OnEnemyDied()
    {
        QueueFree();
    }

    private void OnEnemyHealthChanged(float current, float max)
    {
        GD.Print($"Enemy health changed: {current}");
    }
    private void OnBodyEntered(Node body)
	{
		if (body.IsInGroup("player"))
			Player = body as CharacterBody2D;
	}
	private void OnBodyExited(Node body)
	{
		if (body == Player)
			Player = null;
    }
	public override void _PhysicsProcess(double delta)
	{
		if (Player != null)
		{
			Vector2 direction = (Player.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * Enemy_Speed;
			MoveAndCollide(direction);
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

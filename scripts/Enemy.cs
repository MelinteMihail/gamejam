using Godot;
using System;
using static Player;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public float Enemy_Speed = 100.0f;
	
	private CharacterBody2D Player;
	private Area2D area2D;
	private Health health;
	private Vector2 lastPosition;
	[Export]
	private AnimatedSprite2D animatedSprite;
    public EnemyEnumDirection currentEnemyDirection = EnemyEnumDirection.Down;
    public enum EnemyEnumDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
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
			lastPosition = direction;
			MoveAndCollide(direction);
            GetEnemyInputDirection();
        }
		else
		{
			Velocity = Vector2.Zero;
		}
	}
    private Vector2 GetEnemyInputDirection()
    {
        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
        Velocity = inputDirection * Enemy_Speed;
        if (inputDirection != Vector2.Zero)
        {
            lastPosition = inputDirection;

            if (Mathf.Abs(inputDirection.X) > Mathf.Abs(inputDirection.Y))
            {
                currentEnemyDirection = inputDirection.X > 0 ? EnemyEnumDirection.Right : EnemyEnumDirection.Left;
            }
            else
            {
                currentEnemyDirection = inputDirection.Y > 0 ? EnemyEnumDirection.Down : EnemyEnumDirection.Up;
            }
        }

        return inputDirection;
    }


    private void AnimateEnemy()
    {
        bool isMoving = Velocity != Vector2.Zero;

        switch (currentEnemyDirection)
        {
            case EnemyEnumDirection.Up:
                animatedSprite.Play(isMoving ? "walk_up" : "idle_up");
                break;

            case EnemyEnumDirection.Down:
                animatedSprite.Play(isMoving ? "walk_down" : "idle_down");
                break;

            case EnemyEnumDirection.Left:
                animatedSprite.FlipH = true;
                animatedSprite.Play(isMoving ? "walk_right" : "idle_right");
                break;

            case EnemyEnumDirection.Right:
                animatedSprite.FlipH = false;
                animatedSprite.Play(isMoving ? "walk_right" : "idle_right");
                break;
        }

    }

    public override void _Process(double delta)
	{
        AnimateEnemy();
    }
}

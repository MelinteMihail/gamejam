using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 200.0f;
	[Export]
	public AnimatedSprite2D Sprite;
	[Export]
	public CollisionShape2D CollisionShape;
	[Export]
	public int MaxHealth = 100;

	private float currentHealth;
	private Vector2 lastPosition;
	private EnumDirection currentDirection = EnumDirection.Down;

	public enum EnumDirection
	{
		Up,
		Down,
		Left,
		Right
	}

    public override void _Ready()
    {
        currentHealth = MaxHealth;
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

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
		
		int currentHealthInt = (int)currentHealth;
        GD.Print("Player's current health is: " + currentHealthInt);

        if (currentHealthInt <= 0)
        {
            GD.Print("Player has died.");
            Die();
        }
    }

    private void Die()
    {
        QueueFree();
    }

    private Vector2 GetInputDirection()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * Speed;

		if (inputDirection != Vector2.Zero)
			lastPosition = inputDirection;

		return inputDirection;
	}

	private void AnimatePlayer()
	{
		switch (GetInputDirection())
		{
			case Vector2(0, -1):
				Sprite.Play("walk_up");
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
				if (lastPosition.X == 0)
				{
					if (lastPosition.Y < 0)
						Sprite.Play("idle_up");
					else if (lastPosition.Y > 0)
						Sprite.Play("idle_down");
				}
				else if (lastPosition.Y == 0)
				{
					if (lastPosition.X < 0)
					{
						Sprite.FlipH = true;
						Sprite.Play("idle_right");
					}

					else if (lastPosition.X > 0)
					{
						Sprite.FlipH = false;
						Sprite.Play("idle_right");
					}
				}
				break;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		AnimatePlayer();
	}

}

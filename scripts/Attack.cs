using Godot;
using System;

public partial class Attack : Node2D
{
	private Area2D hitboxArea;
	private AnimatedSprite2D attackSprite;

	private Health health;

	public override void _Ready()
	{
		attackSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        hitboxArea = GetNode<Area2D>("HitboxArea");

        hitboxArea.BodyEntered += OnBodyEntered;
		attackSprite.Play();
		attackSprite.AnimationFinished += OnAnimationFinished;
    }

	public void SetDirection(Vector2 direction)
	{
		float angle = Mathf.Atan2(direction.Y, direction.X);
		Rotation = angle;
    }

    private void OnBodyEntered(Node2D body)
	{
		if(body is  Enemy enemy)
		{
			var enemyHealth = enemy.GetNode<Health>("Health");

			if (enemyHealth != null)
				enemyHealth.TakeDamage(10);
        }
	}
	public void OnAnimationFinished()
	{
        QueueFree();
    }


}

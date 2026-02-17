using Godot;
using System;

public partial class Attack : Node2D
{
	[Signal]
	public delegate void AttackFinishedEventHandler();
    private Area2D hitboxArea;
	private AnimatedSprite2D attackSprite;

	private Health health;
	private Player player;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/game/Player");
        attackSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        hitboxArea = GetNode<Area2D>("HitboxArea");

        hitboxArea.BodyEntered += OnBodyEntered;
		attackSprite.Play();
		attackSprite.AnimationFinished += OnAnimationFinished;
    }

	public void SetDirection()
	{
		switch (player.currentDirection)
		{
			case Player.EnumDirection.Up:
				attackSprite.RotationDegrees = 360;
                break;

			case Player.EnumDirection.Down:
				attackSprite.RotationDegrees = 180;
				break;
			case Player.EnumDirection.Left:
				attackSprite.FlipH = true;
                break;

        }
    }

    private void OnBodyEntered(Node2D body)
	{
		GD.Print($"Hitbox entered by: {body.Name}");
        if (body is  Enemy enemy)
		{
			var enemyHealth = enemy.GetNode<Health>("Health");

			if (enemyHealth != null)
				enemyHealth.TakeDamage(10);
        }
	}
	public void OnAnimationFinished()
	{
		EmitSignal("AttackFinished");
        QueueFree();
    }


}

using Godot;
using System;

public partial class Attack : Node2D
{
    [Signal]
    public delegate void AttackFinishedEventHandler();

    private Area2D hitboxArea;
    private AnimatedSprite2D attackSprite;
    private Player player;
    private float attackDamage = 10f;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as Player;
        attackSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        hitboxArea = GetNode<Area2D>("HitboxArea");
        hitboxArea.BodyEntered += OnBodyEntered;
        attackSprite.Play();
        attackSprite.AnimationFinished += OnAnimationFinished;
    }

    public void SetDamage(float damage)
    {
        attackDamage = damage;
        GD.Print($"Attack damage set to: {attackDamage}");
    }

	public void SetDirection()
	{
        switch (player.currentPlayerDirection)
        {
            case Player.PlayerEnumDirection.Up:
                attackSprite.RotationDegrees = 360;
                break;

            case Player.PlayerEnumDirection.Down:
                attackSprite.RotationDegrees = 180;
                break;
            case Player.PlayerEnumDirection.Left:
                attackSprite.FlipH = true;
                break;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        GD.Print($"Hitbox entered by: {body.Name}");

        if (body.IsInGroup("enemy") || body.IsInGroup("boss"))
        {
            var health = body.GetNodeOrNull<Health>("Health");
            var enemy = body as Enemy;
            var boss = body as Boss;
            if (health != null)
            {
                if (attackDamage < health.CurrentHealth)
                    enemy?.Flicker();
                if (attackDamage < health.CurrentHealth)
                    boss?.Flicker();
                health.TakeDamage(attackDamage);
                GD.Print($"Dealt {attackDamage} damage to {body.Name}");
            }
        }
    }

    public void OnAnimationFinished()
    {
        EmitSignal("AttackFinished");
        QueueFree();
    }
}
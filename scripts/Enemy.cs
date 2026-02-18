using Godot;
using System;
using static Player;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public float Enemy_Speed = 100.0f;
	[Export]
	public string EnemyType = "";
    private CharacterBody2D Player;
    private bool isAttacking = false;
    private Area2D area2D;
	private Health health;
	private Vector2 lastPosition;
    private Collision Attack;
	[Export]
	private AnimatedSprite2D animatedSprite;
    public EnemyEnumDirection currentEnemyDirection = EnemyEnumDirection.Down;

    private string EnemyTypePrefix;
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
        Attack = GetNode<Collision>("Collision Area");
        area2D = GetNode<Area2D>("FollowArea");
        area2D.BodyEntered += OnBodyEntered;
        area2D.BodyExited += OnBodyExited;

        health = GetNode<Health>("Health");

        health.Died += OnEnemyDied;
        health.HealthChanged += OnEnemyHealthChanged;

        EnemyTypePrefix = EnemyType + "_";

        Attack.AttackStarted += OnAttackStarted;
        animatedSprite.AnimationFinished += OnAnimationFinished;


    }
    private void OnEnemyDied()
    {
        if (QuestManager.Instance != null)
        {
            var activeQuests = QuestManager.Instance.GetActiveQuests();

            foreach (var quest in activeQuests)
            {
                if (quest.ProgressType == EnemyType && !quest.IsCompleted())
                {
                    quest.AddProgress(1);
                    GD.Print($"Quest progress: {quest.GetProgressText()}");

                    if (quest.IsCompleted())
                    {
                        GD.Print($"Quest objectives complete! Return to {quest.NpcName}");
                    }
                }
            }
        }

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

    public override void _Process(double delta)
    {
        AnimateEnemy();
    }

    private void GetEnemyInputDirection()
    {
        if (lastPosition != Vector2.Zero)
        {
            if (Mathf.Abs(lastPosition.X) > Mathf.Abs(lastPosition.Y))
            {
                currentEnemyDirection = lastPosition.X > 0 ? EnemyEnumDirection.Right : EnemyEnumDirection.Left;
            }
            else
            {
                currentEnemyDirection = lastPosition.Y > 0 ? EnemyEnumDirection.Down : EnemyEnumDirection.Up;
            }
        }
    }


    private void AnimateEnemy()
    {
        if (isAttacking)
            return; 
        bool isMoving = Velocity != Vector2.Zero;
        
        switch (currentEnemyDirection)
        {
            case EnemyEnumDirection.Up:
                animatedSprite.Play(EnemyTypePrefix + (isMoving ? "walk_up" : "idle_up"));
                break;

            case EnemyEnumDirection.Down:
                animatedSprite.Play(EnemyTypePrefix + (isMoving ? "walk_down" : "idle_down"));
                break;

            case EnemyEnumDirection.Left:
                animatedSprite.FlipH = true;
                animatedSprite.Play(EnemyTypePrefix + (isMoving ? "walk_right" : "idle_right"));
                break;

            case EnemyEnumDirection.Right:
                animatedSprite.FlipH = false;
                animatedSprite.Play(EnemyTypePrefix + (isMoving ? "walk_right" : "idle_right"));
                break;
        }

    }
    private void OnAttackStarted()
    {
        isAttacking = true;

        switch (currentEnemyDirection)
        {
            case EnemyEnumDirection.Up:
                animatedSprite.Play(EnemyTypePrefix + "attack_up");
                break;
            case EnemyEnumDirection.Down:
                animatedSprite.Play(EnemyTypePrefix+ "attack_down");
                break;
            case EnemyEnumDirection.Left:
                animatedSprite.FlipH = true;
                animatedSprite.Play(EnemyTypePrefix + "attack_right");
                break;
            case EnemyEnumDirection.Right:
                animatedSprite.FlipH = false;
                animatedSprite.Play(EnemyTypePrefix + "attack_right");
                break;
        }
    }
    private void OnAnimationFinished()
    {
        string anim = animatedSprite.Animation.ToString();

        if (anim.StartsWith("attack"))
        {
            isAttacking = false;
            Attack.ApplyDamageOnce();
        }
    }

}

using Godot;
using System;
using static Player;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public float Enemy_Speed = 100.0f;
	[Export]
	public string EnemyType = "";
	[Export]
	private AnimatedSprite2D animatedSprite;
    [Export]
    private PackedScene ProjectileScene;

    private AudioStreamPlayer2D hurtSound;
    private CharacterBody2D Player;
    private Area2D area2D;
	private Health health;
    private Collision Attack;
    private AudioStreamPlayer2D attackSound;

    private bool isAttacking = false;
    private string EnemyTypePrefix;
	
    private Vector2 lastPosition;
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

        Attack = GetNode<Collision>("Collision Area");
        attackSound = GetNode<AudioStreamPlayer2D>("AttackSound");
        hurtSound = GetNode<AudioStreamPlayer2D>("HurtSound");
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
        hurtSound?.Play();
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
                        GD.Print($"Quest objectives complete! Return to {quest.NpcName}");
                }
            }
        }

        SetPhysicsProcess(false);
        SetProcess(false);
        isAttacking = true; 
        animatedSprite.Play(EnemyTypePrefix + "death");
        animatedSprite.AnimationFinished -= OnAnimationFinished;
        animatedSprite.AnimationFinished += () => QueueFree();
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
            lastPosition = direction;
            GetEnemyInputDirection();

            if (EnemyType == "goblin")
            {
                Velocity = direction * Enemy_Speed;
                MoveAndCollide(direction);
            }
            
        }
        else
		{
			Velocity = Vector2.Zero;
		}
        if (EnemyType == "eye")
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
        string targetAnim = "";
        bool flip = false;

        switch (currentEnemyDirection)
        {
            case EnemyEnumDirection.Up:
                targetAnim = EnemyTypePrefix + (isMoving ? "walk_up" : "idle_up");
                break;
            case EnemyEnumDirection.Down:
                targetAnim = EnemyTypePrefix + (isMoving ? "walk_down" : "idle_down");
                break;
            case EnemyEnumDirection.Left:
                flip = true;
                targetAnim = EnemyTypePrefix + (isMoving ? "walk_right" : "idle_right");
                break;
            case EnemyEnumDirection.Right:
                targetAnim = EnemyTypePrefix + (isMoving ? "walk_right" : "idle_right");
                break;
            default:
                return;
        }
        animatedSprite.FlipH = flip;

        if (animatedSprite.Animation.ToString() != targetAnim)
            animatedSprite.Play(targetAnim);
    }
    private void OnAttackStarted()
    {
        isAttacking = true;
        attackSound?.Play();

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

        if (anim.StartsWith(EnemyTypePrefix + "attack"))
        {
            isAttacking = false;

            if (EnemyType == "eye" && ProjectileScene != null && Player != null)
            {
                var projectile = ProjectileScene.Instantiate<Projectile>();
                GetParent().AddChild(projectile);
                projectile.GlobalPosition = GlobalPosition;
                Vector2 dir = (Player.GlobalPosition - GlobalPosition).Normalized();
                projectile.Initialize(dir);
                Attack.isAttacking = false;
                Attack.ResetCooldown();     
            }
            else
            {
                Attack.ApplyDamageOnce();
            }
        }
    }
    public async void Flicker(float duration = 0.4f, float interval = 0.05f)
    {
        if (animatedSprite == null)
            return;

        Color originalColor = animatedSprite.Modulate;
        Color flashColor = new Color(3f, 3f, 3f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            animatedSprite.Modulate = flashColor;
            await ToSignal(GetTree().CreateTimer(interval), "timeout");

            animatedSprite.Modulate = originalColor;
            await ToSignal(GetTree().CreateTimer(interval), "timeout");

            elapsed += interval * 2;
        }

        animatedSprite.Modulate = originalColor;
    }
}

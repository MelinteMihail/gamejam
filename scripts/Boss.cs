using Godot;
using System;

public partial class Boss : CharacterBody2D
{
    [Export] 
    public float BossSpeed = 80f;
    [Export] 
    public Label NameLabel;
    [Export] 
    public TextureProgressBar HealthBar;
    [Export] 
    private AnimatedSprite2D animatedSprite;

    private CharacterBody2D Player;
    private Collision Attack;
    private Health health;
    private AudioStreamPlayer2D attackSound;
    private AudioStreamPlayer2D hurtSound;

    private Vector2 lastDirection;
    private bool isAttacking = false;

    public enum BossDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public BossDirection currentDirection = BossDirection.Down;

    public override void _Ready()
    {
        AddToGroup("boss");

        health = GetNode<Health>("Health");
        Attack = GetNode<Collision>("Collision Area");
        attackSound = GetNode<AudioStreamPlayer2D>("AttackSound");
        hurtSound = GetNode<AudioStreamPlayer2D>("HurtSound");

        var followArea = GetNode<Area2D>("FollowArea");
        followArea.BodyEntered += OnBodyEntered;
        followArea.BodyExited += OnBodyExited;

        health.Died += OnBossDied;
        health.HealthChanged += OnHealthChanged;

        Attack.AttackStarted += OnAttackStarted;

        animatedSprite.AnimationFinished += OnAnimationFinished;

        NameLabel.Text = "Boss";
        HealthBar.MinValue = 0;
        HealthBar.MaxValue = health.maxHealth;
        HealthBar.Value = health.CurrentHealth;
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
        if (Player != null && !isAttacking)
        {
            Vector2 direction = (Player.GlobalPosition - GlobalPosition).Normalized();
            lastDirection = direction;

            UpdateDirection(direction);

            Velocity = direction * BossSpeed;
            MoveAndCollide(direction);
        }
        else
        {
            Velocity = Vector2.Zero;
        }
    }

    public override void _Process(double delta)
    {
        AnimateBoss();
    }

    private void UpdateDirection(Vector2 direction)
    {
        if (direction == Vector2.Zero)
            return;

        if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
        {
            currentDirection = direction.X > 0 ? BossDirection.Right : BossDirection.Left;
        }
        else
        {
            currentDirection = direction.Y > 0 ? BossDirection.Down : BossDirection.Up;
        }
    }

    private void AnimateBoss()
    {
        if (isAttacking)
            return;

        bool isMoving = Velocity != Vector2.Zero;
        string targetAnim = "";
        bool flip = false;

        switch (currentDirection)
        {
            case BossDirection.Up:
                targetAnim = isMoving ? "walk_up" : "idle_up";
                break;

            case BossDirection.Down:
                targetAnim = isMoving ? "walk_down" : "idle_down";
                break;

            case BossDirection.Left:
                flip = true;
                targetAnim = isMoving ? "walk_right" : "idle_right";
                break;

            case BossDirection.Right:
                targetAnim = isMoving ? "walk_right" : "idle_right";
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

        string anim = "";
        bool flip = false;

        switch (currentDirection)
        {
            case BossDirection.Up:
                anim = "attack_up";
                break;

            case BossDirection.Down:
                anim = "attack_down";
                break;

            case BossDirection.Left:
                flip = true;
                anim = "attack_right";
                break;

            case BossDirection.Right:
                anim = "attack_right";
                break;
        }

        animatedSprite.FlipH = flip;
        animatedSprite.Play(anim);
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

    private void OnHealthChanged(float current, float max)
    {
        HealthBar.Value = current;
        GD.Print($"Boss Health: {current}/{max}");
    }

    private void OnBossDied()
    {
        GD.Print("Boss died!");
        hurtSound?.Play();
        SetPhysicsProcess(false);
        SetProcess(false);
        isAttacking = true;
        animatedSprite.Play("death");
        animatedSprite.AnimationFinished -= OnAnimationFinished;
        animatedSprite.AnimationFinished += () => QueueFree();
    }
}

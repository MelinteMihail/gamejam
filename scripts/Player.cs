using Godot;
using System;
using System.Collections.Generic;
using static Player;

public partial class Player : CharacterBody2D
{
    [Export]
    public float Speed = 200.0f;
    [Export]
    public AnimatedSprite2D Sprite;
    [Export]
    public CollisionShape2D CollisionShape;
    [Export]
    public float LampDamagePerSecond = 10.0f;

    private Health health;
    private Area2D LampArea;
    private List<Health> enemiesInLamp = new();

    private Vector2 lastPosition;
    private EnumDirection currentDirection = EnumDirection.Down;
    private bool isAttacking = false;

    public enum EnumDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public override void _Ready()
    {
        LampArea = GetNode<Area2D>("LampArea");
        health = GetNode<Health>("Health");

        health.Died += OnPlayerDied;
        health.HealthChanged += OnHealthChanged;

        LampArea.BodyEntered += OnBodyEnteredLampArea;
        LampArea.BodyExited += OnBodyExitedLampArea;

    }

    public override void _PhysicsProcess(double delta)
    {
        GetInputDirection();
        MoveAndSlide();

        float damage = LampDamagePerSecond * (float) delta;

        foreach (var enemyHealth in enemiesInLamp)
        {
            if (enemyHealth != null && !enemyHealth.IsQueuedForDeletion())
                enemyHealth.TakeDamage(damage);
        }

        enemiesInLamp.RemoveAll(e => e == null || e.IsQueuedForDeletion());
    }

    public override void _Process(double delta)
    {
        AnimatePlayer();
        Attack();
    }

    private void OnPlayerDied()
    {
        QueueFree();
    }

    private void OnBodyEnteredLampArea(Node body)
    {
        if (body.IsInGroup("enemy"))
        {
            var enemyHealth = body.GetNodeOrNull<Health>("Health");

            if (enemyHealth != null)
                enemiesInLamp.Add(enemyHealth);
        }
    }

    private void OnBodyExitedLampArea(Node body)
    {
        if (body.IsInGroup("enemy"))
        {
            var enemyHealth = body.GetNodeOrNull<Health>("Health");
            if (enemyHealth != null)
            {
                enemiesInLamp.Remove(enemyHealth);
            }
        }
    }

    private void OnHealthChanged(float current, float max)
    {
        GD.Print($"Player health changed: {(int) current}");
    }

    private Vector2 GetInputDirection()
    {
        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
        Velocity = inputDirection * Speed;

        if (inputDirection != Vector2.Zero)
        {
            lastPosition = inputDirection;

            if (Mathf.Abs(inputDirection.X) > Mathf.Abs(inputDirection.Y))
            {
                currentDirection = inputDirection.X > 0 ? EnumDirection.Right : EnumDirection.Left;
            }
            else
            {
                currentDirection = inputDirection.Y > 0 ? EnumDirection.Down : EnumDirection.Up;
            }
        }

        return inputDirection;
    }


    private void AnimatePlayer()
    {
        bool isMoving = Velocity != Vector2.Zero;

        switch (currentDirection)
        {
            case EnumDirection.Up:
                Sprite.Play(isMoving ? "walk_up" : "idle_up");
                break;

            case EnumDirection.Down:
                Sprite.Play(isMoving ? "walk_down" : "idle_down");
                break;

            case EnumDirection.Left:
                Sprite.FlipH = true;
                Sprite.Play(isMoving ? "walk_right" : "idle_right");
                break;

            case EnumDirection.Right:
                Sprite.FlipH = false;
                Sprite.Play(isMoving ? "walk_right" : "idle_right");
                break;
        }
        
	}
    public EnumDirection GetCurrentDirection()
    {
        return currentDirection;
    }

    private void SpawnAttack()
    {
        
        var scene = GD.Load<PackedScene>("res://scenes/Attack.tscn");
        if (scene != null)
        {
            var attackInstance = scene.Instantiate<Attack>();
            GetParent().AddChild(attackInstance);
            attackInstance.GlobalPosition = GlobalPosition;
            Vector2 directionVector = lastPosition.Normalized();
            attackInstance.SetDirection(directionVector);
            attackInstance.Connect("AttackFinished", new Callable(this, "OnAttackFinished"));
            isAttacking = true;
        }
        else
        {
            GD.PrintErr("Failed to load Attack scene!");
        }
    }

    public void Attack()
    {
        if (Input.IsActionJustPressed("attack") && isAttacking == false)
        {
            SpawnAttack();
        }
    }
    private void OnAttackFinished()
    {
        isAttacking = false;
    }
}
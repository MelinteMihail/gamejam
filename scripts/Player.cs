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
            lastPosition = inputDirection;

        switch (inputDirection)
        {
            case Vector2(0, -1):
                currentDirection = EnumDirection.Up;
                break;

            case Vector2(0, 1):
                currentDirection = EnumDirection.Down;
                break;

            case Vector2(-1, 0):
                currentDirection = EnumDirection.Left;
                break;

            case Vector2(1, 0):
                currentDirection = EnumDirection.Right;
                break;

            default:
                currentDirection = EnumDirection.None;
                break;
        }

        return inputDirection;
    }

    private void AnimatePlayer()
    {
        switch (currentDirection)
        {
            case EnumDirection.Up:
                Sprite.Play("walk_up");
                break;

            case EnumDirection.Down:
                Sprite.Play("walk_down");
                break;

            case EnumDirection.Left:
                Sprite.FlipH = true;
                Sprite.Play("walk_right");
                break;

            case EnumDirection.Right:
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
}
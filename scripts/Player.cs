using Godot;
using System;
using System.Collections.Generic;
using static Player;

public partial class Player : CharacterBody2D
{
    [Export]
    public float Speed = 200.0f;
    [Export]
    public float baseDurability = 100.0f;
    [Export]
    public float baseDamage = 10.0f;
    [Export]
    public AnimatedSprite2D Sprite;
    [Export]
    public CollisionShape2D CollisionShape;
    [Export]
    public float LampDamagePerSecond = 10.0f;
    [Export]
    public float attackDistance = 20.0f;

    public float durabilityMultiplier = 1.0f;
    public float attackMultiplier = 1.0f;

    public float CurrentDurability => baseDurability * durabilityMultiplier;
    public float CurrentDamage => baseDamage * attackMultiplier;

    private Health health;
    private Area2D LampArea;
    private List<Health> enemiesInLamp = new();

    private Vector2 lastPosition;
    public EnumDirection currentDirection = EnumDirection.Down;
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
        if (LockInput.inputLocked)
            return;

        GetInputDirection();
        MoveAndSlide();

        float baseDamage = LampDamagePerSecond * (float)delta;
        float actualDamage = baseDamage * attackMultiplier;

        foreach (var enemyHealth in enemiesInLamp)
        {
            if (enemyHealth != null && !enemyHealth.IsQueuedForDeletion())
                enemyHealth.TakeDamage(actualDamage);
        }

        enemiesInLamp.RemoveAll(e => e == null || e.IsQueuedForDeletion());
    }

    public override void _Process(double delta)
    {
        if (LockInput.inputLocked)
            return;

        AnimatePlayer();
        Attack();
    }

    private void OnPlayerDied()
    {
        QueueFree();
    }

    public void EquipArmorSet(float durabilityBonus, float damageBonus)
    {
        durabilityMultiplier += (durabilityBonus / 100.0f);
        attackMultiplier += (damageBonus / 100.0f);

        if (durabilityBonus >= 20f)
            Sprite.SpriteFrames = GD.Load<SpriteFrames>("res://path/to/steel_armor_spriteframes.tres");
        else if (durabilityBonus >= 10f)
            Sprite.SpriteFrames = GD.Load<SpriteFrames>("res://path/to/iron_armor_spriteframes.tres");

        GD.Print($"Armor equipped! DUR: {durabilityMultiplier}x, ATK: {attackMultiplier}x");
        GD.Print($"Current stats - Durability: {CurrentDurability}, Attack: {CurrentDamage}");
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
        GD.Print($"Player health changed: {(int)current}");
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
            attackInstance.GlobalPosition = GlobalPosition + directionVector * attackDistance;

            attackInstance.SetDamage(CurrentDamage);

            attackInstance.SetDirection();
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
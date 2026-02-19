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
    [Export]
    public AnimatedSprite2D Hurt;
    public float durabilityMultiplier = 1.0f;
    public float attackMultiplier = 1.0f;

    public float CurrentDurability => baseDurability * durabilityMultiplier;
    public float CurrentDamage => baseDamage * attackMultiplier;

    public HealthBar healthBar;

    private Health health;
    private Area2D LampArea;
    private List<Health> enemiesInLamp = new();

    private Vector2 lastPosition;
    private Vector2 respawnPosition;
    public PlayerEnumDirection currentPlayerDirection = PlayerEnumDirection.Down;
    private bool isAttacking = false;

    private string armorPrefix = "";

    public enum PlayerEnumDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public override void _Ready()
    {
        respawnPosition = GlobalPosition;

        LampArea = GetNode<Area2D>("LampArea");
        health = GetNode<Health>("Health");

        health.Died += OnPlayerDied;
        health.HealthChanged += OnHealthChanged;

        healthBar = GetNodeOrNull<HealthBar>("/root/game/UI/HealthBar");

        if (healthBar != null)
        {
            health.HealthChanged += (current, max) => healthBar.UpdateHealth(current, max);

            healthBar.CallDeferred("UpdateHealth", health.CurrentHealth, (float)health.maxHealth);
        }

        LampArea.BodyEntered += OnBodyEnteredLampArea;
        LampArea.BodyExited += OnBodyExitedLampArea;
        GD.Print(healthBar == null ? "HealthBar NOT found" : "HealthBar found");
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

    public void SetCheckpoint(Vector2 position)
    {
        respawnPosition = position;
    }

    private async void OnPlayerDied()
    {
        SetPhysicsProcess(false);
        SetProcess(false);

        Sprite.Play(armorPrefix + "death");
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        Respawn();
    }
    private void Respawn()
    {
        GlobalPosition = respawnPosition;
        Velocity = Vector2.Zero;
        enemiesInLamp.Clear();

        health.Reset();

        GetTree().ChangeSceneToFile("res://scenes/death_screen.tscn");

        SetPhysicsProcess(true);
        SetProcess(true);
    }

    public void EquipArmorSet(float durabilityBonus, float damageBonus)
    {
        durabilityMultiplier += (durabilityBonus / 100.0f);
        attackMultiplier += (damageBonus / 100.0f);

        if (durabilityBonus >= 20f)
            armorPrefix = "steel_";
        else if (durabilityBonus >= 10f)
            armorPrefix = "iron_";

        GD.Print($"Armor equipped! DUR: {durabilityMultiplier}x, ATK: {attackMultiplier}x");
        GD.Print($"Current stats - Durability: {CurrentDurability}, Attack: {CurrentDamage}");
        GD.Print($"Animation prefix set to: {armorPrefix}");
    }

    private void OnBodyEnteredLampArea(Node body)
    {
        if (body.IsInGroup("enemy") || body.IsInGroup("boss"))
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
        Hurt.Play("Hurt");
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
                currentPlayerDirection = inputDirection.X > 0 ? PlayerEnumDirection.Right : PlayerEnumDirection.Left;
            }
            else
            {
                currentPlayerDirection = inputDirection.Y > 0 ? PlayerEnumDirection.Down : PlayerEnumDirection.Up;
            }
        }

        return inputDirection;
    }

    private void AnimatePlayer()
    {
        bool isMoving = Velocity != Vector2.Zero;

        switch (currentPlayerDirection)
        {
            case PlayerEnumDirection.Up:
                Sprite.Play(armorPrefix + (isMoving ? "walk_up" : "idle_up"));
                break;

            case PlayerEnumDirection.Down:
                Sprite.Play(armorPrefix + (isMoving ? "walk_down" : "idle_down"));
                break;

            case PlayerEnumDirection.Left:
                Sprite.Play(armorPrefix + (isMoving ? "walk_left" : "idle_left"));
                break;

            case PlayerEnumDirection.Right:
                Sprite.Play(armorPrefix + (isMoving ? "walk_right" : "idle_right"));
                break;
        }
    }

    public PlayerEnumDirection GetCurrentDirection()
    {
        return currentPlayerDirection;
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
            if (GetViewport().GuiGetHoveredControl() != null)
                return;

            SpawnAttack();
        }
    }

    private void OnAttackFinished()
    {
        isAttacking = false;
    }
}
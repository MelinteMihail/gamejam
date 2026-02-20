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
    private AudioStreamPlayer2D attackSound;
    private AudioStreamPlayer2D hurtSound;
    private AudioStreamPlayer2D grassSound;
    private AudioStreamPlayer2D stoneSound;
    private TileMapLayer tileMap;
    private Health health;
    private Area2D LampArea;
    private List<Health> enemiesInLamp = new();
    private Node2D lanternpivot;
    private PointLight2D lanternLight;

    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f;
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
        lanternpivot = GetNode<Node2D>("LanternPivot");
        lanternpivot.Visible = false;
        var gameState = GetNodeOrNull<LanternState>("/root/Lantern");
        if (gameState != null && gameState.HasLantern)
            lanternpivot.Visible = true;
        lanternLight = GetNode<PointLight2D>("LanternLight");
        lanternLight.Energy = 0.5f;
        respawnPosition = GlobalPosition;
        AddToGroup("player");
        LampArea = GetNode<Area2D>("LampArea");
        health = GetNode<Health>("Health");
        attackSound = GetNode<AudioStreamPlayer2D>("AttackSound");
        hurtSound = GetNode<AudioStreamPlayer2D>("HurtSound");
        grassSound = GetNode<AudioStreamPlayer2D>("GrassSound");
        stoneSound = GetNode<AudioStreamPlayer2D>("StoneSound");
        tileMap = GetTree().GetFirstNodeInGroup("GroundLayer") as TileMapLayer;

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
        PlayFootstepSound(delta);

        if (LockInput.inputLocked)
            return;

        GetInputDirection();
        MoveAndSlide();

        float baseDamage = LampDamagePerSecond * (float)delta;
        float actualDamage = baseDamage * attackMultiplier;

        foreach (var enemyHealth in enemiesInLamp)
        {
            if (GodotObject.IsInstanceValid(enemyHealth))
            {
                enemyHealth.TakeDamage(actualDamage);
            }
        }

        enemiesInLamp.RemoveAll(e => !GodotObject.IsInstanceValid(e));
    }

    public override void _Process(double delta)
    {
        if (LockInput.inputLocked)
            return;

        AnimatePlayer();
        Attack();
    }

    private void PlayFootstepSound(double delta)
    {
        if (Velocity == Vector2.Zero || tileMap == null)
            return;

        footstepTimer -= (float)delta;
        if (footstepTimer > 0)
            return;

        footstepTimer = footstepInterval;

        Vector2I tilePos = tileMap.LocalToMap(tileMap.ToLocal(GlobalPosition));
        TileData tileData = tileMap.GetCellTileData(tilePos);

        GD.Print($"TilePos: {tilePos}, TileData: {tileData}, tileMap: {tileMap.Name}");

        if (tileData == null)
            return;

        string surface = tileData.GetCustomData("surface").AsString();
        GD.Print($"Surface: {surface}");

        int sourceId = tileMap.GetCellSourceId(tilePos);

        switch (sourceId)
        {
            case 0: grassSound?.Play(); break;
            case 1:
            case 2: stoneSound?.Play(); break;
        }
    }

    public void SetCheckpoint(Vector2 position)
    {
        respawnPosition = position;
    }

    private async void OnPlayerDied()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        hurtSound?.Play();
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
        if (body.IsInGroup("enemy") || body.IsInGroup("boss"))
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
        hurtSound?.Play();
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
            attackSound?.Play();
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
    public void EnableLantern()
    {
        lanternpivot.Visible = true;
        lanternLight.Energy = 2.5f;
    }
}
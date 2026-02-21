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
    public float attackDistance = 20.0f;
    [Export]
    public AnimatedSprite2D Hurt;
    [Export]
    public Node2D LanternPivot;

    public float CurrentDamage = 10f;

    public HealthBar healthBar;
    private AudioStreamPlayer2D attackSound;
    private AudioStreamPlayer2D hurtSound;
    private AudioStreamPlayer2D grassSound;
    private AudioStreamPlayer2D dirtSound;
    private AudioStreamPlayer2D stoneSound;
    private TileMapLayer grassLayer;
    private TileMapLayer dirtLayer;
    private TileMapLayer stoneLayer;
    private Health health;
    private Area2D LampArea;
    public PointLight2D lampLight;
    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f;
    private bool wasMoving = false;
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

    private bool IsInTown()
    {
        return GetTree().CurrentScene?.SceneFilePath.Contains("town") == true;
    }

    public override void _Ready()
    {
        respawnPosition = GlobalPosition;
        AddToGroup("player");
        lampLight = GetNode<PointLight2D>("LanternLight");
        LampArea = GetNode<Area2D>("LampArea");
        health = GetNode<Health>("Health");
        attackSound = GetNode<AudioStreamPlayer2D>("AttackSound");
        hurtSound = GetNode<AudioStreamPlayer2D>("HurtSound");
        grassSound = GetNode<AudioStreamPlayer2D>("GrassSound");
        dirtSound = GetNode<AudioStreamPlayer2D>("DirtSound");
        stoneSound = GetNode<AudioStreamPlayer2D>("StoneSound");

        health.Died += OnPlayerDied;
        health.HealthChanged += OnHealthChanged;

        CurrentDamage = baseDamage;
        if (LanternPivot != null)
            LanternPivot.Visible = false;
        if (LampArea != null)
            LampArea.Visible = false;

        var lanternState = GetNodeOrNull<LanternState>("/root/LanternState");
        if (lanternState != null && lanternState.HasLantern)
            EnableLantern();
        if (lanternState.HasLantern == false)
            lampLight.Energy = 0.2f;

        var armorState = GetNodeOrNull<ArmorState>("/root/ArmorState");
        if (armorState != null && armorState.HasArmor)
            RestoreArmor(armorState);

        if (RespawnState.LastPosition != Vector2.Zero)
            respawnPosition = RespawnState.LastPosition;

        CallDeferred(nameof(ApplySpawnPoint));
    }

    private void ApplySpawnPoint()
    {
        if (Checkpoint.ComingFromTown)
        {
            var spawnPoint = GetTree().GetFirstNodeInGroup("OutsideGate") as Node2D;
            if (spawnPoint != null)
                GlobalPosition = spawnPoint.GlobalPosition;
            Checkpoint.ComingFromTown = false;
        }
    }

    public void EnableLantern()
    {
        if (LanternPivot != null)
            LanternPivot.Visible = true;
        if (LampArea != null)
            LampArea.Visible = !IsInTown();
    }

    private void RestoreArmor(ArmorState armorState)
    {
        CurrentDamage = armorState.AttackBonus > 0 ? armorState.AttackBonus : baseDamage;
        health.ApplyHealthBonus(armorState.DurabilityBonus);

        if (armorState.ArmorSetIndex == 2)
            armorPrefix = "steel_";
        else if (armorState.ArmorSetIndex == 1)
            armorPrefix = "iron_";
    }

    public override void _PhysicsProcess(double delta)
    {
        PlayFootstepSound(delta);

        if (LockInput.inputLocked)
            return;

        GetInputDirection();
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        if (LockInput.inputLocked)
            return;

        AnimatePlayer();

        if (!IsInTown())
            Attack();
    }

    private void PlayFootstepSound(double delta)
    {
        bool isMoving = Velocity != Vector2.Zero;

        if (!isMoving)
        {
            footstepTimer = footstepInterval;
            wasMoving = false;
            return;
        }

        if (grassLayer == null) grassLayer = GetTree().GetFirstNodeInGroup("GrassLayer") as TileMapLayer;
        if (dirtLayer == null) dirtLayer = GetTree().GetFirstNodeInGroup("DirtLayer") as TileMapLayer;
        if (stoneLayer == null) stoneLayer = GetTree().GetFirstNodeInGroup("StoneLayer") as TileMapLayer;

        if (!wasMoving)
        {
            wasMoving = true;
            footstepTimer = footstepInterval;
            PlayCurrentSurface();
            return;
        }

        footstepTimer -= (float)delta;
        if (footstepTimer > 0)
            return;

        footstepTimer = footstepInterval;
        PlayCurrentSurface();
    }

    private void PlayCurrentSurface()
    {
        if (grassLayer != null && grassLayer.TileSet != null)
        {
            Vector2I tilePos = grassLayer.LocalToMap(grassLayer.ToLocal(GlobalPosition));
            if (grassLayer.GetCellSourceId(tilePos) != -1) { grassSound?.Play(); return; }
        }
        if (dirtLayer != null && dirtLayer.TileSet != null)
        {
            Vector2I tilePos = dirtLayer.LocalToMap(dirtLayer.ToLocal(GlobalPosition));
            if (dirtLayer.GetCellSourceId(tilePos) != -1) { dirtSound?.Play(); return; }
        }
        if (stoneLayer != null && stoneLayer.TileSet != null)
        {
            Vector2I tilePos = stoneLayer.LocalToMap(stoneLayer.ToLocal(GlobalPosition));
            if (stoneLayer.GetCellSourceId(tilePos) != -1) { stoneSound?.Play(); return; }
        }
    }

    public void SetCheckpoint(Vector2 position)
    {
        respawnPosition = position;
        RespawnState.LastScene = GetTree().CurrentScene.SceneFilePath;
        RespawnState.LastPosition = position;
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
        Velocity = Vector2.Zero;
        health.Reset();
        LoadingScreen.NextScenePath = RespawnState.LastScene;
        GetTree().ChangeSceneToFile("res://scenes/death_screen.tscn");
        SetPhysicsProcess(true);
        SetProcess(true);
    }

    public void EquipArmorSet(float healthBonus, float flatDamage)
    {
        CurrentDamage = flatDamage;

        if (flatDamage >= 30f)
            armorPrefix = "steel_";
        else if (flatDamage >= 15f)
            armorPrefix = "iron_";

        health.ApplyHealthBonus(healthBonus);

        var armorState = GetNodeOrNull<ArmorState>("/root/ArmorState");
        if (armorState != null)
        {
            armorState.HasArmor = true;
            armorState.DurabilityBonus = healthBonus;
            armorState.AttackBonus = flatDamage;
            armorState.ArmorSetIndex = flatDamage >= 30f ? 2 : 1;
        }
    }

    private void OnHealthChanged(float current, float max)
    {
        if (healthBar == null)
            healthBar = GetTree().GetFirstNodeInGroup("HealthBar") as HealthBar;

        if (healthBar == null) return;

        healthBar.UpdateHealth(current, max);

        if (current < max)
        {
            hurtSound?.Play();
            Hurt?.Play("Hurt");
        }
    }

    private Vector2 GetInputDirection()
    {
        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
        Velocity = inputDirection * Speed;

        if (inputDirection != Vector2.Zero)
        {
            lastPosition = inputDirection;

            if (Mathf.Abs(inputDirection.X) > Mathf.Abs(inputDirection.Y))
                currentPlayerDirection = inputDirection.X > 0 ? PlayerEnumDirection.Right : PlayerEnumDirection.Left;
            else
                currentPlayerDirection = inputDirection.Y > 0 ? PlayerEnumDirection.Down : PlayerEnumDirection.Up;
        }

        return inputDirection;
    }

    public void AnimatePlayer()
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
        if (Input.IsActionJustPressed("attack") && !isAttacking)
            SpawnAttack();
    }

    private void OnAttackFinished()
    {
        isAttacking = false;
    }
}
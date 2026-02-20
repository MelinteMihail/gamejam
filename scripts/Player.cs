using Godot;
using System.Collections.Generic;

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
    private AudioStreamPlayer2D dirtSound;
    private AudioStreamPlayer2D stoneSound;
    private TileMapLayer grassLayer;
    private TileMapLayer dirtLayer;
    private TileMapLayer stoneLayer;
    private Health health;
    private Area2D LampArea;
    private List<Health> enemiesInLamp = new();

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

    public override void _Ready()
    {
        respawnPosition = GlobalPosition;
        AddToGroup("player");
        LampArea = GetNode<Area2D>("LampArea");
        health = GetNode<Health>("Health");
        attackSound = GetNode<AudioStreamPlayer2D>("AttackSound");
        hurtSound = GetNode<AudioStreamPlayer2D>("HurtSound");
        grassSound = GetNode<AudioStreamPlayer2D>("GrassSound");
        dirtSound = GetNode<AudioStreamPlayer2D>("DirtSound");
        stoneSound = GetNode<AudioStreamPlayer2D>("StoneSound");

        health.Died += OnPlayerDied;
        health.HealthChanged += OnHealthChanged;

        LampArea.BodyEntered += OnBodyEnteredLampArea;
        LampArea.BodyExited += OnBodyExitedLampArea;

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
                enemyHealth.TakeDamage(actualDamage);
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
        if (grassLayer != null)
        {
            Vector2I tilePos = grassLayer.LocalToMap(grassLayer.ToLocal(GlobalPosition));
            if (grassLayer.GetCellSourceId(tilePos) != -1) { grassSound?.Play(); return; }
        }
        if (dirtLayer != null)
        {
            Vector2I tilePos = dirtLayer.LocalToMap(dirtLayer.ToLocal(GlobalPosition));
            if (dirtLayer.GetCellSourceId(tilePos) != -1) { dirtSound?.Play(); return; }
        }
        if (stoneLayer != null)
        {
            Vector2I tilePos = stoneLayer.LocalToMap(stoneLayer.ToLocal(GlobalPosition));
            if (stoneLayer.GetCellSourceId(tilePos) != -1) { stoneSound?.Play(); return; }
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
                enemiesInLamp.Remove(enemyHealth);
        }
    }

    private void OnHealthChanged(float current, float max)
    {
        if (healthBar == null)
            healthBar = GetTree().GetFirstNodeInGroup("HealthBar") as HealthBar;

        healthBar?.UpdateHealth(current, max);
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
                currentPlayerDirection = inputDirection.X > 0 ? PlayerEnumDirection.Right : PlayerEnumDirection.Left;
            else
                currentPlayerDirection = inputDirection.Y > 0 ? PlayerEnumDirection.Down : PlayerEnumDirection.Up;
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
}
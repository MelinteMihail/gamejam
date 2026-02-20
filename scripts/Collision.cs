using Godot;
using System;
using System.Collections.Generic;

public partial class Collision : Area2D
{
    [Export]
    public float DamagePerSecond = 10.0f;
    private List<Health> bodiesInside = new();
    public bool isAttacking = false;
    [Signal]
    public delegate void AttackStartedEventHandler();
    private float attackCooldown = 0f;
    private float attackCooldownTime = 0f;
    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
        this.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (bodiesInside.Count == 0)
            return;

        if (attackCooldown > 0)
        {
            attackCooldown -= (float)delta;
            return;
        }

        if (!isAttacking)
        {
            isAttacking = true;
            EmitSignal("AttackStarted");
        }
    }
    public void ApplyDamageOnce()
    {
        foreach (var health in bodiesInside)
        {
            health.TakeDamage(DamagePerSecond);
        }

        isAttacking = false;
        attackCooldown = attackCooldownTime;
    }
    public void ResetCooldown()
    {
        attackCooldown = attackCooldownTime;
    }


    private void OnBodyEntered(Node2D body)
    {
        var health = body.GetNodeOrNull<Health>("Health");
        GD.Print($"Body entered: {body.Name}, group player: {body.IsInGroup("player")}, health: {health != null}");
        if (health != null)
            bodiesInside.Add(health);
    }

    private void OnBodyExited(Node2D body)
    {
        var health = body.GetNodeOrNull<Health>("Health");
        GD.Print("Body exited: " + body.Name);
        if (health != null)
        {
            bodiesInside.Remove(health);
        }
    }
}

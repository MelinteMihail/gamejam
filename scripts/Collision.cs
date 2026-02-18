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
    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
        this.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (bodiesInside.Count == 0)
            return;

        if (!isAttacking)
        {
            isAttacking = true;
            EmitSignal("AttackStarted");
        }
        else
        {
            float damage = DamagePerSecond * (float)delta;

            foreach (var health in bodiesInside)
            {
                health.TakeDamage(damage);
            }
        }
    }


    private void OnBodyEntered(Node2D body)
    {
        var health = body.GetNodeOrNull<Health>("Health");

        if (health != null)
        {
            bodiesInside.Add(health);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        var health = body.GetNodeOrNull<Health>("Health");

        if (health != null)
        {
            bodiesInside.Remove(health);
        }
    }
}

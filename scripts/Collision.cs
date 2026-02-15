using Godot;
using System;
using System.Collections.Generic;

public partial class Collision : Area2D
{
    [Export]
    public float DamagePerSecond = 10.0f;

    private List<Health> bodiesInside = new();
    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
        this.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        float damage = DamagePerSecond * (float) delta;

        foreach (var health in bodiesInside)
        {
            health.TakeDamage(damage);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        var health = body.GetNodeOrNull<Health>("Health");

        GD.Print("Body entered: " + body.Name);

        if (health != null)
        {
            bodiesInside.Add(health);
        }
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

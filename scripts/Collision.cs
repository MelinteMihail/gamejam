using Godot;
using System;

public partial class Collision : Area2D
{
    [Export]
    public float DamagePerSecond = 10.0f;
    public Player playerInside = null;
    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
        this.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(playerInside != null)
        {
            float damage = DamagePerSecond * (float)delta;
            playerInside.TakeDamage(damage);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        GD.Print("Body entered: " + body.Name);

        if(body is Player player)
            playerInside = player;
        
    }

    private void OnBodyExited(Node2D body)
    {
        GD.Print("Body exited: " + body.Name);

        if(body is Player player && playerInside == player)
            playerInside = null;
    }
}

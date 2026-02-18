using Godot;

public partial class Projectile : Area2D
{
    [Export]
    public float Speed = 200f;
    [Export]
    public float Damage = 10f;

    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir;
        Rotation = dir.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += direction * Speed * (float)delta;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            var health = body.GetNodeOrNull<Health>("Health");
            health?.TakeDamage(Damage);
            QueueFree();
        }
    }

}
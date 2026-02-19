using Godot;

public partial class Boss : CharacterBody2D
{
    [Export] public Label nameLabel;
    [Export] public TextureProgressBar healthBar;

    private Health health;

    public override void _Ready()
    {
        AddToGroup("enemy");
        health = GetNode<Health>("Health");
        health.Died += OnBossDied;
        health.HealthChanged += OnHealthChanged;

        nameLabel.Text = "Boss Name";
        healthBar.MinValue = 0;
        healthBar.MaxValue = health.maxHealth;
        healthBar.Value = health.CurrentHealth;
    }

    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }

    private void OnHealthChanged(float current, float max)
    {
        healthBar.Value = current;
        GD.Print($"Boss health: {current}/{max}");
    }

    private void OnBossDied()
    {
        GD.Print("Boss died!");
        QueueFree();
    }
}
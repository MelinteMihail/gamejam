using Godot;
using System;

public partial class Health : Node
{
    [Signal]
    public delegate void HealthChangedEventHandler(float currentHealth, float max);
    [Signal]
    public delegate void DiedEventHandler();

    [Export]
	public int maxHealth = 100;

	private float currentHealth;
    private bool isDead = false;

    public override void _Ready()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) 
            return;

        currentHealth -= damage;

        int currentHealthInt = (int) currentHealth;

        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);

        if (currentHealthInt <= 0 && !isDead)
        {
            isDead = true;
            EmitSignal(SignalName.Died);
            Die();
        }
    }

    private void Die()
    {
        QueueFree();
    }

}

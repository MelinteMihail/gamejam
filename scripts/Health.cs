using Godot;
public partial class Health : Node
{
    [Signal]
    public delegate void HealthChangedEventHandler(float currentHealth, float max);
    [Signal]
    public delegate void DiedEventHandler();
    [Export]
    public int maxHealth = 100;
    private float currentHealth;
    public float CurrentHealth => currentHealth;
    private bool isDead = false;
    public override void _Ready()
    {
        currentHealth = maxHealth;
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);
    }
    public void TakeDamage(float damage)
    {
        if (isDead)
            return;
        currentHealth -= damage;
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            EmitSignal(SignalName.Died);
        }
    }
    public void Reset()
    {
        isDead = false;
        currentHealth = maxHealth;
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);
    }
}
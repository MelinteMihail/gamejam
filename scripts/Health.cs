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
    private float effectiveMaxHealth;
    public float CurrentHealth => currentHealth;

    private bool isDead = false;

    public override void _Ready()
    {
        effectiveMaxHealth = maxHealth;
        currentHealth = effectiveMaxHealth;
        EmitSignal(SignalName.HealthChanged, currentHealth, effectiveMaxHealth);
    }

    public void ApplyHealthBonus(float percentBonus)
    {
        float bonus = maxHealth * (percentBonus / 100f);
        effectiveMaxHealth = maxHealth + bonus;
        currentHealth = effectiveMaxHealth;
        EmitSignal(SignalName.HealthChanged, currentHealth, effectiveMaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        GD.Print($"TakeDamage called on: {GetParent().Name}, damage: {damage}");
        currentHealth -= damage;
        EmitSignal(SignalName.HealthChanged, currentHealth, effectiveMaxHealth);

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            EmitSignal(SignalName.Died);
        }
    }

    public void Reset()
    {
        isDead = false;
        currentHealth = effectiveMaxHealth;
        EmitSignal(SignalName.HealthChanged, currentHealth, effectiveMaxHealth);
    }
}
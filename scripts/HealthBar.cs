using Godot;

public partial class HealthBar : Control
{
    private Control fillClip;
    private float maxClipWidth = 0f;
    private float fillStartX = 50f; 

    public override void _Ready()
    {
        fillClip = GetNode<Control>("FillClip");
        maxClipWidth = fillClip.Size.X;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        float percent = Mathf.Clamp(currentHealth / maxHealth, 0f, 1f);
        float newWidth = currentHealth <= 0 ? 0f : maxClipWidth * percent;

        fillClip.Size = new Vector2(newWidth, fillClip.Size.Y);
        fillClip.Visible = (int) currentHealth > 0;
    }
}

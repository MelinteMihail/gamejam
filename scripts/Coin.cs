using Godot;
using System;

public partial class Coin : Control
{
	public static Coin Instance { get; private set; }

	private Label counter;
	private int coinAmount = 0;

	public override void _Ready()
	{
		Instance = this;
		counter = GetNode<Label>("HBoxContainer/Counter");
		UpdateDisplay();
	}

    public override void _Process(double delta)
    {
        bool inTown = GetTree().CurrentScene?.SceneFilePath.Contains("town") == true;
        Visible = inTown;
    }

	public void AddCoins(int amount)
	{
		coinAmount += amount;
		UpdateDisplay();
		GD.Print($"Added {amount} coins. Total: {coinAmount}");
    }

	public void RemoveCoins(int amount)
	{
		coinAmount -= amount;
		UpdateDisplay();
		GD.Print($"Removed {amount} coins. Total: {coinAmount}");
    }

	public int GetCoinAmount() 
	{ 
		return coinAmount; 
	}

    private void UpdateDisplay()
	{
		if (counter != null)
			counter.Text = $"{coinAmount}";
    }
}

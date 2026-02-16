using Godot;
using System;

public partial class Npc : CharacterBody2D
{
	private Label interactableText;
	private Area2D interactableArea;
	private bool playerNearby = false;

	public override void _Ready()
	{
		interactableText = GetNode<Label>("Interact Text");
		interactableArea = GetNode<Area2D>("Interactable Area");

        interactableArea.BodyEntered += OnBodyEntered;
		interactableArea.BodyExited += OnBodyExited;

		interactableText.Visible = false;
    }

	private void OnBodyEntered(Node2D body)
	{
		if(body is Player player)
		{
			playerNearby = true;
            interactableText.Visible = true;
		}
    }

	private void OnBodyExited(Node2D body)
	{
		if(body is Player player)
		{
			playerNearby = false;
            interactableText.Visible = false;
		}
    }

    public override void _Process(double delta)
	{
		if (playerNearby && Input.IsActionJustPressed("interact"))
		{
			OnInteract();
        }
    }

	private void OnInteract()
	{
		var quest = GetNodeOrNull<Quest>("Quest");

		if (quest != null)
		{
			interactableText.Visible = false;
			quest.StartQuest();
        }
	}

	public void ShowInteractText()
	{
		interactableText.Visible = true;
    }
}

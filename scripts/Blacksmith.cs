using Godot;
using System;

public partial class Blacksmith : CharacterBody2D
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
        if (body is Player player)
        {
            playerNearby = true;
            interactableText.Visible = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player player)
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
        var shop = GetNodeOrNull<BlacksmithShop>("/root/game/UI/BlacksmithShop");

        if (shop != null)
        {
            interactableText.Visible = false;
            LockInput.inputLocked = true;
            shop.OpenShop();
        }
    }

    public void ShowInteractText()
    {
        interactableText.Visible = true;
    }
}

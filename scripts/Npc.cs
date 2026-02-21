using Godot;
using System;

public partial class Npc : CharacterBody2D
{
    public enum NpcType
    {
        Herb,
        Mob
    }

    [Export]
    public NpcType Type = NpcType.Herb;

    private Label interactableText;
    private Area2D interactableArea;
    private AnimatedSprite2D sprite;
    private bool playerNearby = false;
    private float interactCooldown = 0f;

    public override void _Ready()
    {
        interactableText = GetNode<Label>("Interact Text");
        interactableArea = GetNode<Area2D>("Interactable Area");
        interactableArea.BodyEntered += OnBodyEntered;
        interactableArea.BodyExited += OnBodyExited;
        interactableText.Visible = false;
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        switch (Type)
        {
            case NpcType.Herb: sprite.Play("herb_idle"); break;
            case NpcType.Mob: sprite.Play("mob_idle"); break;
        }

        var dialogue = GetTree().GetFirstNodeInGroup("Dialogue") as Dialogue;
        if (dialogue != null)
            dialogue.DialogueClosed += OnDialogueClosed;
    }

    private void OnDialogueClosed()
    {
        interactCooldown = 0.3f;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            playerNearby = true;
            interactableText.Visible = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player)
        {
            playerNearby = false;
            interactableText.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        if (interactCooldown > 0)
            interactCooldown -= (float)delta;

        if (playerNearby && Input.IsActionJustPressed("interact") && interactCooldown <= 0)
            OnInteract();
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
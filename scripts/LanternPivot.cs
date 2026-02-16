using Godot;
using System;
using System.ComponentModel.Design;

public partial class LanternPivot : Node2D
{
    private CharacterBody2D player;

    [Export] private float swingAmount = 0.003f;
    [Export] private float swingSmoothness = 8f;
    [Export] private Vector2 offset = new Vector2(0.1f, 0.1f);
    [Export] private Player playerClass;
    [Export] private AnimatedSprite2D lanternSprite; 
    private Vector2 currentOffset;
    private Vector2 Velocity;
    float targetRotation;
    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;
        Position = offset with { X = -offset.X };
    }
    public override void _Process(double delta)
    {
        if (player == null)
            return;
        Velocity = player.Velocity;
        targetRotation = Velocity.X * swingAmount;
        Rotation = Mathf.Lerp(Rotation, targetRotation, swingSmoothness * (float)delta);
        Player.EnumDirection currentDir = playerClass.GetCurrentDirection();

        float horizontalOffset = currentDir == Player.EnumDirection.Up ? offset.X :
              currentDir == Player.EnumDirection.Down ? -offset.X : 0;

        if (currentDir != Player.EnumDirection.None)
        {
            currentOffset = new Vector2(horizontalOffset, offset.Y);
            Position = currentOffset;
        }
        if (currentDir == Player.EnumDirection.Left)
        {
            lanternSprite.ZIndex = -2;
        }
        else if (currentDir == Player.EnumDirection.Down)
        {
            lanternSprite.ZIndex = -2;
        }
        else if (currentDir != Player.EnumDirection.None) lanternSprite.ZIndex = 2;

    }

}
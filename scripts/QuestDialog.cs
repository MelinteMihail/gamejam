using Godot;
using System;

public partial class QuestDialog : Control
{
    [Signal]
    public delegate void OptionSelectedEventHandler(int optionIndex);

    [Signal]
    public delegate void DialogClosedEventHandler();

    private Label titleLabel;
    private Label textLabel;
    private VBoxContainer optionsContainer;
    private PanelContainer panel;

    public override void _Ready()
    {
        panel = GetNode<PanelContainer>("Panel");
        titleLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/Title");
        textLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/Text");
        optionsContainer = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer/Options");

        Hide();
    }

    public void ShowDialog(string title, string text, string[] options)
    {

        titleLabel.Text = title;
        textLabel.Text = text;

        while (optionsContainer.GetChildCount() > 0)
        {
            var child = optionsContainer.GetChild(0);
            optionsContainer.RemoveChild(child);
            child.QueueFree();
        }

        CallDeferred(MethodName.AddButtons, options);
    }

    private void AddButtons(string[] options)
    {
        for (int i = 0; i < options.Length; i++)
        {
            Button optionButton = new();
            optionButton.Text = options[i];
            int index = i;

            optionButton.Pressed += () => OnOptionPressed(index);
            optionsContainer.AddChild(optionButton);
        }

        Show();
    }

    private void OnOptionPressed(int index)
    {
        EmitSignal(SignalName.OptionSelected, index);
        Hide();
        EmitSignal(SignalName.DialogClosed);
    }
}
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Dialogue : Control
{
    [Signal]
    public delegate void DialogueClosedEventHandler();

    private PanelContainer panel;
    private Label speakerLabel;
    private Label textLabel;
    private Label promptLabel;

    private bool _justAdvanced = false;


    private Queue<(string speaker, string text)> dialogueQueue = new();

    public override void _Ready()
    {
        panel = GetNode<PanelContainer>("Panel");
        speakerLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/Speaker");
        textLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/Text");
        promptLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/Prompt");
        
        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible)
            return;
        if (Input.IsActionJustPressed("interact"))
        {
            if (_justAdvanced)
            {
                _justAdvanced = false;
                return;
            }
            Advance();
        }
    }


    public void ShowDialogue((string speaker, string text)[] lines)
    {
        dialogueQueue.Clear();

        foreach (var line in lines)
            dialogueQueue.Enqueue(line);
        
        LockInput.inputLocked = true;
        
        _justAdvanced = true; // block the first _Input from also firing
        Advance();
    }

    private void Advance()
    {
        if (dialogueQueue.Count == 0)
        {
            GD.Print("Queue empty, closing dialogue");
            Hide();
            LockInput.inputLocked = false;
            EmitSignal(SignalName.DialogueClosed);
            return;
        }
        var (speaker, text) = dialogueQueue.Dequeue();
        GD.Print($"Showing line: {speaker}: {text}");
        speakerLabel.Text = speaker;
        speakerLabel.Visible = !string.IsNullOrEmpty(speaker);
        textLabel.Text = text;
        promptLabel.Text = dialogueQueue.Count == 0 ? "Press [E] to close" : "Press [E] to continue";
        Show();
    }
}

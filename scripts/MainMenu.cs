using Godot;
using System;

public partial class MainMenu : Control
{
    private ColorRect fadeOverlay;

    public override void _Ready()
    {
        fadeOverlay = GetNode<ColorRect>("FadeOverlay");

        var playButton = GetNode<Button>("CenterContainer/VBoxContainer/PlayButton");
        var quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");

        playButton.Text = "Play";
        quitButton.Text = "Quit";

        StyleButton(playButton);
        StyleButton(quitButton);

        playButton.Pressed += OnPlayPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    private void StyleButton(Button button)
    {
        button.CustomMinimumSize = new Vector2(200, 50);
        button.AddThemeFontSizeOverride("font_size", 18);

        var normal = new StyleBoxFlat();
        normal.BgColor = new Color(0.1f, 0.1f, 0.15f);
        normal.BorderColor = new Color(0.6f, 0.5f, 0.8f);
        normal.BorderWidthLeft = 2;
        normal.BorderWidthRight = 2;
        normal.BorderWidthTop = 2;
        normal.BorderWidthBottom = 2;
        normal.CornerRadiusTopLeft = 6;
        normal.CornerRadiusTopRight = 6;
        normal.CornerRadiusBottomLeft = 6;
        normal.CornerRadiusBottomRight = 6;
        button.AddThemeStyleboxOverride("normal", normal);

        var hover = new StyleBoxFlat();
        hover.BgColor = new Color(0.25f, 0.2f, 0.35f);
        hover.BorderColor = new Color(0.8f, 0.7f, 1f);
        hover.BorderWidthLeft = 2;
        hover.BorderWidthRight = 2;
        hover.BorderWidthTop = 2;
        hover.BorderWidthBottom = 2;
        hover.CornerRadiusTopLeft = 6;
        hover.CornerRadiusTopRight = 6;
        hover.CornerRadiusBottomLeft = 6;
        hover.CornerRadiusBottomRight = 6;
        button.AddThemeStyleboxOverride("hover", hover);

        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
        button.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f));
    }

    private async void OnPlayPressed()
    {
        fadeOverlay.MouseFilter = Control.MouseFilterEnum.Stop;

        var tween = CreateTween();
        tween.TweenProperty(fadeOverlay, "color:a", 1.0f, 1.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.In);

        await ToSignal(tween, Tween.SignalName.Finished);

        GetTree().ChangeSceneToFile("res://scenes/cutscene.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
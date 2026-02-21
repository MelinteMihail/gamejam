using Godot;
using System;

public partial class BlacksmithShop : Control
{
    [Signal]
    public delegate void ShopClosedEventHandler();

    [Export]
    public int ArmorSet1Cost = 3;
    [Export]
    public string ArmorSet1IconPath = "res://assets/extras/Iron_Armor_Weapon.png";

    [Export]
    public int ArmorSet2Cost = 6;
    [Export]
    public string ArmorSet2IconPath = "res://assets/extras/Steel_Armor_Weapon.png";

    private const float IronHealthBonus = 15f;
    private const float IronDamage = 20f;

    private const float SteelHealthBonus = 25f;
    private const float SteelDamage = 30f;

    private Button armorSet1Button;
    private Button armorSet2Button;
    private Button closeButton;
    private Label armorSet1DurLabel;
    private Label armorSet1AtkLabel;
    private Label armorSet2DurLabel;
    private Label armorSet2AtkLabel;
    private Label armorSet1PriceLabel;
    private Label armorSet2PriceLabel;
    private Player player;

    private bool armorSet1Purchased = false;
    private bool armorSet2Purchased = false;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        OffsetLeft = 0;
        OffsetTop = 0;
        OffsetRight = 0;
        OffsetBottom = 0;

        BuildShopUI();

        var armorState = GetNodeOrNull<ArmorState>("/root/ArmorState");
        if (armorState != null && armorState.HasArmor)
        {
            if (armorState.ArmorSetIndex == 1) armorSet1Purchased = true;
            if (armorState.ArmorSetIndex == 2) armorSet2Purchased = true;
        }

        Hide();
    }

    private bool CanBuyArmor()
    {
        return QuestChain.Instance?.CurrentStage == QuestChain.StoryStage.BuyArmor;
    }

    private void BuildShopUI()
    {
        var center = new CenterContainer();
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(600, 450);
        center.AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        panel.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 20);
        margin.AddChild(vbox);

        var titleLabel = new Label();
        titleLabel.Text = "Blacksmith - Armor Sets";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.843f, 0));
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(titleLabel);

        var optionsContainer = new HBoxContainer();
        optionsContainer.AddThemeConstantOverride("separation", 30);
        optionsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddChild(optionsContainer);

        var armorSet1 = CreateArmorSetOption(
            "Iron Armor", ArmorSet1IconPath,
            $"+{IronHealthBonus}% HP", $"{IronDamage} ATK", ArmorSet1Cost,
            OnArmorSet1Purchase,
            out armorSet1Button, out armorSet1DurLabel, out armorSet1AtkLabel, out armorSet1PriceLabel
        );
        optionsContainer.AddChild(armorSet1);

        var armorSet2 = CreateArmorSetOption(
            "Steel Armor", ArmorSet2IconPath,
            $"+{SteelHealthBonus}% HP", $"{SteelDamage} ATK", ArmorSet2Cost,
            OnArmorSet2Purchase,
            out armorSet2Button, out armorSet2DurLabel, out armorSet2AtkLabel, out armorSet2PriceLabel
        );
        optionsContainer.AddChild(armorSet2);

        closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.CustomMinimumSize = new Vector2(150, 40);
        closeButton.Pressed += OnClosePressed;

        var closeContainer = new CenterContainer();
        closeContainer.AddChild(closeButton);
        vbox.AddChild(closeContainer);
    }

    private PanelContainer CreateArmorSetOption(
        string title, string iconPath,
        string durText, string atkText, int cost,
        Action onPurchase,
        out Button button, out Label durLabel, out Label atkLabel, out Label priceLabel)
    {
        var optionPanel = new PanelContainer();
        optionPanel.CustomMinimumSize = new Vector2(250, 350);

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.15f, 0.15f, 0.2f);
        styleBox.BorderColor = new Color(0.5f, 0.5f, 0.5f);
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.CornerRadiusTopLeft = 10;
        styleBox.CornerRadiusTopRight = 10;
        styleBox.CornerRadiusBottomLeft = 10;
        styleBox.CornerRadiusBottomRight = 10;
        optionPanel.AddThemeStyleboxOverride("panel", styleBox);

        var optionMargin = new MarginContainer();
        optionMargin.AddThemeConstantOverride("margin_left", 15);
        optionMargin.AddThemeConstantOverride("margin_right", 15);
        optionMargin.AddThemeConstantOverride("margin_top", 15);
        optionMargin.AddThemeConstantOverride("margin_bottom", 15);
        optionPanel.AddChild(optionMargin);

        var optionVBox = new VBoxContainer();
        optionVBox.AddThemeConstantOverride("separation", 15);
        optionMargin.AddChild(optionVBox);

        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        optionVBox.AddChild(titleLabel);

        var iconContainer = new CenterContainer();
        if (!string.IsNullOrEmpty(iconPath))
        {
            var iconTexture = new TextureRect();
            iconTexture.Texture = GD.Load<Texture2D>(iconPath);
            iconTexture.CustomMinimumSize = new Vector2(120, 120);
            iconTexture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            iconTexture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            iconContainer.AddChild(iconTexture);
        }
        else
        {
            var iconRect = new ColorRect();
            iconRect.Color = new Color(0.3f, 0.3f, 0.3f);
            iconRect.CustomMinimumSize = new Vector2(120, 120);
            iconContainer.AddChild(iconRect);
        }
        optionVBox.AddChild(iconContainer);

        var statsVBox = new VBoxContainer();
        statsVBox.AddThemeConstantOverride("separation", 8);
        optionVBox.AddChild(statsVBox);

        durLabel = new Label();
        durLabel.Text = durText;
        durLabel.HorizontalAlignment = HorizontalAlignment.Center;
        durLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.8f, 1f));
        durLabel.AddThemeFontSizeOverride("font_size", 18);
        statsVBox.AddChild(durLabel);

        atkLabel = new Label();
        atkLabel.Text = atkText;
        atkLabel.HorizontalAlignment = HorizontalAlignment.Center;
        atkLabel.AddThemeColorOverride("font_color", new Color(1f, 0.5f, 0.5f));
        atkLabel.AddThemeFontSizeOverride("font_size", 18);
        statsVBox.AddChild(atkLabel);

        var spacer = new Control();
        spacer.SizeFlagsVertical = SizeFlags.ExpandFill;
        optionVBox.AddChild(spacer);

        priceLabel = new Label();
        priceLabel.Text = $"{cost} Gold";
        priceLabel.HorizontalAlignment = HorizontalAlignment.Center;
        priceLabel.AddThemeColorOverride("font_color", new Color(1, 0.843f, 0));
        priceLabel.AddThemeFontSizeOverride("font_size", 16);
        optionVBox.AddChild(priceLabel);

        button = new Button();
        button.Text = "Buy";
        button.CustomMinimumSize = new Vector2(0, 40);
        button.Pressed += () => onPurchase();
        optionVBox.AddChild(button);

        return optionPanel;
    }

    public void OpenShop()
    {
        player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player == null)
        {
            GD.PrintErr("BlacksmithShop: Could not find Player");
            return;
        }

        UpdateShopState();
        Show();
    }

    private void UpdateShopState()
    {
        if (Coin.Instance == null || player == null)
            return;

        int currentGold = Coin.Instance.GetCoinAmount();
        bool canBuy = CanBuyArmor();

        if (armorSet1Purchased)
        {
            armorSet1Button.Text = "EQUIPPED";
            armorSet1Button.Disabled = true;
            armorSet1DurLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            armorSet1AtkLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
        }
        else if (!canBuy)
        {
            armorSet1Button.Text = "Not Available";
            armorSet1Button.Disabled = true;
        }
        else
        {
            armorSet1Button.Disabled = currentGold < ArmorSet1Cost;
            armorSet1Button.Text = currentGold < ArmorSet1Cost ? "Not Enough Gold" : "Buy";
        }

        if (armorSet2Purchased)
        {
            armorSet2Button.Text = "EQUIPPED";
            armorSet2Button.Disabled = true;
            armorSet2DurLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            armorSet2AtkLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
        }
        else if (!canBuy)
        {
            armorSet2Button.Text = "Not Available";
            armorSet2Button.Disabled = true;
        }
        else
        {
            armorSet2Button.Disabled = currentGold < ArmorSet2Cost;
            armorSet2Button.Text = currentGold < ArmorSet2Cost ? "Not Enough Gold" : "Buy";
        }
    }

    private void OnArmorSet1Purchase()
    {
        if (Coin.Instance == null || player == null || armorSet1Purchased || !CanBuyArmor())
            return;

        if (Coin.Instance.GetCoinAmount() >= ArmorSet1Cost)
        {
            Coin.Instance.RemoveCoins(ArmorSet1Cost);
            player.EquipArmorSet(IronHealthBonus, IronDamage);
            armorSet1Purchased = true;
            QuestChain.Instance?.OnArmorBought();
            GD.Print("Purchased Iron Armor!");
            UpdateShopState();
        }
    }

    private void OnArmorSet2Purchase()
    {
        if (Coin.Instance == null || player == null || armorSet2Purchased || !CanBuyArmor())
            return;

        if (Coin.Instance.GetCoinAmount() >= ArmorSet2Cost)
        {
            Coin.Instance.RemoveCoins(ArmorSet2Cost);
            player.EquipArmorSet(SteelHealthBonus, SteelDamage);
            armorSet2Purchased = true;
            QuestChain.Instance?.OnArmorBought();
            GD.Print("Purchased Steel Armor!");
            UpdateShopState();
        }
    }

    private void OnClosePressed()
    {
        Hide();
        LockInput.inputLocked = false;
        EmitSignal(SignalName.ShopClosed);
    }

    public override void _Process(double delta)
    {
        if (Visible && Input.IsActionJustPressed("ui_cancel"))
            OnClosePressed();

        if (Visible)
            UpdateShopState();
    }
}
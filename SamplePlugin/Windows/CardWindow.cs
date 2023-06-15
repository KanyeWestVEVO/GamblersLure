using System;
using System.IO;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json.Linq;

namespace SamplePlugin.Windows;

public class CardWindow : Window, IDisposable
{
    private Plugin Plugin;

    public int cardValue = 0;

    private TextureWrap Card;
    public TextureWrap CardProperty
    {
        get
        {
            return Card;
        }
        set
        {
            if (Card == value)
                return;

            Card.Dispose();
            Card = value;
        }
    }

    public CardWindow(string windowName, Plugin plugin, TextureWrap card, Vector2 spawnPosition) : base(
        "Card",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar)
    {
        this.WindowName = windowName;

        BgAlpha = 0;
        this.Card = card;

        this.Size = new Vector2(120, 170);
        this.Position = spawnPosition;

        this.SizeCondition = ImGuiCond.None;
        this.PositionCondition = ImGuiCond.Appearing;

        this.Plugin = plugin;
        this.rng = new Random();
    }

    public void Dispose()
    {
        Card.Dispose();
    }

    public override void Draw()
    {
        ImGui.Image(this.Card.ImGuiHandle, new Vector2(this.Card.Width, this.Card.Height));
    }

    public void SummonCard()
    {
        var imagePath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "Cards/Back.png");
        var cardImg = Plugin.PluginInterface.UiBuilder.LoadImage(imagePath);

        CardProperty = cardImg;
    }

    public void SetCard(int value)
    {
        var imagePath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "Cards/" + value + ".png");
        var cardImg = Plugin.PluginInterface.UiBuilder.LoadImage(imagePath);

        CardProperty = cardImg;
        cardValue = value;
    }

    Random rng;

    public void RandomizeCard()
    {
        int randomCard = RandomizeValueExcept();
        Plugin.LeftCardWindow.SetCard(randomCard);
    }

    public int RandomizeValueExcept(int exceptionValue = -69)
    {
        int randomValue = rng.Next(10);
        if (randomValue >= exceptionValue || randomValue == 0) randomValue = (randomValue + 1) % 10;
        return randomValue;
    }

    public void RemoveCard()
    {
        var imagePath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "Cards/LiterallyNothing.png");
        var cardImg = Plugin.PluginInterface.UiBuilder.LoadImage(imagePath);

        CardProperty = cardImg;
    }

    // only reason this is different from SummonCard() is if later on i figure out how to make the animations i want
    public void FlipBackDown()
    {
        var imagePath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "Cards/Back.png");
        var cardImg = Plugin.PluginInterface.UiBuilder.LoadImage(imagePath);

        CardProperty = cardImg;
    }
}

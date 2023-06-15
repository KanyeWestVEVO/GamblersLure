using System;
using System.IO;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private TextureWrap Portrait;
    public TextureWrap PortraitProperty
    {
        get
        {
            return Portrait;
        }
        set
        {
            if (Portrait == value)
                return;

            Portrait.Dispose();
            Portrait = value;
        }
    }

    private Plugin Plugin;

    public MainWindow(Plugin plugin, TextureWrap portrait) : base(
        "Gambler's Lure", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoMove)
    {
        this.BgAlpha = 0;

        this.Size = new Vector2(545, 735);
        this.Position = new Vector2(69, 110);

        this.SizeCondition = ImGuiCond.None;
        this.PositionCondition = ImGuiCond.Appearing;

        this.Portrait = portrait;
        this.Plugin = plugin;
    }

    public void Dispose()
    {
        this.Portrait.Dispose();
    }

    public override void Draw()
    {
        ImGui.Image(this.Portrait.ImGuiHandle, new Vector2(this.Portrait.Width, this.Portrait.Height));
    }

    public void SetPortrait(string value)
    {
        string imagePath = null;
        TextureWrap portraitImg = null;

        imagePath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, value);
        portraitImg = Plugin.PluginInterface.UiBuilder.LoadImage(imagePath);

        PortraitProperty = portraitImg;
    }
}

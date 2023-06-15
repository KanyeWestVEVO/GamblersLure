using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using static SamplePlugin.Plugin;

namespace SamplePlugin.Windows;

public class ChoiceWindow : Window, IDisposable
{
    private Plugin Plugin;
    public List<Choice> activeChoices = new List<Choice>();

    private string dialogueTxt = "Lorem ipsum";
    public string DialogueTxt
    {
        get
        {
            return dialogueTxt;
        }
        set
        {
            dialogueTxt = value;
        }
    }

    public ChoiceWindow(Plugin plugin) : base(
        "Dealer",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoMove)
    {
        this.Size = new Vector2(545, 90);
        this.Position = new Vector2(69, 734);

        this.SizeCondition = ImGuiCond.None;
        this.PositionCondition = ImGuiCond.Appearing;

        this.Plugin = plugin;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.TitleBg, new Vector4(0.8f, 0.06f, 0.05f, 0.69f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.8f, 0.06f, 0.05f, 0.69f));
    }

    public override void Draw()
    {
        ImGui.TextUnformatted($"{DialogueTxt}");

        if (activeChoices.Count > 0)
            ShowChoices();
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(2);
    }

    private void ShowChoices()
    {
        ImGui.Spacing();

        foreach (Choice choice in activeChoices)
        {
            ImGui.SameLine();

            if (ImGui.Button(choice.choiceTxt))
            {
                HandleChoice(choice.nextState, choice.choiceType);
            }
        }
    }

    private void HandleChoice(string nextState, ChoiceType choiceType = ChoiceType.None)
    {
        if (nextState == "EndGame")
        {
            IsOpen = false;
        }
        else if (nextState != null && !String.IsNullOrEmpty(nextState))
        {
            // special cases
            switch (nextState)
            {
                case "Intro":
                    Plugin.PlayIntro();
                    break;
                case "Help":
                    Plugin.OnHelpCommand(null, null);
                    break;
            }

            // regular cases
            Dialogue nextDialogueToLoad = null;

            foreach (Dialogue dialogue in Plugin.dialogues)
            {
                nextDialogueToLoad = dialogue.stateId == nextState ? dialogue : nextDialogueToLoad;
            }

            if (nextDialogueToLoad != null)
            {
                Plugin.DataBind(nextDialogueToLoad);
            }
        }
        else
        {
            // if nextState is null, assume grading of cards
            // first, set a value for the right card
            int rightCardValue = Plugin.RightCardWindow.RandomizeValueExcept(Plugin.LeftCardWindow.cardValue);
            Plugin.RightCardWindow.SetCard(rightCardValue);

            // then compare the two
            ChoiceType correctChoice = Plugin.RightCardWindow.cardValue > Plugin.LeftCardWindow.cardValue ? ChoiceType.Higher : ChoiceType.Lower;

            // result
            if (correctChoice == choiceType)
            {
                Plugin.YouWin();
            }
            else
            {
                Plugin.YouLose();
            }
        }
    }

    public override void OnOpen()
    {
        // data and initial game state setup
        Plugin.ConstructDatabase();
        Plugin.DataBind(Plugin.dialogues[0]);
    }

    public override void OnClose()
    {
        Plugin.MainWindow.IsOpen = false;
        Plugin.ConfigWindow.IsOpen = false;
        Plugin.LeftCardWindow.IsOpen = false;
        Plugin.RightCardWindow.IsOpen = false;
        Plugin.LeftCardWindow.RemoveCard();
        Plugin.RightCardWindow.RemoveCard();
    }
}

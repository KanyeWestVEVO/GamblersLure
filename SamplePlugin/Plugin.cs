using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using SamplePlugin.Windows;
using Dalamud.Interface;
using System;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using System.Numerics;
using Dalamud.Logging;
using static SamplePlugin.Plugin;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Gambler's Lure";
        private const string CommandName = "/glure";
        private const string SettingsCommandName = "/glurehelp";
        private const string ConfigCommandName = "/gluresettings";

        public DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("SamplePlugin");

        public ConfigWindow ConfigWindow { get; init; }
        public MainWindow MainWindow { get; init; }
        public ChoiceWindow ChoiceWindow { get; init; }
        public CardWindow LeftCardWindow { get; init; }
        public CardWindow RightCardWindow { get; init; }

        public List<Dialogue> dialogues = new List<Dialogue>();

        [PluginService] public static Framework Framework { get; private set; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Portraits/triumphant.png");
            var portraitImg = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, portraitImg);
            ChoiceWindow = new ChoiceWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(ChoiceWindow);

            var imagePath2 = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Cards/LiterallyNothing.png");
            var cardImg = this.PluginInterface.UiBuilder.LoadImage(imagePath2);

            LeftCardWindow = new CardWindow("LeftCard", this, cardImg, new Vector2(75, 375));
            RightCardWindow = new CardWindow("RightCard", this, cardImg, new Vector2(470, 375));

            WindowSystem.AddWindow(LeftCardWindow);
            WindowSystem.AddWindow(RightCardWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Play a game of Gambler's Lure"
            });

            this.CommandManager.AddHandler(SettingsCommandName, new CommandInfo(OnHelpCommand)
            {
                HelpMessage = "Gambler's Lure Tips n' Tricks"
            });

            this.CommandManager.AddHandler(ConfigCommandName, new CommandInfo(OnSettingsCommand)
            {
                HelpMessage = "Configure Gambler's Lure game difficulty, etc."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();
            ChoiceWindow.Dispose();
            LeftCardWindow.Dispose();
            RightCardWindow.Dispose();

            this.CommandManager.RemoveHandler(CommandName);
            this.CommandManager.RemoveHandler(SettingsCommandName);
            this.CommandManager.RemoveHandler(ConfigCommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui's
            MainWindow.IsOpen = true;
            ChoiceWindow.IsOpen = true;
            LeftCardWindow.IsOpen = true;
            RightCardWindow.IsOpen = true;
        }

        public void OnHelpCommand(string command, string args)
        {
            PluginInterface.UiBuilder.AddNotification("Cards are numbered from 1 to 9. Guess if the face down card is higher or lower than the revealed card.", "How to Play", Dalamud.Interface.Internal.Notifications.NotificationType.Info, 6900);
        }

        public void OnSettingsCommand(string command, string args)
        {
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        public enum ChoiceType { None, Higher, Lower }

        // janky shit because i don't feel like setting up deserialization
        public void ConstructDatabase()
        {
            // 0 - hello
            Dialogue aloo = new Dialogue();
            aloo.stateId = "ALOO";
            aloo.content = "Greetings, would you like to play a game of Gambler's Lure?";
            aloo.portraitImagePath = "Portraits/ALOO.png";
            Choice alooChoice1 = new Choice();
            alooChoice1.choiceTxt = "Yes";
            alooChoice1.nextState = "Intro";
            aloo.choices.Add(alooChoice1);
            Choice alooChoice2 = new Choice();
            alooChoice2.choiceTxt = "No";
            alooChoice2.nextState = "CloseGame";
            aloo.choices.Add(alooChoice2);
            Choice alooChoice3 = new Choice();
            alooChoice3.choiceTxt = "How do I play?";
            alooChoice3.nextState = "Help";
            aloo.choices.Add(alooChoice3);
            dialogues.Add(aloo);

            // 1 - goodbye
            Dialogue goodbye = new Dialogue();
            goodbye.stateId = "CloseGame";
            goodbye.content = "Come back when you've grown some balls.";
            goodbye.portraitImagePath = "Portraits/eatPizzaGlare.png";
            Choice goodbyeChoice1 = new Choice();
            goodbyeChoice1.choiceTxt = "Bye";
            goodbyeChoice1.nextState = "EndGame";
            goodbye.choices.Add(goodbyeChoice1);
            dialogues.Add(goodbye);

            // 2 - intro transition
            Dialogue intro = new Dialogue();
            intro.stateId = "Intro";
            intro.content = "Two playing cards coming right up...";
            intro.portraitImagePath = "Portraits/butWait.png";
            dialogues.Add(intro);

            // 3 - time to play
            Dialogue gameLoop = new Dialogue();
            gameLoop.stateId = "GameLoop";
            gameLoop.content = "Choose high or low...";
            gameLoop.portraitImagePath = "Portraits/cardsOut.png";
            Choice gameLoopChoice1 = new Choice();
            gameLoopChoice1.choiceTxt = "Higher";
            gameLoopChoice1.choiceType = ChoiceType.Higher;
            gameLoop.choices.Add(gameLoopChoice1);
            Choice gameLoopChoice2 = new Choice();
            gameLoopChoice2.choiceTxt = "Lower";
            gameLoopChoice2.choiceType = ChoiceType.Lower;
            gameLoop.choices.Add(gameLoopChoice2);
            dialogues.Add(gameLoop);

            // 4 - you win
            Dialogue youWin = new Dialogue();
            youWin.stateId = "YouWin";
            youWin.content = "Lucky guess... I mean congratulations!";
            youWin.portraitImagePath = "Portraits/ohoho.png";
            dialogues.Add(youWin);

            // 5 - you lose
            Dialogue youLose = new Dialogue();
            youLose.stateId = "YouWin";
            youLose.content = "AHAHAHA.. I mean ah, shucks.  That's too bad.";
            youLose.portraitImagePath = "Portraits/bloodlust.png";
            dialogues.Add(youLose);
        }

        public void DataBind(Dialogue dialogueData)
        {
            // set portrait
            MainWindow.SetPortrait(dialogueData.portraitImagePath);

            // set dialogue text
            ChoiceWindow.DialogueTxt = dialogueData.content;

            // set choices if applicable
            ChoiceWindow.activeChoices.Clear();

            foreach (Choice choice in dialogueData.choices)
            {
                ChoiceWindow.activeChoices.Add(choice);
            }

            // special cases
            switch (dialogueData.stateId)
            {
                case "GameLoop":
                    if (LeftCardWindow.cardValue == 0)
                        LeftCardWindow.RandomizeCard();
                    else
                        LeftCardWindow.SetCard(RightCardWindow.cardValue);
                    break;
            }
        }


        public void PlayIntro()
        {
            IntroTransition();

            // RunOnTick - https://goatcorp.github.io/Dalamud/api/Dalamud.Game.Framework.html#methods
            Framework.RunOnTick(TransitionToGameLoop, TimeSpan.FromSeconds(3));
        }

        private void IntroTransition()
        {
            LeftCardWindow.SummonCard();
            RightCardWindow.SummonCard();
        }

        private void TransitionToGameLoop()
        {
            RightCardWindow.FlipBackDown();
            DataBind(dialogues[3]);
        }

        public void YouWin()
        {
            DataBind(dialogues[4]);

            Framework.RunOnTick(TransitionToGameLoop, TimeSpan.FromSeconds(4));
        }

        public void YouLose()
        {
            DataBind(dialogues[5]);

            Framework.RunOnTick(TransitionToGameLoop, TimeSpan.FromSeconds(4));
        }
    }
}

[Serializable]
public class Dialogue
{
    public string stateId = string.Empty;
    public string content = string.Empty;
    public List<Choice> choices = new List<Choice>();
    public string portraitImagePath = string.Empty;
}

[Serializable]
public class Choice
{
    public string choiceTxt = string.Empty;
    public string? nextState;
    public ChoiceType choiceType = ChoiceType.None;
}

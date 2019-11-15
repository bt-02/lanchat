﻿using Lanchat.Cli.PromptLib;
using System;
using static Lanchat.Cli.Program.Program;

namespace Lanchat.Cli.CommandsLib
{
    public static class Command
    {
        public static void Execute(string command)
        {
            string[] args = command.Split(' ');

            // Commands
            switch (args[0])
            {
                case "help":
                    ShowHelp();
                    break;

                case "exit":
                    Exit();
                    break;

                case "nick":
                    SetNick(args[1]);
                    break;

                default:
                    Prompt.Out("Bad command");
                    break;
            }
        }

        // Methods
        private static void ShowHelp()
        {
            Prompt.Out("");
            Prompt.Out("/exit - quit lanchat");
            Prompt.Out("/help - list of commands");
            Prompt.Out("/nick - change nick");
            Prompt.Out("");
        }

        private static void SetNick(string nick)
        {
            if (!string.IsNullOrEmpty(nick))
            {
                EditConfig("nickname", nick);
                Prompt.Notice("Nickname changed");
            }
        }

        private static void Exit() => Environment.Exit(0);
    }
}
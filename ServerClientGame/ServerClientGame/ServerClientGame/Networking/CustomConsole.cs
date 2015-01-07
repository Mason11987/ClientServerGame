﻿using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using ServerClientGame.Commands;

namespace ServerClientGame.Networking
{

    public class CustomConsole : GameComponent
    {
        public bool hasInput { get { return currentinput.EndsWith(Environment.NewLine); } }
        
        private string currentinput;
        private List<string> inputHistory = new List<string>();
        const int maxHistoryLength = 100;
        private int inputHistoryPosition;

        public Client client;
        public Server server;

        public CustomConsole(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public CustomConsole(Game game, Client client) 
            : this(game)
        {
            this.client = client;
        }

        public CustomConsole(Game game, Server server) 
            : this(game)
        {
            this.server = server;
            this.client = server.LocalClient;
        }
         
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            currentinput = "";
            

            base.Initialize();
        }

        private void DoBackspace(int times = 1)
        {
            while (currentinput.Length > 0 && times > 0)
            {
                if (Console.CursorLeft == 0)
                {
                    Console.CursorTop--;
                    Console.CursorLeft = Console.WindowWidth - 1;
                    Console.Write(" ");
                    Console.CursorTop--;
                    Console.CursorLeft = Console.WindowWidth - 1;
                }
                else
                {
                    Console.Write("\b \b");
                }
                times--;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            if (Console.KeyAvailable && !hasInput)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Backspace)
                {
                    DoBackspace();

                    if (currentinput.Length > 0)
                        currentinput = currentinput.Substring(0, currentinput.Length - 1);
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.Write(Environment.NewLine);
                    currentinput += Environment.NewLine;
                }
                else if (key.Key == ConsoleKey.UpArrow) 
                {
                    if (inputHistoryPosition < inputHistory.Count - 1)
                        inputHistoryPosition++;

                    DoBackspace(currentinput.Length);

                    currentinput = inputHistory[inputHistoryPosition];
                    Console.Write(currentinput);
                }
                else if (key.Key == ConsoleKey.DownArrow) 
                {
                    if (inputHistoryPosition > 0)
                        inputHistoryPosition--;

                    DoBackspace(currentinput.Length);

                    currentinput = inputHistory[inputHistoryPosition];
                    Console.Write(currentinput);
                }
                else if (key.KeyChar != '\0')
                {
                    Console.Write(key.KeyChar);
                    currentinput += key.KeyChar;
                }
            }


            base.Update(gameTime);

        }
    
        internal string ReadInput()
        {
            var returnInput = currentinput.TrimEnd(Environment.NewLine.ToCharArray());

            inputHistory.Insert(0, returnInput);
            if (inputHistory.Count > maxHistoryLength)
                inputHistory.RemoveAt(maxHistoryLength);
            inputHistoryPosition = -1;
            currentinput = "";
            return returnInput;
        }

        internal void Output(string p)
        {
            if (!String.IsNullOrEmpty(currentinput)) //If there's already some input
            {
                DoBackspace(currentinput.Length);

                Console.WriteLine(p);
                Console.Write(currentinput);
            }
            else
                Console.WriteLine(p);
        }

        public Command GetCommand(string input)
        {
            var parsar = new CommandParser(Command.GetAvailableCommands(server != null));
            return parsar.ParseCommand(input);
        }


        public ConsoleCommandType GetCommandArgsFromString(string input, out string[] args)
        {
            args = null;
            if (input.StartsWith("/"))
            {
                var inSplit = input.Substring(1).Split(' ');
                var command = inSplit[0];

                args = input.Substring(1 + command.Length).Split(new [] {' '},StringSplitOptions.RemoveEmptyEntries);



                switch (command)
                {
                    case "identify":
                        return ConsoleCommandType.Identify;
                    case "disconnect":
                        return ConsoleCommandType.Disconnect;
                    case "connect":
                        return ConsoleCommandType.Connect;
                    case "exit":
                        return ConsoleCommandType.Exit;
                    case "clients":
                        return ConsoleCommandType.Clients;
                    case "help":
                        return ConsoleCommandType.Help;
                    case "say":
                        return ConsoleCommandType.Say;
                    default:
                        return ConsoleCommandType.Unknown;
                }    
            }
            else
            {
                args = new[] { input };
                return ConsoleCommandType.Text;
            }
        }
    }
}

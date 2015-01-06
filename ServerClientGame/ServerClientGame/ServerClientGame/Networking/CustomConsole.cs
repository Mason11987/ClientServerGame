using System;
using Microsoft.Xna.Framework;

namespace ServerClientGame.Networking
{
    public enum ConsoleCommandType : byte
    {
        Text,
        Disconnect,
        Connect,
        Identify,
        Exit,
        Clients,
        Say,
        Help,
        Unknown
    }

    class CustomConsole : GameComponent
    {
        public bool hasInput { get { return currentinput.EndsWith(Environment.NewLine); } }

        private string currentinput;

        public CustomConsole(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
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
                   Console.Write("\b \b");
                    if (currentinput.Length > 0)
                        currentinput = currentinput.Substring(0,currentinput.Length - 1);
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.Write(Environment.NewLine);
                    currentinput += Environment.NewLine;
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
            currentinput = "";
            return returnInput;
        }

        internal void Output(string p)
        {
            if (!String.IsNullOrEmpty(currentinput)) //If there's already some input
            {
                for (int i = 0; i < currentinput.Length; i++)
                {
                    Console.Write("\b \b");
                }
                Console.WriteLine(p);
                Console.Write(currentinput);
            }
            else
                Console.WriteLine(p);
        }

        public static ConsoleCommandType GetCommandArgsFromString(string input, out string[] args)
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

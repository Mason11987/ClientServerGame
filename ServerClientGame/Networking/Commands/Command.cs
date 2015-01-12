using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Networking.Commands
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

    public class Command  
    {
        public Server Server { get { return NetworkManager.Server; } }
        public Client Client { get { return NetworkManager.Client; } }
        public CustomConsole Console { get { return NetworkManager.Console; } }
        public Game Game { get { return NetworkManager.Game; } }

        private static readonly List<ICommandFactory> _availableServerCommands = 
            new List<ICommandFactory>
            {
                new HelpCommand(),
                new SayCommand(),
                new TextCommand(),
                new ClientsCommand(),
                new SettingsCommand(),
                new ExitCommand()
            };
        private static readonly List<ICommandFactory> _availableClientCommands = 
            new List<ICommandFactory>
            {
                new HelpCommand(),
                new IdentifyCommand(),
                new SayCommand(),
                new ConnectCommand(),
                new DisconnectCommand(),
                new TextCommand(),
                new ClientsCommand(),
                new SettingsCommand(),
                new ExitCommand()
            };

        public virtual CommandResult Execute()
        {
            return CommandResult.Failed;
        }

        public static void AddCommand(ICommandFactory command, bool isServer, bool isClient)
        {
            if (isServer)
                _availableClientCommands.Add(command);
            if (isClient)
                _availableClientCommands.Add(command);
        }

        internal static IEnumerable<ICommandFactory> AvailableCommands(bool isServer)
        {
            return isServer ? _availableServerCommands : _availableClientCommands;
        }
    }
}

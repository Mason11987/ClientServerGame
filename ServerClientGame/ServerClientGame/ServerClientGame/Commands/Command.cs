using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerClientGame.Networking;

namespace ServerClientGame.Commands
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
        public static CustomConsole console;

        public Server Server { get { return console.server; } }
        public Client Client { get { return console.client; } }

        public virtual CommandResult Execute()
        {
            return CommandResult.Failed;
        }

        internal static IEnumerable<ICommandFactory> GetAvailableCommands(bool isServer)
        {
            if (isServer)
                return new ICommandFactory[]
                    {
                        new HelpCommand(),
                        new SayCommand(),
                        new TextCommand(),
                        new ClientsCommand(),
                        new SettingsCommand(),
                        new ExitCommand()
                    };
            else
            {
                return new ICommandFactory[] 
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
            }
            //console.Output("/identify NAME - Identify local client as NAME");
            //console.Output("/say NAME message  - whisper message to NAME");
            //console.Output("/connect - Connects to the server ");
            //console.Output("/disconnect - Disconnects from the server ");
            //console.Output("/exit - Ends the application ");
            //console.Output("/help - Displays this message ");
        }

    }
}

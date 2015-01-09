using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Networking.Networking.Packets;

namespace Networking.Commands
{
    class ClientsCommand : Command, ICommandFactory
    {

        public override CommandResult Execute()
        {
            if (Server == null)
                Client.Send(new PacketConsoleCommand(CommandType, new string[] {}));
            else
            {
                Console.Output("List of Clients");
                Console.Output(Server.GetClientDisplayList());
            }
            return CommandResult.Success;
        }

        public string CommandName { get { return "Clients"; } }
        public string Description { get { return "/clients - Lists all clients "; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Clients; } }


        public Command MakeCommand(string[] args)
        {

            return new ClientsCommand() { };
        }
    }
}
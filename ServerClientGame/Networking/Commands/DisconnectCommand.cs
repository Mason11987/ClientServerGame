using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Networking.Networking;
using Networking.Networking.Packets;

namespace Networking.Commands
{
    class DisconnectCommand : Command, ICommandFactory
    {

        public override CommandResult Execute()
        {
            if (Client.tcpClient.Connected)
                return DisconnectFromServer();
            
            Console.Output("Not Connected to Server");

            return CommandResult.Success;
        }

        public CommandResult DisconnectFromServer()
        {
            Console.Output("Disconnected From Server");
            if (Client.hasLocalServer)
            {
                Client.commThread.Abort();
                Client.CommStream.Close();
                Client.tcpClient.Close();
                Client.Alive = false;
            }
            else
            {
                if (Client.tcpClient.Connected)
                    Client.Send(new PacketConsoleCommand(ConsoleCommandType.Disconnect));
                Client.tcpClient.Close();
                Client.Initialize();
            }
            return CommandResult.Success;
        }

        public string CommandName { get { return "Disconnect"; } }
        public string Description { get { return "/disconnect - Disconnects from the server"; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Disconnect; } }

        public Command MakeCommand(string[] args)
        {

            return new DisconnectCommand() { };
        }
    }
}

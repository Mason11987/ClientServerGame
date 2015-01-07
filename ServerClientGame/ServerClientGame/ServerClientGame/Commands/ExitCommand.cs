﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerClientGame.Networking.Packets;

namespace ServerClientGame.Commands
{
    class ExitCommand : Command, ICommandFactory
    {

        public override CommandResult Execute()
        {
            if (Server == null)
            {
                console.Output("Exiting");
                if (Client.commThread != null)
                {
                    Client.commThread.Abort();
                    Client.tcpClient.GetStream().Close();
                }
                Client.tcpClient.Close();
                Client.Alive = false;
            }
            else
            {
                console.Output("Exiting");
                Server.Broadcast(new PacketConsoleCommand(ConsoleCommandType.Disconnect));

                Server.Alive = false;
                foreach (var client in Server.Clients.Values)
                    client.Close();
                Server.tcpListener.Stop();
                Server.listenThread.Abort();
                Server.Game.Exit();
            }
            return CommandResult.Success;
        }

        public string CommandName { get { return "Exit"; } }
        public string Description { get { return "/exit - Ends the application"; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Exit; } }


        public Command MakeCommand(string[] args)
        {

            return new ExitCommand() { };
        }
    }
}
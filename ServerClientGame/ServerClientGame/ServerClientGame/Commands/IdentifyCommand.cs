﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerClientGame.Networking.Packets;
using ServerClientGame.Networking;

namespace ServerClientGame.Commands
{
    class IdentifyCommand : Command, ICommandFactory
    {
        public string Name { get; set; }
        public RemoteClient RemoteClientToIdentify { get; set; }

        public override CommandResult Execute()
        {
            if (Server == null)
                Client.Send(new PacketConsoleCommand(CommandType, new string[] { Name }));
            else if (RemoteClientToIdentify != null)
            {
                if (Server.Clients.ContainsKey(Name))
                {
                    console.Output("Failed to update identifation of " + RemoteClientToIdentify.IP);
                    RemoteClientToIdentify.Send(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", "Identify Failed: A user already exists with the name: " + Name }));
                    return CommandResult.Failed;
                }

                RemoteClient outRemoteClient;
                if (Server.Clients.TryRemove(RemoteClientToIdentify.Name, out outRemoteClient))
                {
                    string oldName = RemoteClientToIdentify.Name;
                    RemoteClientToIdentify.Name = Name;
                    Server.Clients.TryAdd(RemoteClientToIdentify.Name, RemoteClientToIdentify);

                    console.Output("Client " + RemoteClientToIdentify.IP + " - Identified as " + Name);
                    Server.Broadcast(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", " * " + oldName + " identified as " + RemoteClientToIdentify.Name + " *" }));

                    return CommandResult.Success;
                }
                else
                {
                    console.Output("Failed to update identifation of " + RemoteClientToIdentify.IP);
                    return CommandResult.Failed;
                }
            }
            else if (Server.LocalClient != null)
                Server.LocalClient.Send(new PacketConsoleCommand(CommandType, Name));

            return CommandResult.Success;
        }

        public string CommandName { get { return "Identify"; } }
        public string Description { get { return "/identify NAME - Identify client as NAME"; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Identify; } }

        public Command MakeCommand(string[] args)
        {
            if (args.Length > 2) throw new UnexpectedCommandArgumentException("Identify failed: Name contained spaces");
            if (args.Length == 1) throw new UnexpectedCommandArgumentException("Identify failed: No Name supplied");

            return new IdentifyCommand() { Name = args[1]};
        }

        public Command MakeCommand(string[] args, RemoteClient remoteClient)
        {
            Command command = MakeCommand(new string[] {CommandName.ToLower(), args[0]});

            if (command is IdentifyCommand)
                (command as IdentifyCommand).RemoteClientToIdentify = remoteClient;

            return command;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerClientGame.Networking.Packets;

namespace ServerClientGame.Commands
{
    class TextCommand : Command, ICommandFactory
    {
        public string Message { get; set; }

        public override CommandResult Execute()
        {
            if (Server == null)
                Client.Send(new PacketConsoleCommand(CommandType, Message));
            else
                Server.Broadcast(new PacketConsoleCommand(CommandType, new[] { "SERVER", Message }));

            return CommandResult.Success;
        }

        public string CommandName { get { return "Text"; } }
        public string Description { get { return "/text MESSAGE - Sends a message (can exclude the /text)"; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Text; } }

        public Command MakeCommand(string[] args)
        {
            if (args.Length < 2) throw new UnexpectedCommandArgumentException("Can't send empty Messages");


            return new TextCommand() { Message = string.Join(" ", (args.Skip(1))) };
        }
    }
}


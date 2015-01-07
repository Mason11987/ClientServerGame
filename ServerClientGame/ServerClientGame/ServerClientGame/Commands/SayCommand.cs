using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerClientGame.Networking.Packets;

namespace ServerClientGame.Commands
{
    class SayCommand : Command, ICommandFactory
    {
        public string Name { get; set; }
        public string Message { get; set; }

        public override CommandResult Execute()
        {
            var args = new string[] { Name, string.Join(" ", Message) };
            if (Server == null)
                Client.Send(new PacketConsoleCommand(CommandType, args));
            else
                Server.LocalClient.Send(new PacketConsoleCommand(CommandType, args));

            return CommandResult.Success;
        }

        public string CommandName { get { return "Say"; } }
        public string Description { get { return "/say NAME message  - whisper message to NAME" + ( Server != null ? " from local client" : ""); } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Say; } }

        public Command MakeCommand(string[] args)
        {
            if (args.Length < 3) throw new UnexpectedCommandArgumentException("Say failed: Say requires a Name and Message");


            return new SayCommand() { Name = args[1], Message = string.Join(" ", (args.Skip(2))) };
        }
    }
}

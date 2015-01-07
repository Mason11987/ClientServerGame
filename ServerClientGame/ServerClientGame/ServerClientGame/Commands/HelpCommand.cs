using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerClientGame.Commands
{
    public class HelpCommand : Command, ICommandFactory
    {
        public override CommandResult Execute()
        {
            console.Output("Commands:");
            foreach (var command in Command.GetAvailableCommands(console.server != null))
                console.Output(string.Format("  {0}", command.Description));
            return CommandResult.Success;
        }

        public string CommandName { get { return "Help"; } }
        public string Description { get { return "/help - Displays this message "; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Help; } }


        public Command MakeCommand(string[] args)
        {
            return new HelpCommand();
        }
    }
}

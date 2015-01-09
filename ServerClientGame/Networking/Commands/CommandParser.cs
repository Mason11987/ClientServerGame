using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Networking.Commands;

namespace Networking.Commands
{
    class CommandParser
    {
        readonly IEnumerable<ICommandFactory> availableCommands;

        public CommandParser(IEnumerable<ICommandFactory> availableCommands)
        {
            this.availableCommands = availableCommands;
        }

        internal Command ParseCommand(string input)
        {
            if (!input.StartsWith("/"))
                input = "text " + input;
            else
                input = input.TrimStart('/');

            return ParseCommand(input.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

        }

        private Command ParseCommand(string[] args)
        {
            var requestedCommandName = args[0];

            var command = FindReqeustedCommand(requestedCommandName);

            if (command == null)
                return new NotFoundCommand() { Name = requestedCommandName };

            try
            {
                return command.MakeCommand(args);
            }
            catch (UnexpectedCommandArgumentException e)
            {
                return new ErrorCommand() { Name = requestedCommandName, Exception = e };
            }
            

            //args = null;
            //if (input.StartsWith("/"))
            //{
            //    var inSplit = input.Substring(1).Split(' ');
            //    var command = inSplit[0];

            //    args = input.Substring(1 + command.Length).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);



            //    switch (command)
            //    {
            //        case "identify":
            //            return ConsoleCommandType.Identify;
            //        case "disconnect":
            //            return ConsoleCommandType.Disconnect;
            //        case "connect":
            //            return ConsoleCommandType.Connect;
            //        case "exit":
            //            return ConsoleCommandType.Exit;
            //        case "clients":
            //            return ConsoleCommandType.Clients;
            //        case "help":
            //            return ConsoleCommandType.Help;
            //        case "say":
            //            return ConsoleCommandType.Say;
            //        default:
            //            return ConsoleCommandType.Unknown;
            //    }
            //}
            //else
            //{
            //    args = new[] { input };
            //    return ConsoleCommandType.Text;
            //}
            
            //return new NotFoundCommand();

        }

        private ICommandFactory FindReqeustedCommand(string commandName)
        {
            return availableCommands.FirstOrDefault(cmd => cmd.CommandName.ToLower() == commandName.ToLower());
        }

    }
}

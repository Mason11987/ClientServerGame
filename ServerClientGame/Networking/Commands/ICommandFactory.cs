using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Networking.Networking;

namespace Networking.Commands
{
    public interface ICommandFactory
    {
        string CommandName { get; }
        string Description { get; }
        ConsoleCommandType CommandType { get; }

        Command MakeCommand(string[] arguments);
    }
}

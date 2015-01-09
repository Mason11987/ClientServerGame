using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Networking.Commands
{
    public class CommandResult
    {
        public string Name { get; set; }

        public static CommandResult Success = new CommandResult() { Name = "Success" };
        public static CommandResult Failed = new CommandResult() { Name = "Failed" };
        public static CommandResult NotImplemented = new CommandResult() { Name = "NotImplemented" };
    }

    
}

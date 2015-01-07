using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerClientGame.Commands
{
    class ErrorCommand : Command
    {
        public string Name { get; set; }
        public Exception Exception { get; set; }

        public override CommandResult Execute()
        {
            console.Output(Exception.Message);
            return CommandResult.Success;
        }
    }
}

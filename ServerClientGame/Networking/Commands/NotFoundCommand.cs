using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Networking.Commands
{
    class NotFoundCommand : Command
    {
        public string Name { get; set; }
        public override CommandResult Execute()
        {
            Console.Output("Couldn't find command: " + Name);
            return CommandResult.Success;
        }
    }
}

﻿using System;

namespace Networking.Commands
{
    class ErrorCommand : Command
    {
        public string Name { get; set; }
        public Exception Exception { get; set; }

        public override CommandResult Execute()
        {
            Console.Output(Exception.Message);
            return CommandResult.Success;
        }
    }
}

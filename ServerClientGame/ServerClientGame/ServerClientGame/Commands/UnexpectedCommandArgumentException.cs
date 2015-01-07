using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerClientGame.Commands
{
    class UnexpectedCommandArgumentException : Exception
    {

        public UnexpectedCommandArgumentException(string message) : base(message)
        {
            
        }
    }
}

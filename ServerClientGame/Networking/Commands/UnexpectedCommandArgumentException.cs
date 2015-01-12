using System;

namespace Networking.Commands
{
    class UnexpectedCommandArgumentException : Exception
    {

        public UnexpectedCommandArgumentException(string message) : base(message)
        {
            
        }
    }
}

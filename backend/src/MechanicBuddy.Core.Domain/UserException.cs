using System;

namespace MechanicBuddy.Core.Domain
{
    public class UserException : Exception
    {
        public UserException(string message) : base(message)
        {
        }
    }

}

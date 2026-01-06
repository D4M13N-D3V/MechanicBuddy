using System;

namespace Carmasters.Core.Domain
{
    public class UserException : Exception
    {
        public UserException(string message) : base(message)
        {
        }
    }

}

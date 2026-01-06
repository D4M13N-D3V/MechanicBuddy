using System;
using Carmasters.Core.Domain;
using Npgsql;

namespace Carmasters.Core.Application.Errors
{
    public class JsonErrorDto
    {
        public JsonErrorDto(string message,string exceptionDetails) 
        {
            ExceptionMessage = message;
            ExceptionDetails = exceptionDetails;
        }
        public JsonErrorDto(Exception exception)
        {
            IsUserError = exception is UserException;
            ExceptionMessage = exception.Message;
            ExceptionDetails = exception.ToString();
            //refractor out TODO
            if (IsPosgresTryingToDeleteButRelatedDataExists(exception))
            {
                IsUserError = true;
                ExceptionMessage = "Problem occured while deleting data, there is other data associated with it, preventing the removal.";
            }
           
        }

        private static bool IsPosgresTryingToDeleteButRelatedDataExists(Exception exception)
        {
            return exception?.InnerException is PostgresException 
                && exception?.Message.Contains("could not delete") == true && exception?.InnerException?.Message.Contains("violates foreign key constraint") == true;
        }
        public bool IsUserError { get;   }
        public string ExceptionMessage { get; }
        public string ExceptionDetails { get;  }
    }

}

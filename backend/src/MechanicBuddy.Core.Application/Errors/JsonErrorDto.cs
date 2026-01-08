using System;
using MechanicBuddy.Core.Domain;
using Npgsql;

namespace MechanicBuddy.Core.Application.Errors
{
    public class JsonErrorDto
    {
        public JsonErrorDto(string message, string exceptionDetails)
        {
            ExceptionMessage = message;
            ExceptionDetails = exceptionDetails;
        }

        public JsonErrorDto(Exception exception) : this(exception, includeDetails: true)
        {
        }

        /// <summary>
        /// Creates a JSON error DTO from an exception
        /// </summary>
        /// <param name="exception">The exception to create the DTO from</param>
        /// <param name="includeDetails">Security: Set to false in production to hide stack traces</param>
        public JsonErrorDto(Exception exception, bool includeDetails)
        {
            IsUserError = exception is UserException;
            ExceptionMessage = exception.Message;

            // Security: Only include full exception details (stack trace) when requested
            // In production, this should be false to prevent information disclosure
            ExceptionDetails = includeDetails ? exception.ToString() : null;

            // Handle special case for PostgreSQL foreign key violations
            if (IsPosgresTryingToDeleteButRelatedDataExists(exception))
            {
                IsUserError = true;
                ExceptionMessage = "Problem occured while deleting data, there is other data associated with it, preventing the removal.";
            }

            // Security: For non-user errors in production, use a generic message
            if (!IsUserError && !includeDetails)
            {
                ExceptionMessage = "An unexpected error occurred. Please try again later.";
            }
        }

        private static bool IsPosgresTryingToDeleteButRelatedDataExists(Exception exception)
        {
            return exception?.InnerException is PostgresException
                && exception?.Message.Contains("could not delete") == true && exception?.InnerException?.Message.Contains("violates foreign key constraint") == true;
        }

        public bool IsUserError { get; }
        public string ExceptionMessage { get; }
        public string ExceptionDetails { get; }
    }

}

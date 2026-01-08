using System;
using System.Data;
using System.Linq;



namespace MechanicBuddy.Core.Application.Services
{
    public class WildcardTokens
    {
        private readonly string searchText;

        public WildcardTokens(string searchText)
        {
            this.searchText = searchText;
        }

        public  string[] AllTokens()
        {
            var words = searchText.
                     Split((char[])null, StringSplitOptions.RemoveEmptyEntries).
                     ToArray();

            return words;
        }

        /// <summary>
        /// Returns sanitized tokens safe for use in SQL LIKE patterns.
        /// Escapes SQL wildcards and special characters to prevent injection.
        /// </summary>
        public string[] AllTokensSanitized()
        {
            return AllTokens()
                .Select(SanitizeToken)
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();
        }

        private static string SanitizeToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return token;

            // Escape SQL LIKE wildcards and special characters
            return token
                .Replace("\\", "\\\\")  // Escape backslash first
                .Replace("%", "\\%")    // Escape wildcard
                .Replace("_", "\\_")    // Escape single char wildcard
                .Replace("'", "''")     // Escape single quote for SQL
                .Replace("[", "\\[")    // Escape bracket
                .Replace("]", "\\]");   // Escape bracket
        }
    }
}

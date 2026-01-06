using System;
using System.Data;
using System.Linq;



namespace Carmasters.Core.Application.Services
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
    }
}

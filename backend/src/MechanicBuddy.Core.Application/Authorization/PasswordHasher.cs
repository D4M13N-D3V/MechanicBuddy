using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Authorization
{
    //todo , more secure login implementation
    public static class PasswordHasher
    {
        public static string getHash(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }

        // Verify a hash against a string.
        public static bool verifyHash(string input, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(input, hash);
        }
    }
}

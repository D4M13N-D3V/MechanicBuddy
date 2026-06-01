using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Application.Authorization
{
    //todo , more secure login implementation
    public static class PasswordHasher
    {
        public static string getHash(string input)
        {
            // Use the modern $2b$ bcrypt revision (work factor 11). Verify still
            // accepts existing $2a$ hashes, so this is backwards compatible.
            var salt = BCrypt.Net.BCrypt.GenerateSalt(11, 'b');
            return BCrypt.Net.BCrypt.HashPassword(input, salt);
        }

        // Verify a hash against a string.
        public static bool verifyHash(string input, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(input, hash);
        }
    }
}

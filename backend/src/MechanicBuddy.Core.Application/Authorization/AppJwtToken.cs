using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using MechanicBuddy.Core.Application.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MechanicBuddy.Core.Application.Authorization
{
    public class AppJwtToken
    {
        // Security: Define issuer and audience for token validation
        private const string Issuer = "MechanicBuddy";
        private const string Audience = "MechanicBuddy";

        // Security: Maximum session timeout to prevent overly long sessions
        private static readonly TimeSpan MaxSessionTimeout = TimeSpan.FromHours(8);

        public static JwtSecurityToken LoadJwt(JwtOptions options, string token)
        {
            EnsureJwtSecret(options);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(options.Secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                // Security: Enable issuer and audience validation
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            return jwtToken;
        }

        public static string Generate(JwtOptions options, ClaimsPrincipal principal)
        {
            EnsureJwtSecret(options);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(options.Secret);

            var subject = ((ClaimsIdentity)principal.Identity);

            // Security: Enforce maximum session timeout
            var sessionTimeout = options.SessionTimeout > MaxSessionTimeout
                ? MaxSessionTimeout
                : options.SessionTimeout;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.Add(sessionTimeout),
                // Security: Include issuer and audience in token
                Issuer = Issuer,
                Audience = Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private static void EnsureJwtSecret(JwtOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Secret)) throw new ArgumentException("Jwt secret not configured");
        }

    }
}

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Carmasters.Core.Application.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Carmasters.Core.Application.Authorization
{
    public class AppJwtToken
    {

        public static JwtSecurityToken LoadJwt(JwtOptions options, string token)
        {
            EnsureJwtSecret(options);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(options.Secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            return jwtToken;
        }

        public static string Generate(JwtOptions options, ClaimsPrincipal principal)
        {
            EnsureJwtSecret(options);
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(options.Secret);

            var subject = ((ClaimsIdentity)principal.Identity);

            var tokenDescriptor = new SecurityTokenDescriptor
            { 
                Subject = subject,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.Add(options.SessionTimeout),
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

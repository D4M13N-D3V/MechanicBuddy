using System.ComponentModel.DataAnnotations;

namespace MechanicBuddy.Http.Api.Models
{
    public record LoginDto([Required] string Username, [Required] string Password, [Required] string ServerSecret);
    public record RegisterDto([Required] string Username, [Required] string Password, [Required] string Email);
    public record JwtDto([Required] string Token);
    public record AuthenticateResponseDto(string Jwt, string PublicJwt, int Timeout, bool MustChangePassword);
}
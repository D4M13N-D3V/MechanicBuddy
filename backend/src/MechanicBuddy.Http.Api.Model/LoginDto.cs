using System.ComponentModel.DataAnnotations;

namespace Carmasters.Http.Api.Models
{
    public record LoginDto([Required] string Username, [Required] string Password, [Required] string ServerSecret);
    public record RegisterDto([Required] string Username, [Required] string Password, [Required] string Email);
    public record JwtDto([Required] string Token);
}

using System.Security.Claims;
namespace BLL.Interface;

public interface IJWTService
{
    string GenerateToken(string email, string role);
    string GenerateResetToken(string email, string password);
    ClaimsPrincipal? GetClaimsFromToken(string token);
    string? GetClaimValue(string token, string claimType);
}
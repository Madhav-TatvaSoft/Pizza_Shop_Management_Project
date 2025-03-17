
using System.Security.Claims;
namespace BLL.Interface;

public interface IJWTService
{
    public string GenerateToken(string email, string role);
    public string GenerateResetToken(string email, string password);
    public ClaimsPrincipal? GetClaimsFromToken(string token);
    public string? GetClaimValue(string token, string claimType);
}

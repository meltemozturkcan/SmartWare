using SmartWare.API.Core.Entities;
using System.Security.Claims;

namespace SmartWare.API.Features.Auth.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user); // Mevcut metodu yeniden adlandır
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}

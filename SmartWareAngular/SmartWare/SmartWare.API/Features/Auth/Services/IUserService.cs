using SmartWare.API.Core.Entities;
using SmartWare.API.Features.Auth.DTOs;

namespace SmartWare.API.Features.Auth.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string usernameOrEmail, string password);
        Task<User> RegisterAsync(RegisterDto registerDto);
    }
}

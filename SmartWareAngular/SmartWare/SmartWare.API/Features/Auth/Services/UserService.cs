using Microsoft.EntityFrameworkCore;
using SmartWare.API.Core.Entities;
using SmartWare.API.Data;
using SmartWare.API.Features.Auth.DTOs;

namespace SmartWare.API.Features.Auth.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;
        public UserService(
        ApplicationDbContext context,
        ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<User> AuthenticateAsync(string usernameOrEmail, string password)
        {
            // Kullanıcıyı kullanıcı adı veya email ile bul
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    (u.Username == usernameOrEmail || u.Email == usernameOrEmail) &&
                    !u.IsDeleted
                );
            // Kullanıcı bulunamazsa null döndür
            if (user == null)
                return null;

            // Şifre doğrulaması
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            // Kullanıcı adı veya email kontrolü
            if (await _context.Users.AnyAsync(u =>
                u.Username == registerDto.Username ||
                u.Email == registerDto.Email))
            {
                throw new Exception("Kullanıcı adı veya email zaten kullanımda");
            }

            // Yeni kullanıcı oluştur
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = "User" // Varsayılan rol
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}


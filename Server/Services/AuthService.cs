using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Common;
using Server.Contexts;
using Server.DTOs;
using Server.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Services
{
    public class AuthService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        private static readonly int _rounds = 10;
        private readonly string _generatedSalt = BCrypt.Net.BCrypt.GenerateSalt(_rounds);
        private readonly bool _enhancedEntropy = true;
        private readonly BCrypt.Net.HashType _hashType = BCrypt.Net.HashType.SHA512;

        private readonly UserService _userService;
        private readonly AppDbContext _dbContext;

        public AuthService(
            IConfiguration configuration,
            UserService userService,
            AppDbContext dbContext)
        {
            _secretKey = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];

            if (_secretKey == null || _issuer == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _userService = userService;
            _dbContext = dbContext;
        }

        public string GenerateToken(string id, UserRole role = UserRole.Standard)
        {
            var claims = new[]
            {
                new Claim("UserId", id),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // TODO: Implement refresh tokens
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(
                password,
                _generatedSalt,
                _enhancedEntropy,
                _hashType);
        }

        public bool CheckPassword(string password, string passwordHash)
        {
            if (
                BCrypt.Net.BCrypt.Verify(
                password,
                passwordHash,
                _enhancedEntropy,
                _hashType)
            )
            {
                return true;
            }

            return false;
        }

        public async Task<string> Login(AuthDto dto)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(f => f.Username == dto.Username);
            if (user == null)
            {
                throw new ArgumentNullException($"User not found, UserId: {dto.Username}");
            }

            if (CheckPassword(dto.Password, user.HashedPassword))
            {
                return GenerateToken(user.Id);
            }
            else
            {
                throw new InvalidOperationException("Invalid Password");
            }
        }

        public async Task<User> Create(AuthDto dto)
        {
            var hashedPassword = HashPassword(dto.Password);
            // I don't use AutoMapper here to save time for now
            var userDto = new UserCreateDto
            {
                Username = dto.Username,
                HashedPassword = hashedPassword,
                Role = UserRole.Standard // Enforcing standard role, to get admin, simply update it on the database
            };

            return await _userService.Create(userDto);
        }
    }
}

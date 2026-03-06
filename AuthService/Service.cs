using DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace AuthService { 
    public class Service : IService
    {
        private readonly IConfiguration _configuration;
        private readonly Repository _repository;
        public Service(IConfiguration configuration, Repository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto request)
        {
            var account = await _repository.GetByEmailAsync(request.Email);

            if (account == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
                return null;

            var token = GenerateJwtToken(account);

            return new LoginResponseDto
            {
                Token = token,
                Role = (DTOs.AccountRole)account.Role,
            };
        }

        public async Task<bool> RegisterAsync(RegisterDto request)
        {
            var exists = await _repository.EmailExistsAsync(request.Email);

            if (exists)
                return false;

            var account = new Account
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = (AccountRole)request.Role
            };
            await _repository.CreateAsync(account);

            return true;
        }

        private string GenerateJwtToken(Account account)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
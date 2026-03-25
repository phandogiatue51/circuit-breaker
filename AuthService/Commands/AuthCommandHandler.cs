using DTOs;
using DTOs.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Commands
{

    public class AuthCommandHandler
    {
        private readonly Repository _repository;
        private readonly EventStoreService _eventStore;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthCommandHandler> _logger;

        public AuthCommandHandler(
            Repository repository,
            IConfiguration configuration,
            EventStoreService eventStore,
            ILogger<AuthCommandHandler> logger)
        {
            _repository = repository;
            _configuration = configuration;
            _eventStore = eventStore;
            _logger = logger;
        }

        /// <summary>
        /// COMMAND: Xử lý đăng ký tài khoản
        /// </summary>
        public async Task<Account> Handle(RegisterCommand command)  // Change return type to Account (not nullable)
        {
            
            // Tạo account mới
            var account = new Account
            {
                Email = command.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password),
            };

            await _repository.CreateAsync(account);

            _logger.LogInformation("Account created successfully: {Email}", command.Email);

            await _eventStore.SaveEventAsync(account.Id, "AuthCreated", new
            {
                account.Id,
                account.Email,
                account.PasswordHash
            });

            return account;
        }

        /// <summary>
        /// COMMAND: Xử lý đăng nhập (tạo token)
        /// </summary>
        public async Task<LoginResponseDto?> Handle(LoginCommand command)
        {
            _logger.LogInformation("Handling LoginCommand for email: {Email}", command.Email);

            // Query để lấy account (gọi query handler)
            var account = await _repository.GetByEmailAsync(command.Email);

            if (account == null)
            {
                _logger.LogWarning("Account not found: {Email}", command.Email);
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(command.Password, account.PasswordHash))
            {
                _logger.LogWarning("Invalid password for: {Email}", command.Email);
                return null;
            }

            // Generate token
            var token = GenerateJwtToken(account);

            _logger.LogInformation("Login successful: {Email}", command.Email);

            return new LoginResponseDto
            {
                Token = token,
                Email = account.Email,
                AccountId = account.Id
            };
        }

        private string GenerateJwtToken(Account account)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-key-1234567890-1234567890"));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "AuthService",
                audience: _configuration["Jwt:Audience"] ?? "ProductService",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
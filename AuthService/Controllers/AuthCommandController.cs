using AuthService.Commands;
using DTOs;
using DTOs.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/commands/auth")]
    public class AuthCommandController : ControllerBase
    {
        private readonly AuthCommandHandler _commandHandler;
        private readonly ILogger<AuthCommandController> _logger;

        public AuthCommandController(AuthCommandHandler commandHandler, ILogger<AuthCommandController> logger)
        {
            _commandHandler = commandHandler;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            // Kiểm tra validation cơ bản
            if (string.IsNullOrWhiteSpace(command.Email))
                throw new BadRequestException("Email không được để trống", "EMPTY_EMAIL");

            if (string.IsNullOrWhiteSpace(command.Password))
                throw new BadRequestException("Mật khẩu không được để trống", "EMPTY_PASSWORD");

            if (command.Password.Length < 6)
                throw new BadRequestException("Mật khẩu phải có ít nhất 6 ký tự", "WEAK_PASSWORD");

            // Xử lý command - throw exception nếu có lỗi
            var account = await _commandHandler.Handle(command);

            if (account == null)
                throw new ConflictException("Email đã tồn tại", "EMAIL_EXISTS");

            // Đăng nhập tự động sau khi đăng ký
            var loginCommand = new LoginCommand
            {
                Email = command.Email,
                Password = command.Password
            };

            var loginResult = await _commandHandler.Handle(loginCommand);

            if (loginResult == null)
                throw new UnauthorizedException("Không thể đăng nhập sau khi đăng ký");

            return CreatedAtAction(
                nameof(Register),
                new { email = command.Email },
                ApiResponse<LoginResponseDto>.Success(
                    loginResult,
                    path,
                    "Đăng ký tài khoản thành công"
                )
            );
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            if (string.IsNullOrWhiteSpace(command.Email))
                throw new BadRequestException("Email không được để trống", "EMPTY_EMAIL");

            if (string.IsNullOrWhiteSpace(command.Password))
                throw new BadRequestException("Mật khẩu không được để trống", "EMPTY_PASSWORD");

            var result = await _commandHandler.Handle(command);

            if (result == null)
                throw new UnauthorizedException("Email hoặc mật khẩu không đúng");

            return Ok(ApiResponse<LoginResponseDto>.Success(
                result,
                path,
                "Đăng nhập thành công"
            ));
        }
    }
}
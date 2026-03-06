using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IService _iService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IService iService, ILogger<AuthController> logger)
    {
        _iService = iService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(LoginDto request)
    {
        var result = await _iService.LoginAsync(request);

        if (result == null)
        {
            return Ok(new ApiResponse<LoginResponseDto>
            {
                StatusCode = 401,
                Message = "Email hoặc mật khẩu không đúng",
                Data = null
            });
        }

        return Ok(new ApiResponse<LoginResponseDto>
        {
            StatusCode = 200,
            Message = "Đăng nhập thành công",
            Data = result
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register(RegisterDto request)
    {
        try
        {
            var result = await _iService.RegisterAsync(request);

            if (!result)
            {
                return Ok(new ApiResponse<LoginResponseDto>
                {
                    StatusCode = 409,
                    Message = "Email đã tồn tại",
                    Data = null
                });
            }

            // Auto login after registration
            var loginResult = await _iService.LoginAsync(new LoginDto
            {
                Email = request.Email,
                Password = request.Password
            });

            return Ok(new ApiResponse<LoginResponseDto>
            {
                StatusCode = 201,
                Message = "Đăng ký tài khoản thành công",
                Data = loginResult
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register error for email {Email}", request.Email);
            return Ok(new ApiResponse<LoginResponseDto>
            {
                StatusCode = 500,
                Message = "Có lỗi xảy ra khi đăng ký",
                Data = null
            });
        }
    }
}
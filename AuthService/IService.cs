using DTOs;

namespace AuthService
{
    public interface IService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto request);
        Task<bool> RegisterAsync(RegisterDto dto);
    }
}

namespace DTOs.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Email hoặc mật khẩu không đúng")
        : base(message, 401, "UNAUTHORIZED")
    {
    }
}
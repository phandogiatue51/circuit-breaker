namespace DTOs.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Bạn không có quyền truy cập")
        : base(message, 403, "FORBIDDEN")
    {
    }
}
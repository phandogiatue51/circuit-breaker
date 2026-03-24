namespace DTOs.Exceptions;

public class BadRequestException : BaseException
{
    public BadRequestException(string message)
        : base(message, 400, "BAD_REQUEST")
    {
    }

    public BadRequestException(string message, string errorCode)
        : base(message, 400, errorCode)
    {
    }
}
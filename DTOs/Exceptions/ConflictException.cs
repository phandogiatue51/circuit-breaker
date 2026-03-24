namespace DTOs.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message)
        : base(message, 409, "CONFLICT")
    {
    }

    public ConflictException(string message, string errorCode)
        : base(message, 409, errorCode)
    {
    }
}
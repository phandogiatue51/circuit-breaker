namespace DTOs.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string resource, object id)
        : base($"{resource} với id '{id}' không tồn tại", 404, "RESOURCE_NOT_FOUND")
    {
    }

    public NotFoundException(string message)
        : base(message, 404, "RESOURCE_NOT_FOUND")
    {
    }
}
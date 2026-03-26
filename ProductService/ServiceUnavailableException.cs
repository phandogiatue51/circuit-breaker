namespace ProductService
{
    public class ServiceUnavailableException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode => 503;

        public ServiceUnavailableException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}

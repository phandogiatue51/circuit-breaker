namespace DTOs
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        //public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow.AddHours(7);

        public string? Path { get; set; }

        public static ApiResponse<T> Success(T data, string path, string message = "Success")
        {
            return new ApiResponse<T>
            {
                StatusCode = 200,
                Message = message,
                Data = data,
                Path = path
            };
        }

        public static ApiResponse<T> Error(int statusCode, string message, string path, string errorCode)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                Data = default,
                Path = path
            };
        }
    }

    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow.AddHours(7);

        public string? Path { get; set; }

        public static ApiResponse Success(string path, string message = "Success")
        {
            return new ApiResponse
            {
                StatusCode = 200,
                Message = message,
                Path = path
            };
        }

        public static ApiResponse Error(int statusCode, string message, string path)
        {
            return new ApiResponse
            {
                StatusCode = statusCode,
                Message = message,
                Path = path
            };
        }
    }
}
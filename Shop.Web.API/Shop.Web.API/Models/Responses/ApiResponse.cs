namespace Shop.Web.API.Models.Responses
{
    /// <summary>
    /// Unified JSON envelope for all API responses.
    /// Angular services should always unwrap this.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
        public string? ErrorCode { get; init; }

        public static ApiResponse<T> Ok(T? data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string message, string? errorCode = null) =>
            new() { Success = false, Message = message, ErrorCode = errorCode };
    }
}

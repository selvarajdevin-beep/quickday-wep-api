namespace Shop.Web.API.Exceptions
{
    /// <summary>
    /// Thrown for known business-rule violations.
    /// Always maps to a user-friendly message — never expose stack traces.
    /// </summary>
    public class AppException : Exception
    {
        public string ErrorCode { get; }
        public int HttpStatus { get; }

        public AppException(
            string message,
            string errorCode = "APP_ERROR",
            int httpStatus = 400)
            : base(message)
        {
            ErrorCode = errorCode;
            HttpStatus = httpStatus;
        }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized. Please log in.")
            : base(message, "UNAUTHORIZED", 401) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "You do not have permission to perform this action.")
            : base(message, "FORBIDDEN", 403) { }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, "NOT_FOUND", 404) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, "CONFLICT", 409) { }
    }
}

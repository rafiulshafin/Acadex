namespace Acadex.Api.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message = "Requested resource not found") : base(message, 404)
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized access") : base(message, 401)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action") : base(message, 403)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message = "Resource conflict") : base(message, 409)
    {
    }
}

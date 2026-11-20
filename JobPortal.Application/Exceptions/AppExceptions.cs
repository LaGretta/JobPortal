namespace JobPortal.Application.Exceptions;

public abstract class AppException : Exception
{
    // Машинний код для фронту. Message читає людина, Code читає код:
    // по ньому фронт вибирає поведінку й не залежить від формулювання.
    public string Code { get; }

    protected AppException(string message, string code) : base(message) => Code = code;
}

public class ConflictException : AppException
{
    public ConflictException(string message, string code = "conflict") : base(message, code) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message, string code = "unauthorized") : base(message, code) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message, string code = "not_found") : base(message, code) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message, string code = "forbidden") : base(message, code) { }
}


public class BadRequestException : AppException
{
    public BadRequestException(string message, string code = "bad_request") : base(message, code) { }
}
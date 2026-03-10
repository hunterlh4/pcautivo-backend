using System.Net;

namespace PCautivoCore.Shared.Responses;

public record Result
{
    public bool IsSuccess { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public Error? Error { get; init; }

    protected Result(bool isSuccess, HttpStatusCode statusCode, Error? error)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Error = error;
    }

    public static Result Success(HttpStatusCode statusCode = HttpStatusCode.OK) => new(true, statusCode, null);

    private static Result Failure(Error error) => new(false, error.StatusCode, error);

    public static Result NoContent() => Success(HttpStatusCode.NoContent);

    public static implicit operator Result(Error error) => Failure(error);
}

public record Result<T> : Result
{
    public T? Value { get; init; }

    private Result(T value, HttpStatusCode statusCode) : base(true, statusCode, null)
    {
        Value = value;
    }

    private Result(Error error) : base(false, error.StatusCode, error)
    {
    }

    public static implicit operator Result<T>(T value) => new(value, HttpStatusCode.OK);

    public static implicit operator Result<T>(Error error) => new(error);

    public static Result<T> Created(T value) => new(value, HttpStatusCode.Created);
}

public record Error(HttpStatusCode StatusCode, string Message);

public static class Results
{
    public static Result<T> Created<T>(T value) => Result<T>.Created(value);
    public static Result NoContent() => Result.NoContent();
}

public static class Errors
{
    public static Error BadRequest(string message = "BadRequest") => new(HttpStatusCode.BadRequest, message);
    public static Error Unauthorized(string message = "Unauthorized") => new(HttpStatusCode.Unauthorized, message);
    public static Error Forbidden(string message = "Forbidden") => new(HttpStatusCode.Forbidden, message);
    public static Error NotFound(string message = "NotFound") => new(HttpStatusCode.NotFound, message);
    public static Error InternalServerError(string message = "InternalServerError") => new(HttpStatusCode.InternalServerError, message);
    public static Error UnprocessableContent(string message = "UnprocessableContent") => new(HttpStatusCode.UnprocessableContent, message);
}
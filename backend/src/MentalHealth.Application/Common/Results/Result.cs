namespace MentalHealth.Application.Common.Results;

public class Result
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; protected init; }
    public ErrorType ErrorType { get; protected init; }
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; protected init; } = NoErrors;

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string error, ErrorType type = ErrorType.Unexpected) =>
        new() { IsSuccess = false, Error = error, ErrorType = type };

    public static Result NotFound(string error) => Failure(error, ErrorType.NotFound);

    public static Result Validation(IReadOnlyDictionary<string, string[]> errors) => new()
    {
        IsSuccess = false,
        Error = "One or more validation errors occurred.",
        ErrorType = ErrorType.Validation,
        ValidationErrors = errors
    };
}

public sealed class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static new Result<T> Failure(string error, ErrorType type = ErrorType.Unexpected) =>
        new() { IsSuccess = false, Error = error, ErrorType = type };

    public static new Result<T> NotFound(string error) =>
        new() { IsSuccess = false, Error = error, ErrorType = ErrorType.NotFound };

    public static new Result<T> Validation(IReadOnlyDictionary<string, string[]> errors) => new()
    {
        IsSuccess = false,
        Error = "One or more validation errors occurred.",
        ErrorType = ErrorType.Validation,
        ValidationErrors = errors
    };
}

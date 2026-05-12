namespace HusqvarnaAutomowerConnect.Core.Errors;

public class OperationResult
{
    protected OperationResult(bool isSuccess, ApplicationError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public ApplicationError? Error { get; }

    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(ApplicationError error) => new(false, error);
}

public sealed class OperationResult<T> : OperationResult
{
    private OperationResult(T? value, bool isSuccess, ApplicationError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static OperationResult<T> Success(T value) => new(value, true, null);

    public static new OperationResult<T> Failure(ApplicationError error) => new(default, false, error);
}


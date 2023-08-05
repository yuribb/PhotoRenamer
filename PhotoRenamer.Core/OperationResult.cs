namespace PhotoRenamer.Core;

public class OperationResult
{
    protected OperationResult()
    {
        this.Success = true;
    }
    protected OperationResult(string message)
    {
        this.Success = false;
        this.FailureMessage = message;
    }
    protected OperationResult(Exception ex)
    {
        this.Success = false;
        this.Exception = ex;
    }
    public bool Success { get; protected set; }
    public string? FailureMessage { get; protected set; }
    public Exception? Exception { get; protected set; }

    public static OperationResult SuccessResult()
    {
        return new OperationResult();
    }

    public static OperationResult FailureResult(string message)
    {
        return new OperationResult(message);
    }

    public static OperationResult ExceptionResult(Exception ex)
    {
        return new OperationResult(ex);
    }
}

public class OperationResult<TResult> : OperationResult
{
    protected OperationResult(TResult result)
    {
        Success = true;
        Result = result;
    }

    public TResult? Result { get; protected set; }

    public static OperationResult<TResult> SuccessResult(TResult result)
    {
        return new OperationResult<TResult>(result);
    }
}

public class StringOperationResult : OperationResult<string>
{
    protected StringOperationResult(string result) : base(result)
    {
    }
    public static new StringOperationResult SuccessResult(string result)
    {
        return new StringOperationResult(result);
    }
}
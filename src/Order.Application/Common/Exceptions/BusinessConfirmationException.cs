namespace WolverineApp.Application.Common.Exceptions;

public class BusinessConfirmationException : Exception
{
    public object? ContextData { get; }

    public BusinessConfirmationException(string message, object? contextData = null)
        : base(message)
    {
        ContextData = contextData;
    }
}

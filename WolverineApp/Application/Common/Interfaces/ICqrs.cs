namespace WolverineApp.Application.Common.Interfaces;

/// <summary>
/// CQRS Marker for Commands that return a result.
/// </summary>
public interface ICommand<out TResponse>
{
}

/// <summary>
/// CQRS Marker for Commands without a result.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// CQRS Marker for Read Queries.
/// </summary>
public interface IQuery<out TResponse>
{
}

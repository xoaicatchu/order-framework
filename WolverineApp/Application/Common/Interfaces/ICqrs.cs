namespace WolverineApp.Application.Common.Interfaces;

/// <summary>
/// Base marker for all mutating CQRS Commands.
/// </summary>
public interface IBaseCommand
{
}

/// <summary>
/// CQRS Marker for Commands that return a result.
/// </summary>
public interface ICommand<out TResponse> : IBaseCommand
{
}

/// <summary>
/// CQRS Marker for Commands without a result.
/// </summary>
public interface ICommand : IBaseCommand
{
}

/// <summary>
/// CQRS Marker for Read Queries.
/// </summary>
public interface IQuery<out TResponse>
{
}

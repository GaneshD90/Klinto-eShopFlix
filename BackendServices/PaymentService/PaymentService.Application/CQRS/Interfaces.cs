namespace PaymentService.Application.CQRS;

/// <summary>
/// Marker interface for queries.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQuery<TResult>
{
}

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommand<TResult>
{
}

/// <summary>
/// Marker interface for commands that don't return a result.
/// </summary>
public interface ICommand : ICommand<Unit>
{
}

/// <summary>
/// Represents a void return type for commands.
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}

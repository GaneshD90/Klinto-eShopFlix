namespace PaymentService.Application.CQRS;

/// <summary>
/// Handler for queries.
/// </summary>
/// <typeparam name="TQuery">The type of query</typeparam>
/// <typeparam name="TResult">The type of result</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken ct = default);
}

/// <summary>
/// Handler for commands.
/// </summary>
/// <typeparam name="TCommand">The type of command</typeparam>
/// <typeparam name="TResult">The type of result</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken ct = default);
}

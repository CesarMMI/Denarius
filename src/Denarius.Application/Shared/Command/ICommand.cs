namespace Denarius.Application.Shared.Command;

public interface ICommand<TQuery, TResult> where TQuery : Query
{
    Task<TResult> Execute(TQuery query);
}
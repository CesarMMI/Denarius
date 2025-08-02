using Denarius.Application.Domain.Queries;

namespace Denarius.Application.Domain.Commands;

public interface ICommand<TQuery, TResult> where TQuery : Query
{
    Task<TResult> Execute(TQuery query);
}
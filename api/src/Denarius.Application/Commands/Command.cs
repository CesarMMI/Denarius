using Denarius.Application.Domain.Commands;
using Denarius.Application.Domain.Queries;

namespace Denarius.Application.Commands;

internal abstract class Command<TQuery, TResult> : ICommand<TQuery, TResult> where TQuery : Query
{
    public Task<TResult> Execute(TQuery query)
    {
        Validate(query);
        return Handle(query);
    }

    protected abstract void Validate(TQuery query);
    protected abstract Task<TResult> Handle(TQuery query);
}
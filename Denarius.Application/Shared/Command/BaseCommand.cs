namespace Denarius.Application.Shared.Command;

public class BaseCommand<TQuery, TResult> where TQuery : BaseQuery
{
    public Task<TResult> Handle(TQuery request)
    {
        request.
    }
}
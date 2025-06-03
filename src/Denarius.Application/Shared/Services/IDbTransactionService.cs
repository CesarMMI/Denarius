namespace Denarius.Application.Shared.Services;

public interface IDbTransactionService
{
    Task ExecuteAsync(Func<Task> action);
    Task<T> ExecuteAsync<T>(Func<Task<T>> action);
}

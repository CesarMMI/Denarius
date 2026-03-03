namespace Denarius.Application.Interfaces;

public interface IUseCase<TCommand, TResult>
{
    TResult Execute(TCommand command);
}

namespace Denarius.Application.UseCases;

public interface IUseCase<T, U>
{
    U Execute(T input);
}
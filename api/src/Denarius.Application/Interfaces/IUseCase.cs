namespace Denarius.Application.Interfaces;

public interface IUseCase<T, U>
{
    U Execute(T input);
}
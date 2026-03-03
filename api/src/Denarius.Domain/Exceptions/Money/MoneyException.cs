namespace Denarius.Domain.Exceptions.Money;

public abstract class MoneyException(string message) : DomainException(message)
{
}
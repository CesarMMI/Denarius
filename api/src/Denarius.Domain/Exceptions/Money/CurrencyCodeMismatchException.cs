namespace Denarius.Domain.Exceptions.Money;

public class CurrencyCodeMismatchException(string operation) : DomainException($"Cannot {operation} money with different currency codes.")
{
}
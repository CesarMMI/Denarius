namespace Denarius.Domain.Exceptions.Money;

public class EmptyCurrencyCodeException() : MoneyException("The currency code cannot be empty.")
{
}
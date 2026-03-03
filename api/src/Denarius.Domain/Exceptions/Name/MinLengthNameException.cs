namespace Denarius.Domain.Exceptions.Name;

public class MinLengthNameException(string errorMessageName, int minLength)
    : NameException($"The {errorMessageName} must be at least {minLength} characters long.")
{
}
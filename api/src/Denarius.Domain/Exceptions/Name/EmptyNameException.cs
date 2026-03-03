namespace Denarius.Domain.Exceptions.Name;

public class EmptyNameException(string errorMessageName) : NameException($"The {errorMessageName} cannot be empty.")
{
}
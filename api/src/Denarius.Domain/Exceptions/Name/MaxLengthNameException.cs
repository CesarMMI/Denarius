namespace Denarius.Domain.Exceptions.Name;

public class MaxLengthNameException(string errorMessageName, int maxLength)
    : NameException($"The {errorMessageName} cannot exceed {maxLength} characters.")
{
}
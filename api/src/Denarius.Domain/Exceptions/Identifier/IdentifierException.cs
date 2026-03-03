namespace Denarius.Domain.Exceptions.Identifier;

public abstract class IdentifierException(string message) : DomainException(message)
{
}
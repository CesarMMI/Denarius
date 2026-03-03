namespace Denarius.Domain.Exceptions.Identifier;

public class EmptyIdentifierException() : IdentifierException("Identifier cannot be empty.")
{
}
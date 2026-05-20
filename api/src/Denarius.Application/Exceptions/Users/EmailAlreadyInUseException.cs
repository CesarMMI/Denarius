namespace Denarius.Application.Exceptions.Users;

public class EmailAlreadyInUseException(string email)
    : AppException($"Email '{email}' is already in use.");
